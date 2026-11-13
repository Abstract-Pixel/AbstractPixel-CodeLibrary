using System;
using System.Collections.Generic;
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

            HashSet<string> classesWithARow = new HashSet<string>();
            foreach (SaveFileDefinition row in config.SaveFileList)
            {
                if (row != null && !string.IsNullOrEmpty(row.ClassFullName))
                {
                    classesWithARow.Add(row.ClassFullName);
                }
            }

            foreach (Type savedType in TypeCache.GetTypesDerivedFrom(typeof(ISavable<>)))
            {
                ReportSaveFileWithNoRow(savedType, classesWithARow);
            }
        }

        private static void ReportSaveFileWithNoRow(Type savedType, HashSet<string> classesWithARow)
        {
            Type saveFileClass = SaveFileLookup.FindSaveFileClass(savedType);
            bool namesASaveFile = saveFileClass != null && !saveFileClass.IsGenericParameter;
            if (!namesASaveFile)
            {
                return;
            }

            // A module's own class gets its row added by SaveFileListSync, so only report classes that can't be added.
            bool hasRow = classesWithARow.Contains(saveFileClass.FullName);
            bool rowWillBeAdded = Attribute.IsDefined(saveFileClass, typeof(SaveFileDefaultsAttribute));
            bool isTestClass = savedType.Assembly.GetName().Name.EndsWith("Tests", StringComparison.Ordinal);
            if (hasRow || rowWillBeAdded || isTestClass)
            {
                return;
            }

            UnityEngine.Object scriptAsset = GetScriptAssetFromType(savedType);
            string scriptLink = GetHyperlink(scriptAsset, savedType.Name);
            Debug.LogWarning($"[SaveSystem] {scriptLink} saves into '{saveFileClass.FullName}', but the save system config has no row for that class, so its data isn't saved. Select the config and press Write GameSaveFiles.cs, or add a row named '{saveFileClass.Name}'.", scriptAsset);
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
