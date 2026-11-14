using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// Watches for a saved script showing up while editing, so SavableBridgeAdder can give it a bridge at once:
    /// right away for Add Component, paste, dragging a prefab in and duplicating (ObjectChangeEvents), and after
    /// each compile for a script that just gained its first saved field (open scenes, the open Prefab Stage, and
    /// every prefab asset that uses a saved script class).
    /// </summary>
    [InitializeOnLoad]
    internal static class SavableBridgeEditTimeWatcher
    {
        const string SavedScriptClassesSessionKey = "AbstractPixel.SaveSystem.SavedScriptClasses";

        static SavableBridgeEditTimeWatcher()
        {
            ObjectChangeEvents.changesPublished += OnChangesPublished;
        }

        [DidReloadScripts]
        static void OnScriptsReloaded()
        {
            // Waiting one editor update lets the asset database finish loading before anything is scanned.
            EditorApplication.delayCall += AfterCompile;
        }

        static void AfterCompile()
        {
            ScanOpenScenesAndPrefabStage();
            ScanPrefabAssetsIfSavedClassesChanged();
        }

        static void OnChangesPublished(ref ObjectChangeEventStream stream)
        {
            List<GameObject> changedObjects = new List<GameObject>();
            for (int index = 0; index < stream.length; index++)
            {
                switch (stream.GetEventType(index))
                {
                    case ObjectChangeKind.ChangeGameObjectStructure:
                        stream.GetChangeGameObjectStructureEvent(index, out ChangeGameObjectStructureEventArgs structureArgs);
#if UNITY_7000_0_OR_NEWER
                        AddGameObjectIfValid(structureArgs.entityId, changedObjects);
#else
                        AddGameObjectIfValid(structureArgs.instanceId, changedObjects);
#endif
                        break;
                    case ObjectChangeKind.ChangeGameObjectStructureHierarchy:
                        stream.GetChangeGameObjectStructureHierarchyEvent(index, out ChangeGameObjectStructureHierarchyEventArgs hierarchyArgs);
#if UNITY_7000_0_OR_NEWER
                        AddHierarchyIfValid(hierarchyArgs.entityId, changedObjects);
#else
                        AddHierarchyIfValid(hierarchyArgs.instanceId, changedObjects);
#endif
                        break;
                    case ObjectChangeKind.CreateGameObjectHierarchy:
                        stream.GetCreateGameObjectHierarchyEvent(index, out CreateGameObjectHierarchyEventArgs createArgs);
#if UNITY_7000_0_OR_NEWER
                        AddHierarchyIfValid(createArgs.entityId, changedObjects);
#else
                        AddHierarchyIfValid(createArgs.instanceId, changedObjects);
#endif
                        break;
                }
            }

            if (changedObjects.Count > 0)
            {
                SavableBridgeAdder.EnsureBridges(changedObjects);
            }
        }

#if UNITY_7000_0_OR_NEWER
        static void AddGameObjectIfValid(EntityId entityId, List<GameObject> changedObjects)
        {
            if (EditorUtility.EntityIdToObject(entityId) is GameObject gameObject)
            {
                changedObjects.Add(gameObject);
            }
        }

        // A hierarchy event's id is the hierarchy's root; duplicating, pasting or dragging a prefab in can bring
        // several saved scripts at once, on the root or any child.
        static void AddHierarchyIfValid(EntityId entityId, List<GameObject> changedObjects)
        {
            if (EditorUtility.EntityIdToObject(entityId) is GameObject root)
            {
                changedObjects.AddRange(AllGameObjectsUnder(root));
            }
        }
#else
        static void AddGameObjectIfValid(int instanceId, List<GameObject> changedObjects)
        {
            if (EditorUtility.InstanceIDToObject(instanceId) is GameObject gameObject)
            {
                changedObjects.Add(gameObject);
            }
        }

        // A hierarchy event's id is the hierarchy's root; duplicating, pasting or dragging a prefab in can bring
        // several saved scripts at once, on the root or any child.
        static void AddHierarchyIfValid(int instanceId, List<GameObject> changedObjects)
        {
            if (EditorUtility.InstanceIDToObject(instanceId) is GameObject root)
            {
                changedObjects.AddRange(AllGameObjectsUnder(root));
            }
        }
#endif

        static void ScanOpenScenesAndPrefabStage()
        {
            List<GameObject> candidates = new List<GameObject>();
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                if (!scene.isLoaded)
                {
                    continue;
                }
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    candidates.AddRange(AllGameObjectsUnder(root));
                }
            }

            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                candidates.AddRange(AllGameObjectsUnder(prefabStage.prefabContentsRoot));
            }

            if (candidates.Count > 0)
            {
                SavableBridgeAdder.EnsureBridges(candidates);
            }
        }

        // Runs only when the saved script classes changed since the last compile, so a project with many prefabs
        // isn't rescanned on every unrelated compile. The key survives domain reloads but not an Editor restart.
        static void ScanPrefabAssetsIfSavedClassesChanged()
        {
            List<Type> savedClasses = FindSavedScriptClasses();
            string currentKey = string.Join(",", savedClasses.Select(type => type.FullName).OrderBy(name => name, StringComparer.Ordinal));
            if (currentKey == SessionState.GetString(SavedScriptClassesSessionKey, string.Empty))
            {
                return;
            }
            SessionState.SetString(SavedScriptClassesSessionKey, currentKey);

            HashSet<string> scriptPaths = new HashSet<string>(savedClasses.Select(FindScriptPath).Where(path => path != null));
            if (scriptPaths.Count == 0)
            {
                return;
            }

            List<GameObject> candidates = new List<GameObject>();
            foreach (string prefabGuid in AssetDatabase.FindAssets("t:Prefab"))
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
                if (!AssetDatabase.GetDependencies(prefabPath, false).Any(scriptPaths.Contains))
                {
                    continue;
                }
                GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefabRoot != null)
                {
                    candidates.AddRange(AllGameObjectsUnder(prefabRoot));
                }
            }

            if (candidates.Count > 0)
            {
                SavableBridgeAdder.EnsureBridges(candidates);
            }
        }

        static List<Type> FindSavedScriptClasses()
        {
            return TypeCache.GetTypesDerivedFrom<MonoBehaviour>()
                .Where(type => !type.IsAbstract && SavedScript.IsSaved(type))
                .ToList();
        }

        static string FindScriptPath(Type type)
        {
            foreach (string guid in AssetDatabase.FindAssets($"t:MonoScript {type.Name}"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script != null && script.GetClass() == type)
                {
                    return path;
                }
            }
            return null;
        }

        static IEnumerable<GameObject> AllGameObjectsUnder(GameObject root)
        {
            return root.GetComponentsInChildren<Transform>(true).Select(transform => transform.gameObject);
        }
    }
}
