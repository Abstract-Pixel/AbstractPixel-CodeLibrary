using System.Collections;
using System.Collections.Generic;
using AbstractPixel.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>
    /// The Scene System page's actions: deleting a scene set (blocked while groups use it), the Scene Groups Debugger's
    /// scene set and main scene edits (overrides kept, Undo works), and the page opening where the old window opened.
    /// </summary>
    public class SceneSystemPageTests
    {
        private const string Folder = "Assets/_SceneSystemPageTests";
        private readonly List<Object> created = new List<Object>();

        [SetUp]
        public void CreateFolder()
        {
            AssetDatabase.DeleteAsset(Folder);
            AssetDatabase.CreateFolder("Assets", "_SceneSystemPageTests");
        }

        [TearDown]
        public void DeleteFolder()
        {
            foreach (Object item in created)
            {
                if (item != null && !AssetDatabase.Contains(item))
                {
                    Object.DestroyImmediate(item);
                }
            }
            created.Clear();
            AssetDatabase.DeleteAsset(Folder);
        }

        private static SceneReference Scene(string _name)
        {
            return new SceneReference(_name, "guid-" + _name, "Assets/" + _name + ".unity");
        }

        private static SceneSet CreateSetAsset(string _name)
        {
            SceneSet set = ScriptableObject.CreateInstance<SceneSet>();
            AssetDatabase.CreateAsset(set, Folder + "/" + _name + ".asset");
            return set;
        }

        private static SceneGroup CreateGroupAsset(string _name, SceneSet _set)
        {
            SceneGroup group = ScriptableObject.CreateInstance<SceneGroup>();
            group.SceneSet = _set;
            AssetDatabase.CreateAsset(group, Folder + "/" + _name + ".asset");
            return group;
        }

        private SceneSet Set(string _manager, string _shared, bool _reset)
        {
            SceneSet set = ScriptableObject.CreateInstance<SceneSet>();
            set.ManagerScenes.Add(Scene(_manager));
            set.SharedScenes.Add(Scene(_shared));
            set.ResetSharedScenes = _reset;
            created.Add(set);
            return set;
        }

        private SceneGroup GroupOn(SceneSet _set)
        {
            SceneGroup group = ScriptableObject.CreateInstance<SceneGroup>();
            group.Initialize(new SceneReference[0], new SceneReference[0], Scene("Level 1"));
            group.SceneSet = _set;
            SceneSetSync.ApplySet(group);
            created.Add(group);
            return group;
        }

        [Test]
        public void CanDelete_NoSet_IsFalse()
        {
            bool canDelete = SceneSetDeletion.CanDelete(null, out string reason);

            Assert.IsFalse(canDelete);
            Assert.IsNotEmpty(reason);
        }

        [Test]
        public void CanDelete_SetUsedByGroups_IsFalseAndSaysHowMany()
        {
            SceneSet set = CreateSetAsset("Used");
            CreateGroupAsset("firstGroup", set);
            CreateGroupAsset("secondGroup", set);

            bool canDelete = SceneSetDeletion.CanDelete(set, out string reason);

            Assert.IsFalse(canDelete);
            StringAssert.Contains("2 scene group(s)", reason);
        }

        [Test]
        public void CanDelete_SetUsedByNoGroup_IsTrue()
        {
            SceneSet set = CreateSetAsset("Unused");
            CreateGroupAsset("otherGroup", null);

            bool canDelete = SceneSetDeletion.CanDelete(set, out string reason);

            Assert.IsTrue(canDelete);
            Assert.IsEmpty(reason);
        }

        [Test]
        public void Delete_SetInUse_KeepsTheAssetAndTheSettings()
        {
            SceneSet set = CreateSetAsset("Used");
            CreateGroupAsset("levelGroup", set);
            SceneSystemSettings settings = ScriptableObject.CreateInstance<SceneSystemSettings>();
            created.Add(settings);
            settings.SceneSets.Add(set);
            settings.DefaultSceneSet = set;
            string path = AssetDatabase.GetAssetPath(set);

            bool deleted = SceneSetDeletion.Delete(settings, set);

            Assert.IsFalse(deleted);
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<SceneSet>(path));
            Assert.Contains(set, settings.SceneSets);
            Assert.AreSame(set, settings.DefaultSceneSet);
        }

        [Test]
        public void Delete_UnusedDefaultSet_RemovesItAndClearsTheDefault()
        {
            SceneSet set = CreateSetAsset("Unused");
            SceneSet other = CreateSetAsset("Other");
            SceneSystemSettings settings = ScriptableObject.CreateInstance<SceneSystemSettings>();
            created.Add(settings);
            settings.SceneSets.Add(set);
            settings.SceneSets.Add(other);
            settings.DefaultSceneSet = set;
            string path = AssetDatabase.GetAssetPath(set);

            bool deleted = SceneSetDeletion.Delete(settings, set);

            Assert.IsTrue(deleted);
            Assert.IsNull(AssetDatabase.LoadAssetAtPath<SceneSet>(path));
            CollectionAssert.AreEqual(new[] { other }, settings.SceneSets);
            Assert.IsNull(settings.DefaultSceneSet);
        }

        [Test]
        public void PickSet_GroupWithAnOverride_KeepsTheOverrideAndFillsTheRest()
        {
            SceneSet level = Set("Managers", "Car", true);
            SceneSet menu = Set("MenuManagers", "MenuCar", false);
            SceneGroup group = GroupOn(level);
            group.ManagerialBootScenesList = new List<SceneReference> { Scene("OwnManagers") };
            SceneSetSync.UpdateOverrideAfterEdit(group, SceneSetPart.ManagerScenes);

            SceneSetSync.PickSet(group, menu);

            Assert.AreSame(menu, group.SceneSet);
            Assert.IsTrue(group.IsOverride(SceneSetPart.ManagerScenes));
            Assert.AreEqual("OwnManagers", SceneSetSync.Describe(group.ManagerialBootScenesList));
            Assert.AreEqual("MenuCar", SceneSetSync.Describe(group.ContextualBootScenesList));
            Assert.IsFalse(group.ForceReloadContextualScenes);
        }

        [Test]
        public void PickSet_Undo_PutsTheOldSetAndScenesBack()
        {
            SceneSet level = Set("Managers", "Car", true);
            SceneSet menu = Set("MenuManagers", "MenuCar", false);
            SceneGroup group = GroupOn(level);
            Undo.IncrementCurrentGroup();

            SceneSetSync.PickSet(group, menu);
            Undo.PerformUndo();

            Assert.AreSame(level, group.SceneSet);
            Assert.AreEqual("Car", SceneSetSync.Describe(group.ContextualBootScenesList));
            Assert.IsTrue(group.ForceReloadContextualScenes);
        }

        [Test]
        public void SetMainScene_StoresTheSceneAndClearingItClearsTheStoredIdentity()
        {
            string scenePath = Folder + "/PageTestMain.unity";
            // A scene file written directly, so the test does not depend on which scenes the test runner has open.
            System.IO.File.WriteAllText(scenePath, "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n");
            AssetDatabase.ImportAsset(scenePath);
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            SceneGroup group = GroupOn(null);

            SceneGroupMainScene.Set(group, sceneAsset);

            Assert.AreSame(sceneAsset, group.MainScene.SceneFieldReference.SceneAssetInEditor);
            Assert.AreEqual(scenePath, group.MainScene.ScenePath);
            Assert.AreEqual(AssetDatabase.AssetPathToGUID(scenePath), group.MainScene.SceneGuid);
            Assert.AreEqual("PageTestMain", group.MainScene.SceneName);

            SceneGroupMainScene.Set(group, null);

            Assert.IsNull(group.MainScene.SceneFieldReference.SceneAssetInEditor);
            Assert.IsEmpty(group.MainScene.ScenePath);
            Assert.IsEmpty(group.MainScene.SceneGuid);
            Assert.IsEmpty(group.MainScene.SceneName);
        }

        [Test]
        public void SetMainScene_Undo_PutsTheOldSceneBack()
        {
            SceneGroup group = GroupOn(null);
            Undo.IncrementCurrentGroup();

            SceneGroupMainScene.Set(group, null);
            Undo.PerformUndo();

            Assert.AreEqual("Level 1", group.MainScene.SceneName);
        }

        [Test]
        public void MoveTo_AnotherFolder_MovesTheSettingsKeepingTheirName()
        {
            AssetDatabase.CreateFolder(Folder, "Moved");
            SceneSystemSettings settings = ScriptableObject.CreateInstance<SceneSystemSettings>();
            AssetDatabase.CreateAsset(settings, Folder + "/TestSettings.asset");

            string error = SceneSystemSettingsAssets.MoveTo(settings, Folder + "/Moved/");

            Assert.IsEmpty(error);
            Assert.AreEqual(Folder + "/Moved/TestSettings.asset", AssetDatabase.GetAssetPath(settings));
        }

        [TestCase("Assets/_SceneSystemPageTests/Editor")]
        [TestCase("Packages/Somewhere")]
        [TestCase("Assets/_SceneSystemPageTests/NotThere")]
        public void MoveTo_FolderThatCannotHoldTheSettings_IsRefusedAndNothingMoves(string _folder)
        {
            if (_folder.EndsWith("/Editor"))
            {
                AssetDatabase.CreateFolder(Folder, "Editor");
            }
            SceneSystemSettings settings = ScriptableObject.CreateInstance<SceneSystemSettings>();
            AssetDatabase.CreateAsset(settings, Folder + "/TestSettings.asset");

            string error = SceneSystemSettingsAssets.MoveTo(settings, _folder);

            Assert.IsNotEmpty(error);
            Assert.AreEqual(Folder + "/TestSettings.asset", AssetDatabase.GetAssetPath(settings));
        }

        [Test]
        public void MoveTo_NoSettings_SaysSo()
        {
            Assert.IsNotEmpty(SceneSystemSettingsAssets.MoveTo(null, "Assets"));
        }

        private SceneSet SetAsset(string _name, string _manager, string _shared, bool _reset)
        {
            SceneSet set = ScriptableObject.CreateInstance<SceneSet>();
            set.ManagerScenes.Add(Scene(_manager));
            set.SharedScenes.Add(Scene(_shared));
            set.ResetSharedScenes = _reset;
            AssetDatabase.CreateAsset(set, Folder + "/" + _name + ".asset");
            return set;
        }

        private static SceneGroup GroupAssetOn(string _name, SceneSet _set)
        {
            SceneGroup group = ScriptableObject.CreateInstance<SceneGroup>();
            group.Initialize(new SceneReference[0], new SceneReference[0], Scene("Main " + _name));
            group.SceneSet = _set;
            SceneSetSync.ApplySet(group);
            AssetDatabase.CreateAsset(group, Folder + "/" + _name + ".asset");
            return group;
        }

        private SceneSystemSettings SettingsWith(params SceneSet[] _sets)
        {
            SceneSystemSettings settings = ScriptableObject.CreateInstance<SceneSystemSettings>();
            created.Add(settings);
            settings.SceneSets.AddRange(_sets);
            return settings;
        }

        [Test]
        public void Remove_MoveToAnotherSet_MovesEveryGroupKeepsOverridesAndDeletesTheSet()
        {
            SceneSet level = SetAsset("Level", "Managers", "Car", true);
            SceneSet menu = SetAsset("Menu", "MenuManagers", "MenuCar", false);
            SceneGroup plain = GroupAssetOn("plainGroup", level);
            SceneGroup overridden = GroupAssetOn("overriddenGroup", level);
            overridden.ManagerialBootScenesList = new List<SceneReference> { Scene("OwnManagers") };
            SceneSetSync.UpdateOverrideAfterEdit(overridden, SceneSetPart.ManagerScenes);
            SceneSystemSettings settings = SettingsWith(level, menu);
            string levelPath = AssetDatabase.GetAssetPath(level);

            string error = SceneSetRemoval.Remove(settings, level, SceneSetRemovalMode.MoveToAnotherSet, menu, null);

            Assert.IsEmpty(error);
            Assert.IsNull(AssetDatabase.LoadAssetAtPath<SceneSet>(levelPath));
            Assert.AreSame(menu, plain.SceneSet);
            Assert.AreEqual("MenuCar", SceneSetSync.Describe(plain.ContextualBootScenesList));
            Assert.AreSame(menu, overridden.SceneSet);
            Assert.AreEqual("OwnManagers", SceneSetSync.Describe(overridden.ManagerialBootScenesList));
            CollectionAssert.AreEqual(new[] { menu }, settings.SceneSets);
        }

        [Test]
        public void Remove_ConvertToOverride_KeepsTheScenesAndClearsTheSet()
        {
            SceneSet level = SetAsset("Level", "Managers", "Car", true);
            SceneGroup group = GroupAssetOn("levelGroup", level);
            SceneSystemSettings settings = SettingsWith(level);

            string error = SceneSetRemoval.Remove(settings, level, SceneSetRemovalMode.ConvertToOverride, null, null);

            Assert.IsEmpty(error);
            Assert.IsNull(group.SceneSet);
            Assert.AreEqual("Managers", SceneSetSync.Describe(group.ManagerialBootScenesList));
            Assert.AreEqual("Car", SceneSetSync.Describe(group.ContextualBootScenesList));
            Assert.IsTrue(group.ForceReloadContextualScenes);
            Assert.IsFalse(group.IsOverride(SceneSetPart.ManagerScenes));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Remove_MoveWithoutAUsableReplacement_ChangesNothing(bool _replacementIsTheSameSet)
        {
            SceneSet level = SetAsset("Level", "Managers", "Car", true);
            SceneGroup group = GroupAssetOn("levelGroup", level);
            SceneSystemSettings settings = SettingsWith(level);
            SceneSet replacement = _replacementIsTheSameSet ? level : null;

            string error = SceneSetRemoval.Remove(settings, level, SceneSetRemovalMode.MoveToAnotherSet, replacement, null);

            Assert.IsNotEmpty(error);
            Assert.AreSame(level, group.SceneSet);
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<SceneSet>(Folder + "/Level.asset"));
        }

        [Test]
        public void Remove_TheDefaultSet_MakesThePickedSetTheDefault()
        {
            SceneSet level = SetAsset("Level", "Managers", "Car", true);
            SceneSet menu = SetAsset("Menu", "MenuManagers", "MenuCar", false);
            SceneSystemSettings settings = SettingsWith(level, menu);
            settings.DefaultSceneSet = level;

            string error = SceneSetRemoval.Remove(settings, level, SceneSetRemovalMode.ConvertToOverride, null, menu);

            Assert.IsEmpty(error);
            Assert.AreSame(menu, settings.DefaultSceneSet);
        }

        [Test]
        public void WithOnlyTheseSettingsPreloaded_SwapsOtherSettingsAndKeepsEverythingElse()
        {
            SceneSystemSettings oldSettings = ScriptableObject.CreateInstance<SceneSystemSettings>();
            SceneSystemSettings newSettings = ScriptableObject.CreateInstance<SceneSystemSettings>();
            SceneSet other = ScriptableObject.CreateInstance<SceneSet>();
            created.Add(oldSettings);
            created.Add(newSettings);
            created.Add(other);

            Object[] result = SceneSystemSettingsAssets.WithOnlyTheseSettingsPreloaded(new Object[] { other, null, oldSettings }, newSettings);

            CollectionAssert.AreEqual(new Object[] { other, newSettings }, result);
        }

        [TestCase(SceneGroupListKind.ManagerScenes, 0, "Manager scenes (Scene 1)")]
        [TestCase(SceneGroupListKind.SharedScenes, 2, "Shared scenes (Scene 3)")]
        public void Where_CountsRowsFromOne(SceneGroupListKind _list, int _index, string _expected)
        {
            Assert.AreEqual(_expected, SceneGroupCheck.Where(_list, _index));
        }

        [Test]
        public void BuildFailureMessage_PointsAtTheSceneGroupsDebugger()
        {
            List<SceneGroupProblem> problems = new List<SceneGroupProblem>();
            SceneGroup group = GroupOn(null);
            group.MainScene = new SceneReference(string.Empty);
            problems.AddRange(SceneGroupCheck.Find(group));
            Assume.That(problems.Count, Is.GreaterThan(0));
            Dictionary<string, List<SceneGroupProblem>> problemsByGroup = new Dictionary<string, List<SceneGroupProblem>>();
            problemsByGroup["Assets/levelGroup.asset"] = problems;

            string message = SceneGroupCheck.BuildFailureMessage(problemsByGroup);

            StringAssert.Contains("Abstract Pixel > Scene System > Scene Groups Debugger", message);
            StringAssert.DoesNotContain("Check All Scene Groups", message);
        }

        [Test]
        public void CheckAllSceneGroupsWindow_IsGone()
        {
            System.Type windowType = typeof(SceneSystemSettingsProvider).Assembly.GetType("AbstractPixel.SceneManagement.Editor.CheckAllSceneGroupsWindow");

            Assert.IsNull(windowType);
        }

        [UnityTest]
        public IEnumerator OpenAtDebugger_OpensTheSceneSystemPage()
        {
            bool wasOpen = ProjectSettingsWindow() != null;

            SceneSystemSettingsProvider.OpenAtDebugger();
            for (int frame = 0; frame < 300 && !SceneSystemSettingsProvider.IsOpen; frame++)
            {
                yield return null;
            }

            Assert.IsTrue(SceneSystemSettingsProvider.IsOpen);
            EditorWindow window = ProjectSettingsWindow();
            if (!wasOpen && window != null)
            {
                window.Close();
            }
        }

        private static EditorWindow ProjectSettingsWindow()
        {
            foreach (EditorWindow window in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                if (window.GetType().Name == "ProjectSettingsWindow")
                {
                    return window;
                }
            }
            return null;
        }
    }
}
