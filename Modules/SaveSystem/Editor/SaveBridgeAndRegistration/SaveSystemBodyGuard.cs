using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// Checks the project's saved scripts after every compile and before play mode: a script whose save file class
    /// has no row in the save system config would save nothing, so it is reported with a link to the script.
    /// </summary>
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
            ValidateCodebase();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange _state)
        {
            if (_state != PlayModeStateChange.ExitingEditMode)
            {
                return;
            }

            // Only report. A dialog here would stop play mode for everyone, including automated test runs.
            ValidateCodebase();
        }

        private static void ValidateCodebase()
        {
            SaveSystemConfigSO config = SaveSystemConfigFinder.FindInProject();
            if (config == null)
            {
                return;
            }

            foreach (Type savedType in TypeCache.GetTypesDerivedFrom<MonoBehaviour>())
            {
                ReportSaveFileWithNoRow(savedType, config);
            }
        }

        private static void ReportSaveFileWithNoRow(Type savedType, SaveSystemConfigSO config)
        {
            // A test fixture's made-up save file class never belongs in the project's config.
            bool isTestClass = savedType.Assembly.GetName().Name.EndsWith("Tests", StringComparison.Ordinal);
            if (isTestClass || !SavedScript.IsSaved(savedType))
            {
                return;
            }

            string problem = SaveFileRowCheck.ProblemWith(savedType, config);
            if (problem == null)
            {
                return;
            }

            UnityEngine.Object scriptAsset = GetScriptAssetFromType(savedType);
            string scriptLink = GetHyperlink(scriptAsset, savedType.Name);
            SaveLog.Warning($"[SaveSystem] {scriptLink} {problem}", scriptAsset);
        }

        private static string GetHyperlink(UnityEngine.Object asset, string fallbackName)
        {
            if (asset == null)
            {
                return $"'{fallbackName}'";
            }

            string path = AssetDatabase.GetAssetPath(asset);
            string guid = AssetDatabase.AssetPathToGUID(path);

            // This syntax makes a clickable link in the Unity console.
            return $"<a href=\"asset:{guid}\">'{fallbackName}'</a>";
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
