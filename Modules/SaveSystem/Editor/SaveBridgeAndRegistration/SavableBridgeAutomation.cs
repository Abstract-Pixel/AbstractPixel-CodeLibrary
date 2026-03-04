#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SaveSystem.Editor
{
    [InitializeOnLoad]
    public class SavableBridgeAutomation : IProcessSceneWithReport
    {
        static SavableBridgeAutomation()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                ProcessSceneObjects(SceneManager.GetActiveScene());
            }
        }

        public int callbackOrder { get { return 0; } }
        public void OnProcessScene(Scene _scene, BuildReport _report)
        {
            ProcessSceneObjects(_scene);
        }


        static void ProcessSceneObjects(Scene _scene)
        {
            GameObject[] roots = _scene.GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                AddSavableBridgeToRootObject(root);
            }
        }

        static void AddSavableBridgeToRootObject(GameObject _rootObject)
        {
            try
            {
                MonoBehaviour[] scriptsOnRoot = _rootObject.GetComponentsInChildren<MonoBehaviour>();
                if (scriptsOnRoot == null || scriptsOnRoot.Length < 0) return;
                foreach (MonoBehaviour script in scriptsOnRoot)
                {
                    if(script == null)
                    {
                        continue;
                    }
                    if (script.GetType().GetCustomAttribute<SavableAttribute>() == null)
                    {
                        continue;
                    }
                    if (script.TryGetComponent(out ISavableBridge bridge))
                    {
                        // It already contains a SavableBridge
                        continue;
                    }
                    script.gameObject.AddComponent<SavableBridge>();
                    Debug.Log($"Added Bridge to{script.gameObject.name}");

                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error processing root object for savable automation {_rootObject.name}: {ex.Message} \n" +
                    $"This should not happen, Save system should still function correctly," +
                    $" as long as savable bridge is added manually and the gameobject implements savable attribute and ISavable interface");
            }
        }
    }
}
#endif
