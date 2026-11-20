using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AbstractPixel.SaveSystem.Editor;
using NUnit.Framework;
using UnityEditor.Build;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Tests
{
    // T6: an out-of-date save file list fails the build, like the other editor-visible mistakes.
    public class SaveFileListBuildGuardTests
    {
        string tempScriptPath;
        SaveSystemConfigSO config;

        [TearDown]
        public void TearDown()
        {
            if (config != null)
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
            if (tempScriptPath != null && File.Exists(tempScriptPath))
            {
                File.Delete(tempScriptPath);
            }
        }

        [Test]
        public void AnOutOfDateList_FailsTheBuild()
        {
            tempScriptPath = NewTempPath();
            config = CreateConfig(tempScriptPath, SaveFileDefinition.CreateForList("Ghosts", SaveLocation.GameProfile));

            Assert.Throws<BuildFailedException>(() => SaveFileListBuildGuard.CheckConfig(config));
        }

        [Test]
        public void AnUpToDateList_DoesNotFailTheBuild()
        {
            tempScriptPath = NewTempPath();
            List<SaveFileDefinition> rows = new List<SaveFileDefinition> { SaveFileDefinition.CreateForList("Ghosts", SaveLocation.GameProfile) };
            File.WriteAllText(tempScriptPath, SaveFilesCodeWriter.WriteCode(rows));
            config = CreateConfig(tempScriptPath, rows[0]);

            Assert.DoesNotThrow(() => SaveFileListBuildGuard.CheckConfig(config));
        }

        static SaveSystemConfigSO CreateConfig(string scriptPath, params SaveFileDefinition[] rows)
        {
            SaveSystemConfigSO config = ScriptableObject.CreateInstance<SaveSystemConfigSO>();
            config.ReplaceSaveFileList(rows);
            SetField(config, "gameSaveFilesScriptPath", scriptPath);
            return config;
        }

        static string NewTempPath()
        {
            return Path.Combine(Path.GetTempPath(), $"SaveFileListBuildGuardTests_{Guid.NewGuid()}.cs");
        }

        static void SetField(object target, string fieldName, object value)
        {
            target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(target, value);
        }
    }
}
