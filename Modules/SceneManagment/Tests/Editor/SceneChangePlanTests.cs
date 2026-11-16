using System.Collections.Generic;
using System.Linq;
using AbstractPixel.Core;
using NUnit.Framework;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>Which scenes a scene change closes and opens. No scene is loaded: the plan is plain data.</summary>
    public class SceneChangePlanTests
    {
        private SceneCoordinatorTestRig rig;

        [SetUp]
        public void CreateRig()
        {
            rig = SceneCoordinatorTestRig.StartInLevel("Level1");
        }

        [TearDown]
        public void DestroyRig()
        {
            rig.Dispose();
        }

        // Different level. Shared scenes it also uses are kept when the group does not force a reload.
        [TestCase("Car,Hud", "Car,Hud", "", "")]
        [TestCase("Car", "Car,Rain", "", "Rain")]
        [TestCase("Car,Hud", "Car", "Hud", "")]
        [TestCase("Car", "Rain", "Car", "Rain")]
        public void DifferentLevelKeepsSharedScenesWithoutForceReload(string _open, string _target, string _expectedClose, string _expectedOpen)
        {
            SceneChangePlan plan = CreatePlan("Level1", _open, "Level2", _target, false);

            Assert.AreEqual("Level1", plan.MainSceneToClose.SceneName);
            Assert.AreEqual("Level2", plan.MainSceneToOpen.SceneName);
            Assert.AreEqual(_expectedClose, Names(plan.ContextualToClose));
            Assert.AreEqual(_expectedOpen, Names(plan.ContextualToOpen));
        }

        // With force reload every open shared scene closes and every shared scene of the new group opens.
        [TestCase("Car", "Car", "Car", "Car")]
        [TestCase("Car,Hud", "Car", "Car,Hud", "Car")]
        [TestCase("Car", "Car,Rain", "Car", "Car,Rain")]
        [TestCase("", "Car", "", "Car")]
        public void ForceReloadClosesAndReopensSharedScenes(string _open, string _target, string _expectedClose, string _expectedOpen)
        {
            SceneChangePlan plan = CreatePlan("Level1", _open, "Level2", _target, true);

            Assert.AreEqual(_expectedClose, Names(plan.ContextualToClose));
            Assert.AreEqual(_expectedOpen, Names(plan.ContextualToOpen));
        }

        [TestCase("Car,Hud", true, "Car,Hud", "Car,Hud")]
        [TestCase("Car,Hud", false, "", "")]
        public void SameGroupAgainReopensTheMainSceneAndForcedSharedScenes(string _shared, bool _forceReload, string _expectedClose, string _expectedOpen)
        {
            SceneChangePlan plan = CreatePlan("Level1", _shared, "Level1", _shared, _forceReload);

            Assert.AreEqual("Level1", plan.MainSceneToClose.SceneName);
            Assert.AreEqual("Level1", plan.MainSceneToOpen.SceneName);
            Assert.AreEqual(_expectedClose, Names(plan.ContextualToClose));
            Assert.AreEqual(_expectedOpen, Names(plan.ContextualToOpen));
        }

        [TestCase("Car", "Car,Rain", false, "Car,Rain")]
        [TestCase("Car,Hud", "Car", false, "Car")]
        [TestCase("Car,Hud", "Car", true, "Car")]
        [TestCase("", "Car", true, "Car")]
        public void SharedScenesOpenAfterTheChangeAreTheNewGroupsScenes(string _open, string _target, bool _forceReload, string _expectedOpenAfter)
        {
            SceneChangePlan plan = CreatePlan("Level1", _open, "Level2", _target, _forceReload);

            Assert.AreEqual(_expectedOpenAfter, Names(plan.ContextualOpenAfter));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ManagerScenesAreNeverClosed(bool _forceReload)
        {
            SceneGroup openGroup = rig.CreateGroup("Level1", new[] { "Car" }, _forceReload);
            SceneGroup targetGroup = rig.CreateGroup("Level2", new[] { "Car" }, _forceReload);
            SceneChangePlan.OpenScenes open = CreateOpenScenes(openGroup);
            open.ManagerialScenes.Add(new SceneReference("AudioScene"));

            SceneChangePlan plan = new SceneChangePlan(targetGroup, open);

            Assert.AreEqual("", Names(plan.ManagerialToOpen), "the manager scenes are already open");
            Assert.AreEqual("AudioScene," + SceneCoordinatorTestRig.ManagerScene, Names(plan.ManagerialOpenAfter));
            Assert.IsFalse(plan.ContextualToClose.Any(scene => scene.SceneName == "AudioScene"));
        }

        [Test]
        public void ManagerSceneOfTheNewGroupIsOpenedOnce()
        {
            SceneGroup targetGroup = rig.CreateGroup("Level2", new[] { "Car" }, true);
            SceneChangePlan.OpenScenes open = new SceneChangePlan.OpenScenes();

            SceneChangePlan plan = new SceneChangePlan(targetGroup, open);

            Assert.AreEqual(SceneCoordinatorTestRig.ManagerScene, Names(plan.ManagerialToOpen));
        }

        private SceneChangePlan CreatePlan(string _openLevel, string _openShared, string _targetLevel, string _targetShared, bool _forceReload)
        {
            SceneGroup openGroup = rig.CreateGroup(_openLevel, Split(_openShared), _forceReload);
            SceneGroup targetGroup = rig.CreateGroup(_targetLevel, Split(_targetShared), _forceReload);
            SceneChangePlan.OpenScenes open = CreateOpenScenes(openGroup);
            return new SceneChangePlan(targetGroup, open);
        }

        private static SceneChangePlan.OpenScenes CreateOpenScenes(SceneGroup _openGroup)
        {
            SceneChangePlan.OpenScenes open = new SceneChangePlan.OpenScenes();
            open.MainScene = _openGroup.MainScene;
            open.ManagerialScenes = new HashSet<SceneReference>(_openGroup.ManagerialBootScenesList);
            open.ContextualScenes = new HashSet<SceneReference>(_openGroup.ContextualBootScenesList);
            return open;
        }

        private static string[] Split(string _names)
        {
            return _names.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
        }

        private static string Names(IEnumerable<SceneReference> _scenes)
        {
            return string.Join(",", _scenes.Select(scene => scene.SceneName).OrderBy(name => name, System.StringComparer.Ordinal));
        }
    }
}
