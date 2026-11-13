using AbstractPixel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using UnityEngine;


namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// The Savable Bridge: connects the saved scripts on this GameObject to the save system. A saved script implements
    /// ISavable&lt;TData&gt; or ISavable&lt;TData, TFile&gt;. The editor finds those scripts and gives each one a save ID.
    /// </summary>
    // Runs before other scripts, so registering in Start restores data after every Awake and before any other Start.
    [DefaultExecutionOrder(-10000)]
    public class SavableBridge : MonoBehaviour
    {
        [field: SerializeField, ReadOnly(true)] public string UniqueId { get; private set; }
        [SerializeField, HideInInspector] string lastKnownGlobalObjectID = "";

        [SerializeField, ReadOnly] private List<SavableTarget> savableTargets = new List<SavableTarget>();

        /// <summary>This GameObject's saved scripts, for the bridge inspector's read-only list and Copy save ID button.</summary>
        internal IReadOnlyList<SavableTarget> SavableTargets => savableTargets;

        readonly string isavableCaptureMethod = "CaptureData";
        readonly string isavableRestoreMethod = "RestoreData";
        readonly string stringSeparatorIdentifier = "#";

        private bool hasStarted;
        private bool isRegistered;

        // Set by SetSaveId, for a spawned copy. Null means "use this bridge's own UniqueId", the scene-placed case.
        private string spawnedCopySaveId;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) return;

            ValidateIdentity();
            SyncSavableTargets();
        }

        // The save ID uses only the GUID, so renaming the GameObject never changes it and needs no handling here.
        private void ValidateIdentity()
        {
            string currentGlobalId = UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(this).ToString();
            bool needsNewGuid = SavableBridgeIdentity.NeedsNewGuid(UniqueId, lastKnownGlobalObjectID, currentGlobalId)
                || SavableBridgeIdentity.IsDuplicateOfAnotherBridge(UniqueId, currentGlobalId, SiblingBridgeIdentities());
            if (needsNewGuid)
            {
                string newGuid = Guid.NewGuid().ToString();
                UniqueId = $"{gameObject.name} GameObject {stringSeparatorIdentifier}[{newGuid}]";
                UnityEditor.EditorUtility.SetDirty(this);
            }

            // A scene that was never saved gives every object the same id, so it is only remembered once the scene has a path.
            bool isWorthRemembering = SavableBridgeIdentity.IsFromASavedScene(currentGlobalId) && lastKnownGlobalObjectID != currentGlobalId;
            if (isWorthRemembering)
            {
                lastKnownGlobalObjectID = currentGlobalId;
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }

        // Other Savable Bridges in this GameObject's scene, so ValidateIdentity can tell a copy from the original
        // once the scene is first saved and their GlobalObjectIds finally differ (B1).
        private IEnumerable<(string UniqueId, string GlobalObjectId)> SiblingBridgeIdentities()
        {
            foreach (SavableBridge bridge in FindObjectsByType<SavableBridge>(FindObjectsSortMode.None))
            {
                if (bridge == this || bridge == null || bridge.gameObject.scene != gameObject.scene) continue;
                yield return (bridge.UniqueId, UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(bridge).ToString());
            }
        }

        private void SyncSavableTargets()
        {
            if (savableTargets == null) savableTargets = new List<SavableTarget>();

            MonoBehaviour[] allScripts = GetComponents<MonoBehaviour>();
            List<MonoBehaviour> validScripts = new List<MonoBehaviour>();

            foreach (MonoBehaviour script in allScripts)
            {
                if (script == null) continue;
                if (!SavedScript.IsSaved(script.GetType())) continue;
                validScripts.Add(script);
            }

            //Clean up removed scripts
            for (int i = savableTargets.Count - 1; i >= 0; i--)
            {
                if (savableTargets[i] == null || savableTargets[i].Script == null || !validScripts.Contains(savableTargets[i].Script))
                {
                    savableTargets.RemoveAt(i);
                    UnityEditor.EditorUtility.SetDirty(this);
                }
            }

            // Add or Update Targets
            foreach (MonoBehaviour script in validScripts)
            {
                SavableTarget existingTarget = savableTargets.FirstOrDefault(t => t.Script == script);

                if (existingTarget != null)
                {
                    if (existingTarget.Identification != null && existingTarget.Identification.ClassName != script.GetType().Name)
                    {
                        // This handles the case where a script was renamed. We keep the same GUID but update the class name for clarity.
                        existingTarget.Identification.ClassName = script.GetType().Name;
                        existingTarget.InspectorName = script.GetType().Name;
                        UnityEditor.EditorUtility.SetDirty(this);
                    }
                }
                else
                {
                    string newGuid = Guid.NewGuid().ToString();
                    SavableIdentification id = new SavableIdentification(script.GetType().Name, newGuid);
                    savableTargets.Add(new SavableTarget(script, id));
                    UnityEditor.EditorUtility.SetDirty(this);
                }
            }
        }

#endif

        // Reflection is looked up once per saved script, here, and kept on this bridge instance.
        private void Awake()
        {
            foreach (SavableTarget target in savableTargets)
            {
                if (target == null || target.Script == null) continue;

                Type interfaceType = SavedScript.FindSavableInterface(target.Script.GetType());
                if (interfaceType != null)
                {
                    target.CaptureDataMethod = interfaceType.GetMethod(isavableCaptureMethod);
                    target.RestoreDataMethod = interfaceType.GetMethod(isavableRestoreMethod);
                    target.DataToSaveType = interfaceType.GetGenericArguments()[0];
                }
                else
                {
                    target.Fields = new SavedFieldSet(target.Script.GetType());
                }
            }
        }

        // Registering here, not in OnEnable, means every Awake has run, so restored data isn't overwritten by new-game values.
        private void Start()
        {
            hasStarted = true;
            TryRegisterTargets();
        }

        // Covers objects that were disabled and enabled again after their first Start.
        private void OnEnable()
        {
            if (hasStarted)
            {
                TryRegisterTargets();
            }
        }

        // Unregistering captures the scripts' current data first, so nothing is lost when the object goes away.
        private void OnDisable()
        {
            SaveActions.Unregister(this);
            isRegistered = false;
        }

        /// <summary>
        /// Gives a spawned copy its own save ID, so it doesn't share its prefab's. Call it right after Instantiate,
        /// before the copy's Start; the bridge waits for it instead of registering with the prefab's shared ID.
        /// </summary>
        public void SetSaveId(string saveId)
        {
            if (string.IsNullOrEmpty(saveId))
            {
                Debug.LogError($"[SaveSystem] SetSaveId on '{gameObject.name}' needs a non-empty ID.");
                return;
            }
            spawnedCopySaveId = saveId;
            TryRegisterTargets();
        }

        private void TryRegisterTargets()
        {
            if (isRegistered) return;

            if (spawnedCopySaveId == null && IsSpawnedCopyWithNoIdYet())
            {
                Debug.LogWarning($"[SaveSystem] '{gameObject.name}' was spawned after its scene was already loaded, with no SetSaveId call, so it isn't saved. Call its SetSaveId right after Instantiate, before Start: goblin.SetSaveId(\"a unique id\");");
                return;
            }

            RegisterTargets();
            isRegistered = true;
        }

        // A Savable Bridge Awoken after its own scene already became ready is a spawned copy, not one of the
        // scene's own objects: every scene object's Awake runs before its scene's SceneReady fires.
        private bool IsSpawnedCopyWithNoIdYet()
        {
            return SaveActions.Session?.IsSceneAlreadyReady(gameObject.scene.name) ?? false;
        }

        private void RegisterTargets()
        {
            SaveSession session = SaveActions.Session;
            if (session == null) return;

            // A spawned copy's data is forgotten outright; it has no gone marker to turn itself off with.
            Action turnOffSceneObject = spawnedCopySaveId == null ? TurnOffBecauseForgotten : null;

            foreach (SavableTarget target in savableTargets)
            {
                if (target == null || target.Script == null || target.Identification == null) continue;
                if (!RegisterOneTarget(session, target, turnOffSceneObject)) continue;

                // A gone marker turned this object off; the rest of its scripts stay unregistered.
                if (!gameObject.activeSelf)
                {
                    return;
                }
            }
        }

        // The object's own scene, so with additive scenes a chest saves into its scene's file, not the active scene's.
        // Returns false for a target Awake couldn't reflect, which leaves it unregistered.
        private bool RegisterOneTarget(SaveSession session, SavableTarget target, Action turnOffSceneObject)
        {
            if (target.CaptureDataMethod != null)
            {
                session.RegisterScript(this, target.Script, target.Script.GetType(), GetSaveId(target), target.DataToSaveType,
                    () => InvokeScript(target.CaptureDataMethod, target.Script, null),
                    data => InvokeScript(target.RestoreDataMethod, target.Script, new[] { data }),
                    gameObject.scene.name, GetEntryName(target), turnOffSceneObject);
                return true;
            }
            if (target.Fields != null)
            {
                session.RegisterSavedFieldScript(this, target.Script, target.Fields, GetSaveId(target),
                    gameObject.scene.name, GetEntryName(target), turnOffSceneObject);
                return true;
            }
            return false;
        }

        // Called from Start when a save ID has a gone marker. This bridge runs first, so the other scripts on this
        // GameObject never reach their Start (their Awake has already run).
        private void TurnOffBecauseForgotten()
        {
            SaveActions.Session?.SavedObjects.DropWithoutCapturing(this);
            gameObject.SetActive(false);
        }

        /// <summary>The save ID of one saved script on this GameObject: "&lt;bridge GUID or SetSaveId&gt;/&lt;script GUID&gt;".</summary>
        internal string GetSaveId(SavableTarget target)
        {
            string bridgeGuid = spawnedCopySaveId ?? DefaultBridgeGuid();
            return $"{bridgeGuid}/{target.Identification.GUID}";
        }

        private string DefaultBridgeGuid()
        {
            int separatorIndex = UniqueId.LastIndexOf(stringSeparatorIdentifier, StringComparison.Ordinal);
            string bridgeGuid = separatorIndex >= 0 ? UniqueId.Substring(separatorIndex + 1) : UniqueId;
            return bridgeGuid.Trim('[', ']');
        }

        // A readable name for people reading the save file. Matching uses only the save ID.
        private string GetEntryName(SavableTarget target)
        {
            string scenePath = string.IsNullOrEmpty(gameObject.scene.path) ? gameObject.scene.name : gameObject.scene.path;
            return $"{scenePath}/{gameObject.name}/{target.Script.GetType().Name}";
        }

        // Reflection wraps a script's exception; unwrap it so the save system logs the script's own message.
        private static object InvokeScript(MethodInfo method, object script, object[] arguments)
        {
            try
            {
                return method.Invoke(script, arguments);
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                throw;
            }
        }
    }
}