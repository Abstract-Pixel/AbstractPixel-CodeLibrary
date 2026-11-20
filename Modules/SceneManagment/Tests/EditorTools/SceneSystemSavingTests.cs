using System.IO;
using AbstractPixel.Core;
using AbstractPixel.Core.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>
    /// Session 4B-2 persistence audit: an edit in the group inspector and an Undo or Redo of a saved change are on disk
    /// at once, so a crash or a killed Editor loses nothing (a clean quit already saved them).
    /// </summary>
    public class SceneSystemSavingTests
    {
        private const string TempRoot = "Assets/_SceneSystemSavingTests";

        private SceneSet levelSet;
        private SceneSet menuSet;
        private SceneGroup group;

        [SetUp]
        public void CreateAssets()
        {
            if (AssetDatabase.IsValidFolder(TempRoot))
            {
                AssetDatabase.DeleteAsset(TempRoot);
            }
            AssetDatabase.CreateFolder("Assets", "_SceneSystemSavingTests");

            levelSet = ScriptableObject.CreateInstance<SceneSet>();
            levelSet.ResetSharedScenes = true;
            AssetDatabase.CreateAsset(levelSet, TempRoot + "/Level.asset");
            menuSet = ScriptableObject.CreateInstance<SceneSet>();
            menuSet.ResetSharedScenes = false;
            AssetDatabase.CreateAsset(menuSet, TempRoot + "/Menu.asset");

            group = ScriptableObject.CreateInstance<SceneGroup>();
            group.Initialize(levelSet.ManagerScenes, levelSet.SharedScenes, new SceneReference("Level2"), true);
            group.SceneSet = levelSet;
            AssetDatabase.CreateAsset(group, TempRoot + "/Level2Group.asset");
            AssetDatabase.SaveAssets();
            Undo.ClearAll();
        }

        [TearDown]
        public void DeleteAssets()
        {
            Undo.ClearAll();
            AssetDatabase.DeleteAsset(TempRoot);
        }

        [Test]
        public void AnInspectorEditThatMakesAnOverrideIsOnDiskAtOnce()
        {
            EditInInspector("ForceReloadContextualScenes", false);

            bool changed = SceneSetSync.SaveInspectorEdit(group, SceneSetPart.ResetSharedScenes);

            Assert.IsTrue(changed, "the edit made an override");
            string onDisk = GroupFileText();
            StringAssert.Contains("ForceReloadContextualScenes: 0", onDisk);
            StringAssert.Contains("resetSharedScenesOverride: 1", onDisk);
            Assert.IsFalse(EditorUtility.IsDirty(group), "nothing left unsaved");
        }

        [Test]
        public void AnInspectorEditThatChangesNoOverrideIsStillSaved()
        {
            group.SceneSet = null;
            EditorUtility.SetDirty(group);
            AssetDatabase.SaveAssetIfDirty(group);
            EditInInspector("ForceReloadContextualScenes", false);

            SceneSetSync.SaveInspectorEdit(group, SceneSetPart.ResetSharedScenes);

            StringAssert.Contains("ForceReloadContextualScenes: 0", GroupFileText());
        }

        [Test]
        public void UndoOfASavedSceneSetPickIsOnDisk()
        {
            Undo.IncrementCurrentGroup();
            SceneSetSync.PickSet(group, menuSet);
            StringAssert.Contains(GuidOf(menuSet), GroupFileText(), "the pick is saved");

            Undo.PerformUndo();

            Assert.AreSame(levelSet, group.SceneSet, "Undo put the set back in memory");
            string onDisk = GroupFileText();
            StringAssert.Contains(GuidOf(levelSet), onDisk, "and on disk");
            StringAssert.DoesNotContain(GuidOf(menuSet), onDisk);
        }

        [Test]
        public void RedoOfASavedSceneSetPickIsOnDisk()
        {
            Undo.IncrementCurrentGroup();
            SceneSetSync.PickSet(group, menuSet);
            Undo.PerformUndo();

            Undo.PerformRedo();

            Assert.AreSame(menuSet, group.SceneSet);
            StringAssert.Contains(GuidOf(menuSet), GroupFileText());
        }

        [Test]
        public void SavesOnlyDirtySceneSystemAssets()
        {
            group.ForceReloadContextualScenes = false;
            EditorUtility.SetDirty(group);

            int saved = SceneSystemUndoSave.SaveDirtySceneSystemAssets();

            Assert.GreaterOrEqual(saved, 1);
            Assert.IsFalse(EditorUtility.IsDirty(group));
            Assert.IsFalse(EditorUtility.IsDirty(levelSet), "a clean asset is not touched");
            StringAssert.Contains("ForceReloadContextualScenes: 0", GroupFileText());
        }

        [Test]
        public void ABuildSettingsChangeIsOnDiskAtOnce()
        {
            EditorBuildSettingsScene[] original = EditorBuildSettings.scenes;
            Assume.That(original.Length, Is.GreaterThanOrEqualTo(2), "needs two scenes in Build Settings");
            EditorBuildSettingsScene[] swapped = (EditorBuildSettingsScene[])original.Clone();
            swapped[0] = original[1];
            swapped[1] = original[0];
            try
            {
                SceneBuildSettingsWriter.Write(swapped);

                Assert.AreEqual(original[1].path, FirstBuildScenePathOnDisk(), "the new order is on disk");
            }
            finally
            {
                SceneBuildSettingsWriter.Write(original);
            }
            Assert.AreEqual(original[0].path, FirstBuildScenePathOnDisk(), "and the old one again");
        }

        private static string FirstBuildScenePathOnDisk()
        {
            foreach (string line in File.ReadAllLines("ProjectSettings/EditorBuildSettings.asset"))
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith("path: "))
                {
                    return trimmed.Substring("path: ".Length);
                }
            }
            return string.Empty;
        }

        // What a bound field in the group inspector does: a SerializedObject edit, applied with Undo.
        private void EditInInspector(string _propertyName, bool _value)
        {
            SerializedObject groupObject = new SerializedObject(group);
            groupObject.FindProperty(_propertyName).boolValue = _value;
            groupObject.ApplyModifiedProperties();
        }

        private string GroupFileText()
        {
            string path = AssetDatabase.GetAssetPath(group);
            return File.ReadAllText(path);
        }

        private static string GuidOf(Object _asset)
        {
            return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_asset));
        }
    }
}
