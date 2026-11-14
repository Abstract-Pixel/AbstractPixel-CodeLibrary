using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// Adds a Savable Bridge to a GameObject with a saved script and none, while editing: SavableBridgeEditTimeWatcher
    /// calls this right after a saved script is added and after each compile. Never at play time or build time;
    /// SavableBridgeSceneBuildGuard and SavableBridgePlayModeCheck report a scene that still needs one instead.
    /// </summary>
    internal static class SavableBridgeAdder
    {
        // Remembers, for this Editor session, which GameObject already had a bridge the last time this ran. Lets a
        // bridge that comes back (the owner removed it, but a saved script still needs it) say so, instead of
        // reading the same as a script getting its first bridge ever. Reset is harmless: it only downgrades the
        // warning to a plain "added" log once per domain reload.
        static readonly HashSet<GameObject> knownToHaveABridge = new HashSet<GameObject>();

        /// <summary>Adds a bridge to every GameObject among these that has a saved script and none, and logs what it did.</summary>
        public static void EnsureBridges(IEnumerable<GameObject> gameObjects)
        {
            List<(string Context, string Name)> added = new List<(string Context, string Name)>();
            List<string> broughtBack = new List<string>();
            HashSet<Scene> scenesToDirty = new HashSet<Scene>();
            HashSet<GameObject> prefabRootsToSave = new HashSet<GameObject>();

            foreach (GameObject gameObject in gameObjects)
            {
                if (gameObject != null)
                {
                    EnsureBridgeOn(gameObject, added, broughtBack, scenesToDirty, prefabRootsToSave);
                }
            }

            foreach (Scene scene in scenesToDirty)
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }
            foreach (GameObject prefabRoot in prefabRootsToSave)
            {
                PrefabUtility.SavePrefabAsset(prefabRoot);
            }

            foreach (IGrouping<string, (string Context, string Name)> group in added.GroupBy(entry => entry.Context))
            {
                string names = string.Join(" and ", group.Select(entry => entry.Name));
                Debug.Log($"[SaveSystem] Added a Savable Bridge to {names} in {group.Key}.");
            }
            foreach (string name in broughtBack)
            {
                Debug.LogWarning($"[SaveSystem] {name} still saves, so it needs its Savable Bridge. It came back with a new ID, so this object's save data from before no longer loads under it. Remove the saved scripts first if you don't want {name} saved.");
            }
        }

        static void EnsureBridgeOn(GameObject gameObject, List<(string Context, string Name)> added, List<string> broughtBack, HashSet<Scene> scenesToDirty, HashSet<GameObject> prefabRootsToSave)
        {
            if (gameObject.GetComponent<SavableBridge>() != null)
            {
                knownToHaveABridge.Add(gameObject);
                return;
            }
            if (!HasAnySavedScript(gameObject))
            {
                knownToHaveABridge.Remove(gameObject);
                return;
            }

            // A saved script inherited from the prefab asset gets its bridge there, so every instance and every
            // spawned copy has it. A saved script added as an override on only this instance gets it here instead.
            GameObject bridgeTarget = gameObject;
            if (PrefabUtility.IsPartOfPrefabInstance(gameObject))
            {
                GameObject correspondingAssetObject = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
                if (correspondingAssetObject != null && HasAnySavedScript(correspondingAssetObject))
                {
                    bridgeTarget = correspondingAssetObject;
                }
            }
            if (bridgeTarget.GetComponent<SavableBridge>() != null)
            {
                // Another instance already routed the asset to a bridge this pass.
                return;
            }

            bool isComingBack = knownToHaveABridge.Remove(gameObject);
            bool isPrefabAssetTarget = bridgeTarget != gameObject;

            if (isPrefabAssetTarget)
            {
                bridgeTarget.AddComponent<SavableBridge>();
                EditorUtility.SetDirty(bridgeTarget);
                prefabRootsToSave.Add(bridgeTarget.transform.root.gameObject);
            }
            else
            {
                Undo.AddComponent<SavableBridge>(gameObject);
                EditorUtility.SetDirty(gameObject);
                if (gameObject.scene.IsValid())
                {
                    scenesToDirty.Add(gameObject.scene);
                }
                else
                {
                    // A prefab asset open in Prefab Mode: its stage scene never reports IsValid, but the asset still needs saving.
                    prefabRootsToSave.Add(gameObject.transform.root.gameObject);
                }
            }

            knownToHaveABridge.Add(gameObject);
            if (isComingBack)
            {
                broughtBack.Add(gameObject.name);
            }
            else
            {
                added.Add((ContextNameFor(gameObject, bridgeTarget, isPrefabAssetTarget), gameObject.name));
            }
        }

        static string ContextNameFor(GameObject gameObject, GameObject bridgeTarget, bool isPrefabAssetTarget)
        {
            if (isPrefabAssetTarget)
            {
                return PrefabNameOf(bridgeTarget);
            }
            if (gameObject.scene.IsValid())
            {
                return gameObject.scene.name;
            }
            return PrefabNameOf(gameObject);
        }

        static string PrefabNameOf(GameObject prefabGameObject)
        {
            GameObject root = prefabGameObject.transform.root.gameObject;
            string path = AssetDatabase.GetAssetPath(root);
            return string.IsNullOrEmpty(path) ? root.name : Path.GetFileNameWithoutExtension(path);
        }

        static bool HasAnySavedScript(GameObject gameObject)
        {
            return gameObject.GetComponents<MonoBehaviour>().Any(script => script != null && SavedScript.IsSaved(script.GetType()));
        }
    }
}
