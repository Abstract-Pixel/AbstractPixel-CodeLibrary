#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;

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
        public void OnProcessScene(Scene _scene,BuildReport _report)
        {
            ProcessSceneObjects(_scene);
        }


        static void ProcessSceneObjects(Scene _scene)
        {
            GameObject[] roots = _scene.GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                AddSaveableBridgeToRootObject(root);
            }
        }

        static void AddSaveableBridgeToRootObject(GameObject _rootObject)      
        {
            MonoBehaviour[] scriptsOnRoot = _rootObject.GetComponentsInChildren<MonoBehaviour>();
            foreach (MonoBehaviour script in scriptsOnRoot)
            {
                if (script.GetType().GetCustomAttribute<SaveableAttribute>() == null)
                {
                    continue;
                }
                if (script.TryGetComponent(out ISavableBridge bridge))
                {
                    // It already contains a SaveableBridge
                    continue;
                }
                script.gameObject.AddComponent<SavableBridge>();
                Debug.Log($"Added Bridge to{script.gameObject.name}");

            }
        }
    }
}
#endif
