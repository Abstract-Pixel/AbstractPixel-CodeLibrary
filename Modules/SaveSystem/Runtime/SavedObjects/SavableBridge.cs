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
    public class SavableBridge : MonoBehaviour, ISavableBridge
    {
        [field: SerializeField, ReadOnly(true)] public string UniqueId { get; private set; }
        [SerializeField, HideInInspector] string lastKnownGlobalObjectID = "";

        [SerializeField, ReadOnly] private List<SavableTarget> savableTargets = new List<SavableTarget>();

        readonly string isavableCaptureMethod = "CaptureData";
        readonly string isavableRestoreMethod = "RestoreData";
        readonly string stringSeparatorIdentifier = "#";

        private bool hasStarted;

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
            if (SavableBridgeIdentity.NeedsNewGuid(UniqueId, lastKnownGlobalObjectID, currentGlobalId))
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

        private void SyncSavableTargets()
        {
            if (savableTargets == null) savableTargets = new List<SavableTarget>();

            MonoBehaviour[] allScripts = GetComponents<MonoBehaviour>();
            List<MonoBehaviour> validScripts = new List<MonoBehaviour>();

            foreach (MonoBehaviour script in allScripts)
            {
                if (script == null) continue;
                Type type = script.GetType();
                if (type.GetInterface(typeof(ISavable<>).Name) == null) continue;
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

                Type interfaceType = target.Script.GetType().GetInterface(typeof(ISavable<>).Name);
                if (interfaceType == null) continue;

                target.CaptureDataMethod = interfaceType.GetMethod(isavableCaptureMethod);
                target.RestoreDataMethod = interfaceType.GetMethod(isavableRestoreMethod);
                target.DataToSaveType = interfaceType.GetGenericArguments()[0];
            }
        }

        // Registering here, not in OnEnable, means every Awake has run, so restored data isn't overwritten by new-game values.
        private void Start()
        {
            hasStarted = true;
            RegisterTargets();
        }

        // Covers objects that were disabled and enabled again after their first Start.
        private void OnEnable()
        {
            if (hasStarted)
            {
                RegisterTargets();
            }
        }

        // Unregistering captures the scripts' current data first, so nothing is lost when the object goes away.
        private void OnDisable()
        {
            SaveActions.Unregister(this);
        }

        private void RegisterTargets()
        {
            SaveSession session = SaveActions.Session;
            if (session == null) return;

            foreach (SavableTarget target in savableTargets)
            {
                if (target == null || target.Script == null || target.CaptureDataMethod == null || target.Identification == null) continue;

                // The object's own scene, so with additive scenes a chest saves into its scene's file, not the active scene's.
                session.RegisterScript(this, target.Script, target.Script.GetType(), GetSaveId(target), target.DataToSaveType,
                    () => InvokeScript(target.CaptureDataMethod, target.Script, null),
                    data => InvokeScript(target.RestoreDataMethod, target.Script, new[] { data }),
                    gameObject.scene.name, GetEntryName(target), TurnOffBecauseForgotten);

                // A gone marker turned this object off; the rest of its scripts stay unregistered.
                if (!gameObject.activeSelf)
                {
                    return;
                }
            }
        }

        // Called from Start when a save ID has a gone marker. This bridge runs first, so the other scripts on this
        // GameObject never reach their Start (their Awake has already run).
        private void TurnOffBecauseForgotten()
        {
            SaveActions.Session?.DropWithoutCapturing(this);
            gameObject.SetActive(false);
        }

        /// <summary>The save ID of one saved script on this GameObject: "&lt;bridge GUID&gt;/&lt;script GUID&gt;".</summary>
        private string GetSaveId(SavableTarget target)
        {
            int separatorIndex = UniqueId.LastIndexOf(stringSeparatorIdentifier, StringComparison.Ordinal);
            string bridgeGuid = separatorIndex >= 0 ? UniqueId.Substring(separatorIndex + 1) : UniqueId;
            return $"{bridgeGuid.Trim('[', ']')}/{target.Identification.GUID}";
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