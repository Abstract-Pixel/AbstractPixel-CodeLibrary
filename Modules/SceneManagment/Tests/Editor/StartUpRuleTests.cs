using System.Collections.Generic;
using AbstractPixel.Core;
using NUnit.Framework;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>
    /// Which scene group start-up opens (session 4B, decision 2). Settings shaped like Rally Shift's: a launch scene
    /// "SplashScreen", the start scene group "MainMenu", level groups, and one scene set with ManagerBootScene and
    /// GameBootScene. Open scenes are listed active first.
    /// </summary>
    public class StartUpRuleTests
    {
        private const string LaunchScene = "SplashScreen";
        private const string ManagerScene = "ManagerBootScene";
        private const string SharedScene = "GameBootScene";

        private readonly List<Object> createdObjects = new List<Object>();
        private SceneSystemSettings settings;
        private SceneSet levelSet;

        [SetUp]
        public void CreateSettings()
        {
            levelSet = ScriptableObject.CreateInstance<SceneSet>();
            levelSet.name = "Level";
            levelSet.ManagerScenes.Add(new SceneReference(ManagerScene));
            levelSet.SharedScenes.Add(new SceneReference(SharedScene));
            levelSet.ResetSharedScenes = true;
            createdObjects.Add(levelSet);

            settings = ScriptableObject.CreateInstance<SceneSystemSettings>();
            createdObjects.Add(settings);
            settings.SceneSets.Add(levelSet);
            settings.DefaultSceneSet = levelSet;
            settings.LaunchScene = new SceneReference(LaunchScene);
            settings.StartSceneGroup = AddGroup("MainMenu");
            AddGroup("Level2");
            AddGroup("Level7");
        }

        [TearDown]
        public void DestroyCreatedObjects()
        {
            foreach (Object createdObject in createdObjects)
            {
                Object.DestroyImmediate(createdObject);
            }
            createdObjects.Clear();
        }

        [TestCase("SplashScreen", StartUpChoiceKind.BehindLaunchScene, "MainMenu")]
        [TestCase("SplashScreen,MainMenu", StartUpChoiceKind.BehindLaunchScene, "MainMenu")]
        [TestCase("Level2", StartUpChoiceKind.GroupOfOpenMainScene, "Level2")]
        [TestCase("Level2,ManagerBootScene,GameBootScene", StartUpChoiceKind.GroupOfOpenMainScene, "Level2")]
        [TestCase("ManagerBootScene,Level7", StartUpChoiceKind.GroupOfOpenMainScene, "Level7")]
        [TestCase("MainMenu", StartUpChoiceKind.GroupOfOpenMainScene, "MainMenu")]
        [TestCase("GameBootScene", StartUpChoiceKind.StartSceneGroup, "MainMenu")]
        [TestCase("ManagerBootScene,GameBootScene", StartUpChoiceKind.StartSceneGroup, "MainMenu")]
        public void PicksTheGroup(string _openScenes, StartUpChoiceKind _expectedKind, string _expectedMainScene)
        {
            StartUpChoice choice = StartUpRule.Choose(settings, Scenes(_openScenes));

            Assert.AreEqual(_expectedKind, choice.Kind);
            Assert.IsNotNull(choice.Group, "a group from the settings");
            Assert.AreEqual(_expectedMainScene, choice.Group.MainScene.SceneName);
            Assert.IsEmpty(choice.ErrorMessage);
        }

        [TestCase("Level2,Level7", "Level2")]
        [TestCase("Level7,ManagerBootScene,Level2", "Level7")]
        public void TwoMainScenesOpenLogsAnErrorAndUsesTheActiveOne(string _openScenes, string _expectedMainScene)
        {
            StartUpChoice choice = StartUpRule.Choose(settings, Scenes(_openScenes));

            Assert.AreEqual(StartUpChoiceKind.GroupOfOpenMainScene, choice.Kind);
            Assert.AreEqual(_expectedMainScene, choice.Group.MainScene.SceneName);
            StringAssert.Contains("Level2", choice.ErrorMessage);
            StringAssert.Contains("Level7", choice.ErrorMessage);
        }

        [Test]
        public void OnlyASceneSetSceneOpenLogsWhyItOpensTheStartSceneGroup()
        {
            StartUpChoice choice = StartUpRule.Choose(settings, Scenes(SharedScene));

            StringAssert.Contains(SharedScene, choice.InfoMessage);
            StringAssert.Contains("MainMenu", choice.InfoMessage);
            Assert.IsEmpty(choice.WarningMessage);
        }

        [TestCase("CarTest")]
        [TestCase("CarTest,ManagerBootScene,GameBootScene")]
        public void ASceneInNoGroupGetsATemporaryGroupFromTheDefaultSceneSet(string _openScenes)
        {
            StartUpChoice choice = StartUpRule.Choose(settings, Scenes(_openScenes));

            Assert.AreEqual(StartUpChoiceKind.TemporaryGroup, choice.Kind);
            Assert.IsNull(choice.Group, "the caller builds the temporary group");
            Assert.AreEqual("CarTest", choice.MainScene.SceneName);
            Assert.AreSame(levelSet, choice.SceneSet);
            StringAssert.Contains("CarTest", choice.WarningMessage);
            StringAssert.Contains("Create Scene Group", choice.WarningMessage);
        }

        [Test]
        public void NoSettingsAssetDoesNothingAndSaysSo()
        {
            StartUpChoice choice = StartUpRule.Choose(null, Scenes("Level2"));

            Assert.AreEqual(StartUpChoiceKind.DoNothing, choice.Kind);
            Assert.IsNull(choice.Group);
            StringAssert.Contains("settings", choice.ErrorMessage);
        }

        [Test]
        public void LaunchSceneWithoutAStartSceneGroupDoesNothingAndSaysSo()
        {
            settings.StartSceneGroup = null;

            StartUpChoice choice = StartUpRule.Choose(settings, Scenes(LaunchScene));

            Assert.AreEqual(StartUpChoiceKind.DoNothing, choice.Kind);
            StringAssert.Contains("start scene group", choice.ErrorMessage);
        }

        [Test]
        public void WithoutALaunchSceneTheSplashIsASceneInNoGroup()
        {
            settings.LaunchScene = new SceneReference(string.Empty);

            StartUpChoice choice = StartUpRule.Choose(settings, Scenes(LaunchScene));

            Assert.AreEqual(StartUpChoiceKind.TemporaryGroup, choice.Kind);
        }

        [Test]
        public void AMainSceneMatchesByGuidWhenItsNameIsStale()
        {
            SceneGroup renamed = AddGroup("OldName");
            renamed.MainScene = new SceneReference("OldName", "guid-level-9", "Assets/Levels/OldName.unity");
            List<SceneReference> open = new List<SceneReference> { new SceneReference("NewName", "guid-level-9", "Assets/Levels/NewName.unity") };

            StartUpChoice choice = StartUpRule.Choose(settings, open);

            Assert.AreEqual(StartUpChoiceKind.GroupOfOpenMainScene, choice.Kind);
            Assert.AreSame(renamed, choice.Group);
        }

        [Test]
        public void AMainSceneMatchesByPathInABuildWhereOpenScenesHaveNoGuid()
        {
            SceneGroup group = AddGroup("Level9");
            group.MainScene = new SceneReference("Level9", "guid-level-9", "Assets/A/Level9.unity");
            SceneGroup sameNameElsewhere = AddGroup("Level9");
            sameNameElsewhere.MainScene = new SceneReference("Level9", "guid-other", "Assets/B/Level9.unity");
            List<SceneReference> open = new List<SceneReference> { new SceneReference("Level9", string.Empty, "Assets/B/Level9.unity") };

            StartUpChoice choice = StartUpRule.Choose(settings, open);

            Assert.AreSame(sameNameElsewhere, choice.Group);
        }

        [Test]
        public void ASharedSceneOfASampleSceneGroupOpensThatSampleGroupNotTheStartSceneGroup()
        {
            SceneGroup sample = AddSampleGroup("SampleArea", "SampleShared");

            StartUpChoice choice = StartUpRule.Choose(settings, Scenes("SampleShared"));

            Assert.AreSame(sample, choice.Group, "the sample scene group that uses the scene, not " + (choice.Group == null ? "none" : choice.Group.name));
            Assert.AreEqual("SampleArea", choice.MainScene.SceneName);
            Assert.AreEqual(StartUpChoiceKind.SampleSceneGroupOfOpenScene, choice.Kind);
            StringAssert.Contains("sample scene group", choice.InfoMessage);
        }

        [Test]
        public void ASampleLaunchSceneHoldsWithItsSampleGroupBehindIt()
        {
            SceneGroup sample = AddSampleGroup("SampleMenu", "SampleShared");
            sample.SampleLaunchScene = new SceneReference("SampleSplash");

            StartUpChoice choice = StartUpRule.Choose(settings, Scenes("SampleSplash"));

            Assert.AreEqual(StartUpChoiceKind.BehindLaunchScene, choice.Kind);
            Assert.AreSame(sample, choice.Group, "the sample scene group that names the launch scene");
            Assert.IsEmpty(choice.ErrorMessage);
        }

        [Test]
        public void TheProjectLaunchSceneWinsOverASampleLaunchScene()
        {
            SceneGroup sample = AddSampleGroup("SampleMenu", "SampleShared");
            sample.SampleLaunchScene = new SceneReference("SampleSplash");

            StartUpChoice choice = StartUpRule.Choose(settings, Scenes("SampleSplash,SplashScreen"));

            Assert.AreEqual(StartUpChoiceKind.BehindLaunchScene, choice.Kind);
            Assert.AreEqual("MainMenu", choice.Group.MainScene.SceneName);
        }

        [Test]
        public void AGameSceneGroupNeverSetsALaunchScene()
        {
            SceneGroup game = AddGroup("Level4");
            game.SampleLaunchScene = new SceneReference("GameSplash");

            StartUpChoice choice = StartUpRule.Choose(settings, Scenes("GameSplash"));
            bool holds = StartUpRule.ShouldHoldLaunchScene(settings, Scenes("GameSplash"));

            Assert.AreNotEqual(StartUpChoiceKind.BehindLaunchScene, choice.Kind);
            Assert.IsFalse(holds);
        }

        [Test]
        public void ASampleLaunchSceneIsHeld()
        {
            SceneGroup sample = AddSampleGroup("SampleMenu", "SampleShared");
            sample.SampleLaunchScene = new SceneReference("SampleSplash");

            bool holds = StartUpRule.ShouldHoldLaunchScene(settings, Scenes("SampleSplash"));

            Assert.IsTrue(holds);
        }

        // Scene system S, Lab: on Unity 7 the Editor lists the scene Play opens as not loaded yet at BeforeSceneLoad, so
        // the hold must be decided again after the first scenes load instead of being given up.
        [TestCase(true, false, true, true)]
        [TestCase(true, true, true, false)]
        [TestCase(true, false, false, false)]
        [TestCase(false, false, true, false)]
        public void AListedSceneNotLoadedYetInTheEditorDefersTheHoldDecision(bool _isValid, bool _isLoaded, bool _isEditor, bool _expected)
        {
            Assert.AreEqual(_expected, StartUpRule.MayStillBeOpening(_isValid, _isLoaded, _isEditor));
        }

        // Scene system S, Lab: on Unity 7 the scenes open in edit mode are still loading when start-up looks, so a scene
        // that was loaded in edit mode counts as opening even before it reports loaded; one left unloaded never does.
        [TestCase(true, false, true, true, true)]
        [TestCase(true, false, true, false, false)]
        [TestCase(true, true, true, false, true)]
        [TestCase(false, false, true, true, false)]
        [TestCase(true, false, false, false, true)]
        public void ASceneLoadedInEditModeCountsAsOpening(bool _isValid, bool _isLoaded, bool _isEditor, bool _wasLoadedInEditMode, bool _expected)
        {
            Assert.AreEqual(_expected, StartUpRule.CountsAsOpening(_isValid, _isLoaded, _isEditor, _wasLoadedInEditMode));
        }

        [Test]
        public void NoOpenSceneDoesNothing()
        {
            StartUpChoice choice = StartUpRule.Choose(settings, new List<SceneReference>());

            Assert.AreEqual(StartUpChoiceKind.DoNothing, choice.Kind);
        }

        private SceneGroup AddGroup(string _mainSceneName)
        {
            SceneGroup group = ScriptableObject.CreateInstance<SceneGroup>();
            group.name = _mainSceneName + "Group";
            createdObjects.Add(group);
            group.Initialize(levelSet.ManagerScenes, levelSet.SharedScenes, new SceneReference(_mainSceneName), true);
            group.SceneSet = levelSet;
            settings.SceneGroups.Add(group);
            return group;
        }

        // A sample scene group (ADR 0014): no scene set, one shared scene of its own.
        private SceneGroup AddSampleGroup(string _mainSceneName, string _sharedSceneName)
        {
            SceneGroup group = ScriptableObject.CreateInstance<SceneGroup>();
            group.name = _mainSceneName + "Group";
            createdObjects.Add(group);
            List<SceneReference> shared = new List<SceneReference> { new SceneReference(_sharedSceneName) };
            group.Initialize(new List<SceneReference>(), shared, new SceneReference(_mainSceneName), false);
            group.IsSample = true;
            settings.SceneGroups.Add(group);
            return group;
        }

        private static List<SceneReference> Scenes(string _commaSeparatedNames)
        {
            List<SceneReference> scenes = new List<SceneReference>();
            foreach (string sceneName in _commaSeparatedNames.Split(','))
            {
                scenes.Add(new SceneReference(sceneName));
            }
            return scenes;
        }
    }
}
