using System;
using System.IO;
using System.Linq;
using System.Reflection;
using AbstractPixel.SaveSystem.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SaveSystem.Tests
{
    // T6: the config inspector's Update save file classes button shows only while the list is out of date.
    public class SaveSystemConfigEditorTests
    {
        string tempScriptPath;
        SaveSystemConfigSO config;
        UnityEditor.Editor editor;

        [TearDown]
        public void TearDown()
        {
            if (editor != null)
            {
                UnityEngine.Object.DestroyImmediate(editor);
            }
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
        public void AnOutOfDateList_ShowsTheUpdateButton()
        {
            config = CreateConfig(SaveFileDefinition.CreateForList("Ghosts", SaveLocation.GameProfile));

            VisualElement area = UpdateAreaOf(config);

            Assert.AreEqual(DisplayStyle.Flex, area.style.display.value);
        }

        [Test]
        public void AnUpToDateList_HidesTheUpdateButton()
        {
            SaveFileDefinition row = SaveFileDefinition.CreateForList("Ghosts", SaveLocation.GameProfile);
            config = CreateConfig(row);
            File.WriteAllText(tempScriptPath, SaveFilesCodeWriter.WriteCode(new[] { row }));

            VisualElement area = UpdateAreaOf(config);

            Assert.AreEqual(DisplayStyle.None, area.style.display.value);
        }

        [Test]
        public void AClash_DisablesTheUpdateButtonInsteadOfHidingIt()
        {
            SaveFileDefinition rowA = SaveFileDefinition.CreateForList("Ghosts", SaveLocation.GameProfile);
            SaveFileDefinition rowB = SaveFileDefinition.CreateForList("Ghosts", SaveLocation.Global);
            config = CreateConfig(rowA, rowB);

            VisualElement area = UpdateAreaOf(config);

            Assert.AreEqual(DisplayStyle.Flex, area.style.display.value, "The clash warning sits on the row, not here, so the area still shows.");
            Assert.IsFalse(FindUpdateButton(area).enabledSelf);
        }

        VisualElement UpdateAreaOf(SaveSystemConfigSO forConfig)
        {
            editor = UnityEditor.Editor.CreateEditor(forConfig);
            VisualElement root = editor.CreateInspectorGUI();
            return FindUpdateButton(root).parent;
        }

        static Button FindUpdateButton(VisualElement root)
        {
            return root.Query<Button>().ToList().First(button => button.text == "Update save file classes");
        }

        SaveSystemConfigSO CreateConfig(params SaveFileDefinition[] rows)
        {
            tempScriptPath = Path.Combine(Path.GetTempPath(), $"SaveSystemConfigEditorTests_{Guid.NewGuid()}.cs");
            SaveSystemConfigSO newConfig = ScriptableObject.CreateInstance<SaveSystemConfigSO>();
            newConfig.ReplaceSaveFileList(rows);
            SetField(newConfig, "gameSaveFilesScriptPath", tempScriptPath);
            return newConfig;
        }

        static void SetField(object target, string fieldName, object value)
        {
            target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(target, value);
        }
    }
}
