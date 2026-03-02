using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor;
using System;


using System.Reflection;
using System.Linq;

namespace AbstractPixel.SaveSystem.Editor
{
    [InitializeOnLoad]
    public static class SaveSystemBodyGuard
    {

        static SaveSystemBodyGuard()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }


        [DidReloadScripts]
        public static void OnScriptsReloaded()
        {
            ValidateCodebase(out bool _errorFound);

        }

        private static void OnPlayModeStateChanged(PlayModeStateChange _state)
        {
            if (_state != PlayModeStateChange.ExitingEditMode) return;

            ValidateCodebase(out bool errorFound);

            if (errorFound)
            {
                EditorApplication.isPlaying = false;
                EditorUtility.DisplayDialog("Save System Integrity Error",
                        "Critical Save System errors found in your scripts.\n\n" +
                        "Play Mode has been blocked to prevent data corruption.\n\n" +
                        "Check the Console for specific script names.", "Ok,Will fix it");
            }
        }

        private static void ValidateCodebase(out bool errorFound)
        {
            errorFound = false;

            TypeCache.TypeCollection allScriptTypes = TypeCache.GetTypesDerivedFrom<MonoBehaviour>();

            foreach (Type type in allScriptTypes)
            {
                if (type.Assembly.FullName.StartsWith("Unity"))
                {
                    continue;
                }

                SaveableAttribute saveableAttribute = type.GetCustomAttribute<SaveableAttribute>();
                bool hasSaveInterface = ImplementsGenericInterface(type, typeof(ISavable<>));

                if ((saveableAttribute != null))
                {
                    if ((!hasSaveInterface))
                    {
                        errorFound = true;
                        UnityEngine.Object scriptAsset = GetScriptAssetFromType(type);
                        string scriptLink = GetHyperlink(scriptAsset, type.Name);
                        Debug.LogError($"<color=red>[SaveSystem Critical]</color> The script {scriptLink} has the <b>[Saveable]</b> attribute but DOES NOT implement <b>ISaveable<></b> Interface.\n" +
                               $"You must implement the interface to ensure data Capture/Restore works.",scriptAsset);

                    }
                }
                else
                {
                    if (hasSaveInterface)
                    {
                        errorFound = true;
                        UnityEngine.Object scriptAsset = GetScriptAssetFromType(type);
                        string link = GetHyperlink(scriptAsset, type.Name);
                        Debug.LogError($"<color=red>[SaveSystem Critical]</color> The script {link} implements <b>ISaveable<></b> Interface but is missing the <b>[Saveable(Category)]</b> attribute.\n" +
                           $"The Save System will ignore this script without the attribute.",scriptAsset);
                    }
                }
            }
        }
        private static string GetHyperlink(UnityEngine.Object asset, string fallbackName)
        {
            if (asset == null) return $"<b>'{fallbackName}'</b>";

            string path = AssetDatabase.GetAssetPath(asset);
            string guid = AssetDatabase.AssetPathToGUID(path);

            // This syntax creates a clickable link in Unity Console
            return $"<a href=\"asset:{guid}\"><b>'{fallbackName}'</b></a>";
        }

        private static bool ImplementsGenericInterface(Type _type, Type _genericInterface)
        {
            return _type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == _genericInterface);
        }

        private static UnityEngine.Object GetScriptAssetFromType(Type _type)
        {
            string[] guids = AssetDatabase.FindAssets($"t:MonoScript {_type.Name}");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script != null && script.GetClass() == _type)
                {
                    return script;
                }
               
            }
            return null;
        }

    }
}
