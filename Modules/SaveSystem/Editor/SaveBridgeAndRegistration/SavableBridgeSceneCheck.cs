using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// Finds a scene's saved scripts with no Savable Bridge. Shared by the play mode check and the build guard,
    /// so both report the same scenes and objects the same way. A bridge inherited from a prefab asset counts,
    /// since GetComponent already returns it.
    /// </summary>
    internal static class SavableBridgeSceneCheck
    {
        public static List<GameObject> FindMissingBridges(Scene scene)
        {
            List<GameObject> missing = new List<GameObject>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
                {
                    GameObject gameObject = transform.gameObject;
                    if (gameObject.GetComponent<SavableBridge>() != null)
                    {
                        continue;
                    }
                    bool hasSavedScript = gameObject.GetComponents<MonoBehaviour>().Any(script => script != null && SavedScript.IsSaved(script.GetType()));
                    if (hasSavedScript)
                    {
                        missing.Add(gameObject);
                    }
                }
            }
            return missing;
        }

        public static string Describe(Scene scene, List<GameObject> missing, string nextStep)
        {
            string names = string.Join(", ", missing.Select(gameObject => gameObject.name));
            return $"[SaveSystem] {scene.name} has saved scripts with no Savable Bridge yet ({names}). Open {scene.name} once in the Editor so its Savable Bridges are added, then {nextStep}.";
        }
    }
}
