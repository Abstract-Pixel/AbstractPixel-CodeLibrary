using System.Collections.Generic;
using AbstractPixel.Core;
using NUnit.Framework;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>
    /// "Open the group with its main scene" (session 4B, decision 6): which scenes open next to a scene opened on its
    /// own, and which scenes the tool closes when you switch groups. Rally-shaped settings: launch scene SplashScreen,
    /// start scene group MainMenu, a Level scene set with ManagerBootScene and GameBootScene.
    /// </summary>
    public class OpenGroupWithMainSceneTests
    {
        private readonly List<Object> createdObjects = new List<Object>();
        private SceneSystemSettings settings;

        [SetUp]
        public void CreateSettings()
        {
            SceneSet levelSet = ScriptableObject.CreateInstance<SceneSet>();
            createdObjects.Add(levelSet);
            levelSet.ManagerScenes.Add(new SceneReference("ManagerBootScene"));
            levelSet.SharedScenes.Add(new SceneReference("GameBootScene"));

            settings = ScriptableObject.CreateInstance<SceneSystemSettings>();
            createdObjects.Add(settings);
            settings.SceneSets.Add(levelSet);
            settings.DefaultSceneSet = levelSet;
            settings.LaunchScene = new SceneReference("SplashScreen");
            settings.StartSceneGroup = AddGroup("MainMenu", levelSet);
            AddGroup("Level2", levelSet);

            SceneSet emptySet = ScriptableObject.CreateInstance<SceneSet>();
            createdObjects.Add(emptySet);
            emptySet.ManagerScenes.Add(new SceneReference("ManagerBootScene"));
            settings.SceneSets.Add(emptySet);
            AddGroup("Puzzle", emptySet);
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

        [TestCase("Level2", "ManagerBootScene,GameBootScene")]
        [TestCase("MainMenu", "ManagerBootScene,GameBootScene")]
        [TestCase("Puzzle", "ManagerBootScene")]
        [TestCase("CarTest", "ManagerBootScene,GameBootScene")]
        [TestCase("GameBootScene", "MainMenu,ManagerBootScene")]
        [TestCase("ManagerBootScene", "MainMenu,GameBootScene")]
        [TestCase("SplashScreen", "")]
        public void OpensWhatPlayWouldLoad(string _openedScene, string _expected)
        {
            List<SceneReference> scenes = GroupScenesToOpen.For(settings, new SceneReference(_openedScene));

            Assert.AreEqual(_expected, Names(scenes));
        }

        [Test]
        public void NoSettingsOpensNothing()
        {
            List<SceneReference> scenes = GroupScenesToOpen.For(null, new SceneReference("Level2"));

            Assert.AreEqual(0, scenes.Count);
        }

        [Test]
        public void NoStartSceneGroupOpensNothingForASharedScene()
        {
            settings.StartSceneGroup = null;

            List<SceneReference> scenes = GroupScenesToOpen.For(settings, new SceneReference("GameBootScene"));

            Assert.AreEqual(0, scenes.Count);
        }

        // needed, open, opened by the tool -> open, close, remembered after
        [TestCase("ManagerBootScene,GameBootScene", "Level2", "", "ManagerBootScene,GameBootScene", "", "ManagerBootScene,GameBootScene")]
        [TestCase("ManagerBootScene,GameBootScene", "Level2,ManagerBootScene", "", "GameBootScene", "", "GameBootScene")]
        [TestCase("ManagerBootScene", "Puzzle,ManagerBootScene,GameBootScene", "ManagerBootScene,GameBootScene", "", "GameBootScene", "ManagerBootScene")]
        [TestCase("ManagerBootScene", "Puzzle,ManagerBootScene,GameBootScene", "ManagerBootScene", "", "", "ManagerBootScene")]
        [TestCase("", "SplashScreen,GameBootScene", "GameBootScene", "", "GameBootScene", "")]
        [TestCase("ManagerBootScene,GameBootScene", "Level2", "OldScene", "ManagerBootScene,GameBootScene", "", "ManagerBootScene,GameBootScene")]
        public void ClosesOnlyScenesTheToolOpenedThatTheNewGroupDoesNotNeed(string _needed, string _open, string _openedByTool, string _expectedOpen, string _expectedClose, string _expectedRemembered)
        {
            OpenedGroupScenesPlan plan = OpenedGroupScenesPlan.Compute(Scenes(_needed), Scenes(_open), Scenes(_openedByTool));

            Assert.AreEqual(_expectedOpen, Names(plan.ToOpen), "open");
            Assert.AreEqual(_expectedClose, Names(plan.ToClose), "close");
            Assert.AreEqual(_expectedRemembered, Names(plan.OpenedByToolAfter), "remembered");
        }

        private SceneGroup AddGroup(string _mainScene, SceneSet _set)
        {
            SceneGroup group = ScriptableObject.CreateInstance<SceneGroup>();
            group.name = _mainScene + "Group";
            createdObjects.Add(group);
            group.Initialize(_set.ManagerScenes, _set.SharedScenes, new SceneReference(_mainScene), true);
            group.SceneSet = _set;
            settings.SceneGroups.Add(group);
            return group;
        }

        private static List<SceneReference> Scenes(string _names)
        {
            List<SceneReference> scenes = new List<SceneReference>();
            if (string.IsNullOrEmpty(_names))
            {
                return scenes;
            }
            foreach (string sceneName in _names.Split(','))
            {
                scenes.Add(new SceneReference(sceneName));
            }
            return scenes;
        }

        private static string Names(List<SceneReference> _scenes)
        {
            List<string> names = new List<string>();
            foreach (SceneReference scene in _scenes)
            {
                names.Add(scene.SceneName);
            }
            return string.Join(",", names);
        }
    }
}
