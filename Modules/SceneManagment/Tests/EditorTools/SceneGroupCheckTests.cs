using System.Collections.Generic;
using System.Linq;
using AbstractPixel.Core;
using AbstractPixel.Core.Editor;
using NUnit.Framework;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>The group check: no main scene, Missing scene, scene Not in build, same scene listed twice.</summary>
    public class SceneGroupCheckTests
    {
        private readonly List<SceneGroup> groups = new List<SceneGroup>();
        private readonly Dictionary<string, SceneStatus> statusByName = new Dictionary<string, SceneStatus>();

        [TearDown]
        public void DestroyGroups()
        {
            foreach (SceneGroup group in groups)
            {
                Object.DestroyImmediate(group);
            }
            groups.Clear();
            statusByName.Clear();
        }

        private SceneReference Scene(string _name, SceneStatus _status)
        {
            statusByName[_name] = _status;
            return new SceneReference(_name, "guid-" + _name, "Assets/" + _name + ".unity");
        }

        private SceneGroup Group(string _name, SceneReference[] _managers, SceneReference[] _shared, SceneReference _main)
        {
            SceneGroup group = ScriptableObject.CreateInstance<SceneGroup>();
            group.name = _name;
            group.Initialize(_managers, _shared, _main);
            groups.Add(group);
            return group;
        }

        private SceneStatus StatusOf(SceneReference _scene)
        {
            if (_scene == null || string.IsNullOrEmpty(_scene.SceneName))
            {
                return SceneStatus.NoScene;
            }
            return statusByName[_scene.SceneName];
        }

        private List<SceneGroupProblem> Check(SceneGroup _group)
        {
            return SceneGroupCheck.Find(_group, StatusOf);
        }

        [Test]
        public void Find_HealthyGroup_HasNoProblems()
        {
            SceneGroup group = Group("level1Group", new[] { Scene("Managers", SceneStatus.InBuild) }, new[] { Scene("Car", SceneStatus.Addressable) }, Scene("Level 1", SceneStatus.InBuild));

            Assert.IsEmpty(Check(group));
        }

        [Test]
        public void Find_NoMainScene_IsAProblemWithoutAFix()
        {
            SceneGroup group = Group("emptyGroup", new SceneReference[0], new SceneReference[0], new SceneReference(string.Empty));

            List<SceneGroupProblem> problems = Check(group);

            Assert.AreEqual(1, problems.Count);
            Assert.AreEqual(SceneGroupProblemKind.NoMainScene, problems[0].Kind);
            Assert.AreEqual(SceneGroupProblemFix.None, problems[0].Fix);
            StringAssert.Contains("emptyGroup", problems[0].Message);
        }

        [Test]
        public void Find_NullMainScene_IsNoMainScene()
        {
            SceneGroup group = Group("nullGroup", new SceneReference[0], new SceneReference[0], null);

            List<SceneGroupProblem> problems = Check(group);

            Assert.AreEqual(SceneGroupProblemKind.NoMainScene, problems.Single().Kind);
        }

        [Test]
        public void Find_SceneNotInBuild_NamesTheGroupAndTheScene_AndOffersAddToBuild()
        {
            SceneGroup group = Group("level1Group", new SceneReference[0], new[] { Scene("Car", SceneStatus.NotInBuild) }, Scene("Level 1", SceneStatus.InBuild));

            SceneGroupProblem problem = Check(group).Single();

            Assert.AreEqual(SceneGroupProblemKind.NotInBuild, problem.Kind);
            Assert.AreEqual(SceneGroupProblemFix.AddToBuild, problem.Fix);
            Assert.AreEqual(SceneGroupListKind.SharedScenes, problem.List);
            Assert.AreEqual(0, problem.Index);
            StringAssert.Contains("level1Group", problem.Message);
            StringAssert.Contains("Car", problem.Message);
        }

        [Test]
        public void Find_MissingSceneInAList_ShowsTheOldPath_AndOffersRemoveFromGroup()
        {
            SceneGroup group = Group("level1Group", new[] { Scene("Old Managers", SceneStatus.Missing) }, new SceneReference[0], Scene("Level 1", SceneStatus.InBuild));

            SceneGroupProblem problem = Check(group).Single();

            Assert.AreEqual(SceneGroupProblemKind.MissingScene, problem.Kind);
            Assert.AreEqual(SceneGroupProblemFix.RemoveFromGroup, problem.Fix);
            Assert.AreEqual(SceneGroupListKind.ManagerScenes, problem.List);
            StringAssert.Contains("Assets/Old Managers.unity", problem.Message);
        }

        [Test]
        public void Find_MissingMainScene_HasNoOneClickFix()
        {
            SceneGroup group = Group("level1Group", new SceneReference[0], new SceneReference[0], Scene("Level 1", SceneStatus.Missing));

            SceneGroupProblem problem = Check(group).Single();

            Assert.AreEqual(SceneGroupProblemKind.MissingScene, problem.Kind);
            Assert.AreEqual(SceneGroupListKind.MainScene, problem.List);
            Assert.AreEqual(SceneGroupProblemFix.None, problem.Fix);
        }

        [Test]
        public void Find_SameSceneListedTwice_ReportsTheLaterEntry_AndOffersRemoveFromGroup()
        {
            SceneReference car = Scene("Car", SceneStatus.InBuild);
            SceneReference carAgain = Scene("Car", SceneStatus.InBuild);
            SceneGroup group = Group("level1Group", new SceneReference[0], new[] { car, carAgain }, Scene("Level 1", SceneStatus.InBuild));

            SceneGroupProblem problem = Check(group).Single();

            Assert.AreEqual(SceneGroupProblemKind.ListedTwice, problem.Kind);
            Assert.AreEqual(SceneGroupListKind.SharedScenes, problem.List);
            Assert.AreEqual(1, problem.Index);
            Assert.AreEqual(SceneGroupProblemFix.RemoveFromGroup, problem.Fix);
            StringAssert.Contains("Car", problem.Message);
        }

        [Test]
        public void Find_MainSceneAlsoInSharedScenes_ReportsTheSharedEntry()
        {
            SceneGroup group = Group("level1Group", new SceneReference[0], new[] { Scene("Level 1", SceneStatus.InBuild) }, Scene("Level 1", SceneStatus.InBuild));

            SceneGroupProblem problem = Check(group).Single();

            Assert.AreEqual(SceneGroupProblemKind.ListedTwice, problem.Kind);
            Assert.AreEqual(SceneGroupListKind.SharedScenes, problem.List);
        }

        [Test]
        public void Find_DuplicateSceneNotInBuild_IsReportedNotInBuildOnce()
        {
            SceneGroup group = Group("level1Group", new[] { Scene("Car", SceneStatus.NotInBuild) }, new[] { Scene("Car", SceneStatus.NotInBuild) }, Scene("Level 1", SceneStatus.InBuild));

            List<SceneGroupProblem> problems = Check(group);

            Assert.AreEqual(1, problems.Count(problem => problem.Kind == SceneGroupProblemKind.NotInBuild));
            Assert.AreEqual(1, problems.Count(problem => problem.Kind == SceneGroupProblemKind.ListedTwice));
        }

        [Test]
        public void Find_EmptyEntryInAList_IsIgnored()
        {
            SceneGroup group = Group("level1Group", new[] { new SceneReference(string.Empty), new SceneReference(string.Empty) }, new SceneReference[0], Scene("Level 1", SceneStatus.InBuild));

            Assert.IsEmpty(Check(group));
        }

        [Test]
        public void BuildFailureMessage_NoProblems_IsEmpty()
        {
            Dictionary<string, List<SceneGroupProblem>> none = new Dictionary<string, List<SceneGroupProblem>>();

            Assert.AreEqual(string.Empty, SceneGroupCheck.BuildFailureMessage(none));
        }

        [Test]
        public void BuildFailureMessage_NamesEachGroupAndScene()
        {
            SceneGroup first = Group("level1Group", new SceneReference[0], new[] { Scene("Car", SceneStatus.NotInBuild) }, Scene("Level 1", SceneStatus.InBuild));
            SceneGroup second = Group("level2Group", new SceneReference[0], new SceneReference[0], Scene("Level 2", SceneStatus.Missing));
            Dictionary<string, List<SceneGroupProblem>> problemsByGroup = new Dictionary<string, List<SceneGroupProblem>>();
            problemsByGroup["Assets/level1Group.asset"] = Check(first);
            problemsByGroup["Assets/level2Group.asset"] = Check(second);

            string message = SceneGroupCheck.BuildFailureMessage(problemsByGroup);

            StringAssert.Contains("Assets/level1Group.asset", message);
            StringAssert.Contains("Car", message);
            StringAssert.Contains("Assets/level2Group.asset", message);
            StringAssert.Contains("Level 2", message);
        }
    }
}
