using System.Collections.Generic;
using AbstractPixel.SaveSystem.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Tests
{
    // The inspector's Name field applies on every keystroke, and each apply runs the config's OnValidate. A new row
    // must not take its storage name from the first letter typed; it gets one when its class is written.
    public class NewRowStorageNameTests
    {
        SaveSystemConfigSO config;
        SerializedObject serializedConfig;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<SaveSystemConfigSO>();
            config.ReplaceSaveFileList(new List<SaveFileDefinition> { new SaveFileDefinition() });
            serializedConfig = new SerializedObject(config);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(config);
        }

        void TypeName(params string[] keystrokes)
        {
            TypeNameInRow(0, keystrokes);
        }

        void TypeNameInRow(int rowIndex, params string[] keystrokes)
        {
            foreach (string textSoFar in keystrokes)
            {
                serializedConfig.Update();
                SerializedProperty row = serializedConfig.FindProperty("saveFileList").GetArrayElementAtIndex(rowIndex);
                row.FindPropertyRelative(nameof(SaveFileDefinition.Name)).stringValue = textSoFar;
                serializedConfig.ApplyModifiedProperties();
            }
        }

        SaveFileDefinition Row => config.EditableSaveFileList[0];

        [Test]
        public void TypingANewRowsName_StoresItUnderTheWholeName_NotTheFirstLetter()
        {
            TypeName("G", "Gh", "Ghosts");

            Assert.AreEqual("Ghosts", Row.FileName);
        }

        [Test]
        public void TwoNewRowsStartingWithTheSameLetter_DoNotClash()
        {
            config.ReplaceSaveFileList(new List<SaveFileDefinition> { new SaveFileDefinition(), new SaveFileDefinition() });
            serializedConfig = new SerializedObject(config);
            TypeNameInRow(0, "G", "Ghosts");
            TypeNameInRow(1, "G", "Gold");

            CollectionAssert.IsEmpty(SaveFileClash.FindAll(config.EditableSaveFileList));
        }

        [Test]
        public void ANewRowRenamedAfterItsClassIsWritten_KeepsTheStorageNameItWasWrittenWith()
        {
            TypeName("G", "Ghosts");
            SaveFilesCodeWriter.PointRowsAtTheirClasses(config.EditableSaveFileList, "Assembly-CSharp");

            TypeName("P", "Phantoms");

            Assert.AreEqual("Ghosts", Row.FileName);
        }

        [Test]
        public void ARowThatAlreadyHasAClass_GetsItsStorageNameFixed_SoARenameKeepsOldDataReadable()
        {
            Row.ClassFullName = "GameSaveFiles+Ghosts";

            // The first apply stands for the config loading in the Editor, before anyone renames the row.
            TypeName("Ghosts", "Phantoms");

            Assert.AreEqual("Ghosts", Row.FileName);
        }
    }
}
