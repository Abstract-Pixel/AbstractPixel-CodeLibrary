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
