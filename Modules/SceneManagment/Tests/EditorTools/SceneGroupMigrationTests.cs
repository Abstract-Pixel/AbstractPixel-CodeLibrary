using System.Collections.Generic;
using AbstractPixel.Core;
using NUnit.Framework;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>Migration: one scene set per distinct (manager scenes, shared scenes, Reset shared scenes), names and the report.</summary>
    public class SceneGroupMigrationTests
    {
        private readonly List<SceneGroup> groups = new List<SceneGroup>();

        [TearDown]
        public void DestroyGroups()
        {
            foreach (SceneGroup group in groups)
            {
                Object.DestroyImmediate(group);
            }
            groups.Clear();
        }

        private static SceneReference Scene(string _name)
        {
            return new SceneReference(_name, "guid-" + _name, "Assets/" + _name + ".unity");
        }

        private SceneGroup Group(string _name, string _main, string[] _managers, string[] _shared, bool _reset)
        {
            SceneGroup group = ScriptableObject.CreateInstance<SceneGroup>();
            group.name = _name;
            List<SceneReference> managers = new List<SceneReference>();
            foreach (string scene in _managers)
            {
                managers.Add(Scene(scene));
            }
            List<SceneReference> shared = new List<SceneReference>();
            foreach (string scene in _shared)
            {
                shared.Add(Scene(scene));
            }
            SceneReference main = string.IsNullOrEmpty(_main) ? new SceneReference(string.Empty) : Scene(_main);
            group.Initialize(managers, shared, main, _reset);
            groups.Add(group);
            return group;
        }

        // Rally Shift's shape: levels share one combination, the main menu has its own, boot groups have no main scene.
        private void MakeRallyLikeGroups()
        {
            Group("level2Group", "Level 2", new[] { "ManagerBootScene" }, new[] { "GameBootScene" }, true);
            Group("level3Group", "Level 3", new[] { "ManagerBootScene" }, new[] { "GameBootScene" }, true);
            Group("level13Group", "Level 13", new[] { "ManagerBootScene" }, new[] { "GameBootScene" }, true);
            Group("MainMenuGroup", "MainMenu", new[] { "ManagerBootScene" }, new[] { "LevelSelectionScene" }, true);
            Group("LevelBootGroup", null, new[] { "ManagerBootScene" }, new[] { "GameBootScene" }, true);
        }

        // Found in the Unity 7 Lab (session 5A2): setup's Ready said "2 scene groups can be sorted" for the two sample
        // scene groups, which the Scene System page's migration box leaves out.
        [Test]
        public void SetupsMigrationLineLeavesSampleSceneGroupsOut()
        {
            Group("level2Group", "Level 2", new[] { "ManagerBootScene" }, new[] { "GameBootScene" }, true);
            SceneGroup sample = Group("Sample 05 Area AGroup", "Sample 05 Area A", new string[0], new[] { "Sample 05 Shared" }, false);
            sample.IsSample = true;

            SceneGroupMigrationPlan plan = SceneSystemSetup.MigrationPlanFor(groups);

            Assert.AreEqual(1, plan.GroupsToMigrateCount, "only the game's scene group");
            StringAssert.StartsWith("1 scene groups", SceneSystemSetup.MigrationHint(plan));
        }

        [Test]
        public void Plan_RallyLikeGroups_MakesOneSetPerCombination()
        {
            MakeRallyLikeGroups();

            SceneGroupMigrationPlan plan = SceneGroupMigration.Plan(groups);

            Assert.AreEqual(2, plan.GroupsBySet.Count);
            Assert.AreEqual(3, plan.GroupsBySet[0].Count);
            Assert.AreEqual(1, plan.GroupsBySet[1].Count);
            Assert.AreEqual("MainMenuGroup", plan.GroupsBySet[1][0].name);
            Assert.AreEqual(new[] { "Level", "Main Menu" }, plan.SetNames.ToArray());
            Assert.AreEqual(0, plan.DefaultSetIndex);
            Assert.AreEqual(1, plan.GroupsWithoutMainScene.Count);
            Assert.AreEqual("LevelBootGroup", plan.GroupsWithoutMainScene[0].name);
        }

        [Test]
        public void Plan_ResetDiffers_SeparateSets()
        {
            Group("a", "A", new[] { "M" }, new[] { "S" }, true);
            Group("b", "B", new[] { "M" }, new[] { "S" }, false);

            SceneGroupMigrationPlan plan = SceneGroupMigration.Plan(groups);

            Assert.AreEqual(2, plan.GroupsBySet.Count);
        }

        [Test]
        public void Plan_SceneOrderDiffers_SeparateSets()
        {
            Group("a", "A", new[] { "M1", "M2" }, new string[0], false);
            Group("b", "B", new[] { "M2", "M1" }, new string[0], false);

            SceneGroupMigrationPlan plan = SceneGroupMigration.Plan(groups);

            Assert.AreEqual(2, plan.GroupsBySet.Count);
        }

        [Test]
        public void Plan_GroupAlreadyInASet_IsLeftAlone()
        {
            SceneGroup inSet = Group("a", "A", new[] { "M" }, new string[0], false);
            SceneSet set = ScriptableObject.CreateInstance<SceneSet>();
            inSet.SceneSet = set;

            SceneGroupMigrationPlan plan = SceneGroupMigration.Plan(groups);
            Object.DestroyImmediate(set);

            Assert.AreEqual(0, plan.GroupsBySet.Count);
            Assert.AreEqual(1, plan.GroupsAlreadyInASet.Count);
        }

        [TestCase(new[] { "Level 2", "Level 3", "Level 13" }, 0, "Level")]
        [TestCase(new[] { "MainMenu" }, 1, "Main Menu")]
        [TestCase(new[] { "Forest", "Desert" }, 2, "Scene set 3")]
        [TestCase(new[] { "Arena_01", "Arena_02" }, 0, "Arena")]
        public void SetName_CommonStartOfTheMainScenes(string[] _mainScenes, int _index, string _expected)
        {
            Assert.AreEqual(_expected, SceneGroupMigration.SetName(_mainScenes, _index));
        }

        [Test]
        public void Report_ListsEveryGroupBeforeAndAfter_AndTheBootGroupsAsUnused()
        {
            MakeRallyLikeGroups();
            SceneGroupMigrationPlan plan = SceneGroupMigration.Plan(groups);

            string report = SceneGroupMigration.Report(plan, true, _group => "Assets/" + _group.name + ".asset", _group => string.Empty);

            StringAssert.Contains("Preview", report);
            StringAssert.Contains("**Level** (default)", report);
            StringAssert.Contains("**Main Menu**", report);
            foreach (SceneGroup group in groups)
            {
                StringAssert.Contains(group.name, report);
            }
            StringAssert.Contains("ManagerBootScene", report);
            StringAssert.Contains("GameBootScene", report);
            StringAssert.Contains("unused, can be deleted", report);
            StringAssert.Contains("Before", report);
            StringAssert.Contains("After", report);
        }

        [Test]
        public void Report_BootGroupWithReferences_SaysWhatStillUsesIt()
        {
            MakeRallyLikeGroups();
            SceneGroupMigrationPlan plan = SceneGroupMigration.Plan(groups);

            string report = SceneGroupMigration.Report(plan, false, _group => "Assets/" + _group.name + ".asset", _group => "Assets/Boot.prefab");

            StringAssert.Contains("Assets/Boot.prefab", report);
            StringAssert.DoesNotContain("Preview", report);
        }
    }
}
