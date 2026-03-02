using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using AbstractPixel.Core;


namespace AbstractPixel.SaveSystem
{
    /// <summary>Provides a bridge component for managing the saving and restoring of state for associated saveable targets
    /// within a Unity GameObject. Implements the ISavableBridge interface to support category-based serialization and
    /// deserialization of component data.</summary>
    /// <remarks>Attach this component to a GameObject to enable save and restore functionality for all
    /// components on the GameObject that implement the ISaveable<T> interface and are marked with the
    /// SaveableAttribute. The SavableBridge automatically discovers eligible components and manages their unique
    /// identification and registration with the SaveManager.</remarks>
    public class SavableBridge : MonoBehaviour, ISavableBridge
    {
        [field: SerializeField, ReadOnly(true)] public string UniqueId { get; private set; }
        [SerializeField, HideInInspector] string lastKnownGlobalObjectID = "";
        [SerializeField, HideInInspector] string lastKnownGameObjectName;

        [SerializeField, ReadOnly] private List<SaveableTarget> savableTargets = new List<SaveableTarget>();
        private List<SaveCategory> foundCategoriesList = new List<SaveCategory>();

        private Dictionary<SaveCategory, List<SaveableTarget>> savableTargetsRegistry;

        readonly string isavableCaptureMethod = "CaptureData";
        readonly string isavableRestoreMethod = "RestoreData";
        readonly string stringSeparatorIdentifier = "#";

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) return;

            UnityEditor.EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            UnityEditor.EditorApplication.hierarchyChanged += OnHierarchyChanged;

            ValidateIdentity();
            SyncSavableTargets();
        }

        private void ValidateIdentity()
        {
            string currentGlobalId = UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(this).ToString();
            bool isNewOrDuplicated = lastKnownGlobalObjectID != currentGlobalId || string.IsNullOrEmpty(UniqueId);
            bool isRenamed = lastKnownGameObjectName != gameObject.name;

            if (isNewOrDuplicated)
            {
                string newGuid = Guid.NewGuid().ToString();
                UniqueId = $"{gameObject.name} GameObject {stringSeparatorIdentifier}[{newGuid}]";

                lastKnownGlobalObjectID = currentGlobalId;
                lastKnownGameObjectName = gameObject.name;
                UnityEditor.EditorUtility.SetDirty(this);
            }
            else if (isRenamed)
            {
                int separatorIndex = UniqueId.IndexOf(stringSeparatorIdentifier);
                if (separatorIndex >= 0)
                {
                    // Extract everything from the '#' to the end
                    string pureGuidPart = UniqueId.Substring(separatorIndex);

                    // Stitch the new name with the old GUID
                    UniqueId = $"{gameObject.name} GameObject {pureGuidPart}";
                    lastKnownGameObjectName = gameObject.name;

                    UnityEditor.EditorUtility.SetDirty(this);
                }
            }
        }
        private void OnHierarchyChanged()
        {
            if (Application.isPlaying || this == null) return;

            // Performance check: Only run the heavy logic if OUR name actually changed
            if (lastKnownGameObjectName != gameObject.name)
            {
                ValidateIdentity();
            }
        }

        private void SyncSavableTargets()
        {
            if (savableTargets == null) savableTargets = new List<SaveableTarget>();

            MonoBehaviour[] allScripts = GetComponents<MonoBehaviour>();
            List<MonoBehaviour> validScripts = new List<MonoBehaviour>();

            foreach (MonoBehaviour script in allScripts)
            {
                if (script == null) continue;
                Type type = script.GetType();
                if (type.GetInterface(typeof(ISavable<>).Name) == null) continue;
                if (type.GetCustomAttribute<SaveableAttribute>() == null) continue;
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
                SaveableTarget existingTarget = savableTargets.FirstOrDefault(t => t.Script == script);

                if (existingTarget != null)
                {
                    if (existingTarget.Identification != null && existingTarget.Identification.ClassName != script.GetType().Name)
                    {
                        // This handles the case where a script was renamed. We keep the same GUID but update the class name for clarity.
                        existingTarget.Identification.ClassName = script.GetType().Name;
                        existingTarget.InpsectorName = script.GetType().Name;
                        UnityEditor.EditorUtility.SetDirty(this);
                    }
                }
                else
                {
                    string newGuid = Guid.NewGuid().ToString();
                    SaveableIdentification id = new SaveableIdentification(script.GetType().Name, newGuid);
                    savableTargets.Add(new SaveableTarget(script, id));
                    UnityEditor.EditorUtility.SetDirty(this);
                }
            }
        }

#endif

        private void Awake()
        {
            savableTargetsRegistry = new Dictionary<SaveCategory, List<SaveableTarget>>();
            foundCategoriesList = new List<SaveCategory>();

            foreach (SaveableTarget target in savableTargets)
            {
                if (target == null || target.Script == null) continue;

                Type componentType = target.Script.GetType();
                SaveableAttribute attribute = componentType.GetCustomAttribute<SaveableAttribute>();
                if (attribute == null) continue;

                Type interfaceType = componentType.GetInterface(typeof(ISavable<>).Name);
                if (interfaceType == null) continue;

                target.CaptureDataMethod = componentType.GetMethod(isavableCaptureMethod);
                target.RestoreDataMethod = componentType.GetMethod(isavableRestoreMethod);
                target.DataToSaveType = interfaceType.GetGenericArguments()[0];

                if (!savableTargetsRegistry.ContainsKey(attribute.Category))
                {
                    savableTargetsRegistry.Add(attribute.Category, new List<SaveableTarget>());
                }
                savableTargetsRegistry[attribute.Category].Add(target);

                if (!foundCategoriesList.Contains(attribute.Category))
                {
                    foundCategoriesList.Add(attribute.Category);
                }
            }
        }

        public object CaptureState(SaveCategory categoryFilter)
        {
            Dictionary<string, object> combinedCapturedDataMap = new Dictionary<string, object>();

            if (!savableTargetsRegistry.TryGetValue(categoryFilter, out List<SaveableTarget> saveableTargetsList))
            {
                return null;
            }

            foreach (SaveableTarget target in saveableTargetsList)
            {
                object capturedData = target.CaptureDataMethod.Invoke(target.Script, null);
                if (capturedData != null)
                {
                    if (target.Identification != null && !string.IsNullOrEmpty(target.Identification.GUID))
                    {
                        string compositeKey = $"{target.Identification.ClassName} Component {stringSeparatorIdentifier}{target.Identification.GUID}";
                        combinedCapturedDataMap.Add(compositeKey, capturedData);
                    }
                }
            }

            return combinedCapturedDataMap;
        }

        public void RestoreState(object data, SaveCategory categoryFilter)
        {
            Dictionary<string, object> combinedCapturedDataMap = SaveDataConverter.Convert<Dictionary<string, object>>(data);
            if (combinedCapturedDataMap == null) return;

            if (!savableTargetsRegistry.TryGetValue(categoryFilter, out List<SaveableTarget> targetsList))
            {
                return;
            }

            Dictionary<string, SaveableTarget> guidToTargetMap = new Dictionary<string, SaveableTarget>();
            foreach (SaveableTarget target in targetsList)
            {
                if (target.Identification != null && !string.IsNullOrEmpty(target.Identification.GUID))
                {
                    if (!guidToTargetMap.ContainsKey(target.Identification.GUID))
                    {
                        guidToTargetMap.Add(target.Identification.GUID, target);
                    }
                }
            }

            foreach (KeyValuePair<string, object> kvp in combinedCapturedDataMap)
            {
                string compositeKey = kvp.Key;

                // Extract GUID from "ClassName#GUID"
                // We split by the LAST '#' to ensure we get the GUID at the end.
                int separatorIndex = compositeKey.LastIndexOf(stringSeparatorIdentifier) + 1;

                string extractedGuid = (separatorIndex != -1) ? compositeKey.Substring(separatorIndex) : compositeKey;

                if (guidToTargetMap.TryGetValue(extractedGuid, out SaveableTarget target))
                {
                    object typedData = SaveDataConverter.Convert(kvp.Value, target.DataToSaveType);
                    target.RestoreDataMethod.Invoke(target.Script, new object[] { typedData });
                }
            }
        }

        private void OnEnable()
        {
            if (foundCategoriesList.Count > 0)
            {
                SaveManager.Instance.RegisterSavableObject(this, foundCategoriesList);
            }
        }

        private void OnDisable()
        {
            if (SaveManager.Instance != null && foundCategoriesList.Count > 0)
            {
                SaveManager.Instance.UnregisterSavableObject(this, foundCategoriesList);
            }
        }
    }
}