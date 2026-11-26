using System.Collections.Generic;
using AbstractPixel.Core.Editor;
using NUnit.Framework;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>
    /// The scene picker's rows, search and label as plain data (session 5A, stage 2). The project is shaped like Rally
    /// Shift's: level groups on a "Level" scene set with ManagerBootScene and GameBootScene, a scene in no group, a
    /// library sample with a shared scene, a group with no main scene, a test runner scene and another package's scene.
    /// </summary>
    public class ScenePickerRowsTests
    {
        private const string LibrarySamples = "Packages/com.abstractpixel.library/Modules/SceneManagement/Samples/05 Persistent Player And Spawn Points/";

        private List<ScenePickerSceneInfo> scenes;
        private List<ScenePickerGroupInfo> groups;
        private List<ScenePickerSetInfo> sets;

        [SetUp]
        public void CreateProject()
        {
            scenes = new List<ScenePickerSceneInfo>
            {
                Scene("Level 10", "Assets/Levels/Level 10/Level 10.unity"),
                Scene("Level 2", "Assets/Levels/Level 2/Level 2.unity"),
                Scene("MainMenu", "Assets/Menus/MainMenu.unity"),
                Scene("CarTest", "Assets/Tests/Driving/CarTest.unity"),
                Scene("ManagerBootScene", "Assets/Boot/ManagerBootScene.unity"),
                Scene("GameBootScene", "Assets/Boot/GameBootScene.unity"),
                Scene("InitTestScene637", "Assets/InitTestScene637.unity"),
                Scene("Package Scene", "Packages/com.unity.other/Scenes/Package Scene.unity"),
                Scene("Loading Scene", "Packages/com.abstractpixel.library/Modules/SceneManagement/SceneScreens/ReadyMade/Loading Scene.unity"),
                Scene("Sample 05 Area A", LibrarySamples + "Sample 05 Area A.unity"),
                Scene("Sample 05 Shared", LibrarySamples + "Sample 05 Shared.unity")
            };

            ScenePickerSetInfo levelSet = new ScenePickerSetInfo("Level", "guid-Level");
            levelSet.SceneGuids.Add(GuidOf("ManagerBootScene"));
            levelSet.SceneGuids.Add(GuidOf("GameBootScene"));
            sets = new List<ScenePickerSetInfo> { levelSet };

            groups = new List<ScenePickerGroupInfo>
            {
                Group("Level 10Group", "Level 10", "Level", false),
                Group("Level 2Group", "Level 2", "Level", false),
                Group("MainMenuGroup", "MainMenu", "Level", false),
                Group("Sample 05 Area AGroup", "Sample 05 Area A", string.Empty, true),
                Group("BrokenGroup", string.Empty, "Level", false)
            };
            groups[3].OtherSceneGuids.Add(GuidOf("Sample 05 Shared"));
        }

        [TestCase("CarTest", "No group")]
        [TestCase("Level 2", "Level 2Group")]
        [TestCase("MainMenu", "MainMenuGroup")]
        [TestCase("ManagerBootScene", "Level (scene set)")]
        [TestCase("GameBootScene", "Level (scene set)")]
        [TestCase("Sample 05 Area A", "Sample 05 Area AGroup")]
        [TestCase("Sample 05 Shared", "Shared by Sample 05 Area AGroup")]
        public void ASceneRowNamesItsGroup(string _scene, string _expectedDetail)
        {
            ScenePickerRow row = FindRow(ScenePickerRows.ForScenes(scenes, groups, sets), _scene);

            Assert.IsNotNull(row, _scene + " is listed");
            Assert.AreEqual(_expectedDetail, row.Detail);
        }

        // scene -> label kind, label target GUID, label tooltip
        [TestCase("Level 2", ScenePickerLabelKind.SceneGroup, "guid-Level 2Group", "Select Level 2Group")]
        [TestCase("ManagerBootScene", ScenePickerLabelKind.SceneSet, "guid-Level", "Select Level")]
        [TestCase("Sample 05 Shared", ScenePickerLabelKind.SceneGroup, "guid-Sample 05 Area AGroup", "Select Sample 05 Area AGroup")]
        [TestCase("CarTest", ScenePickerLabelKind.None, "", "")]
        public void ASceneLabelSelectsWhatItNames(string _scene, ScenePickerLabelKind _kind, string _guid, string _tooltip)
        {
            ScenePickerRow row = FindRow(ScenePickerRows.ForScenes(scenes, groups, sets), _scene);

            Assert.AreEqual(_kind, row.LabelKind);
            Assert.AreEqual(_guid, row.LabelGuid);
            Assert.AreEqual(_tooltip, ScenePickerLabelRules.Tooltip(row));
        }

        [Test]
        public void NoGroupIsPlainTextThatCannotBeClicked()
        {
            ScenePickerRow row = FindRow(ScenePickerRows.ForScenes(scenes, groups, sets), "CarTest");

            Assert.AreEqual(ScenePickerRows.NoGroupText, row.Detail);
            Assert.IsFalse(ScenePickerLabelRules.IsClickable(row));
        }

        [Test]
        public void ASceneSharedBySeveralGroupsSelectsTheFirst()
        {
            groups[1].OtherSceneGuids.Add(GuidOf("CarTest"));
            groups[0].OtherSceneGuids.Add(GuidOf("CarTest"));

            ScenePickerRow row = FindRow(ScenePickerRows.ForScenes(scenes, groups, sets), "CarTest");

            Assert.AreEqual("Shared by Level 10Group, Level 2Group", row.Detail);
            Assert.AreEqual("guid-Level 10Group", row.LabelGuid);
            Assert.AreEqual("guid-Level 10Group", row.GroupGuid, "its status and menu use the same group");
            Assert.AreEqual("Shared by Level 10Group, Level 2Group\nSelect Level 10Group", ScenePickerLabelRules.Tooltip(row));
        }

        [Test]
        public void AGroupLabelSelectsItsSceneSetAndAGroupWithNoSetHasPlainText()
        {
            groups[2].SceneSetName = string.Empty;
            List<ScenePickerRow> rows = ScenePickerRows.ForGroups(groups);

            ScenePickerRow level = FindRow(rows, "Level 2Group");
            ScenePickerRow noSet = FindRow(rows, "MainMenuGroup");

            Assert.AreEqual(ScenePickerLabelKind.SceneSet, level.LabelKind);
            Assert.AreEqual("guid-Level", level.LabelGuid);
            Assert.AreEqual(ScenePickerRows.NoSceneSetText, noSet.Detail);
            Assert.IsFalse(ScenePickerLabelRules.IsClickable(noSet));
        }

        [Test]
        public void GroupRowsShowTheDebuggerNumberAndSceneRowsShowNone()
        {
            groups[0].Number = 3;
            groups[1].Number = 1;

            List<ScenePickerRow> groupRows = ScenePickerRows.ForGroups(groups);
            List<ScenePickerRow> sceneRows = ScenePickerRows.ForScenes(scenes, groups, sets);

            Assert.AreEqual(3, FindRow(groupRows, "Level 10Group").Number);
            Assert.AreEqual(1, FindRow(groupRows, "Level 2Group").Number);
            Assert.AreEqual(0, FindRow(groupRows, "Sample 05 Area AGroup").Number, "a sample group is not in the Debugger");
            Assert.IsTrue(sceneRows.TrueForAll(_row => _row.Number == 0));
        }

        [Test]
        public void RecentRowsAreNumberedByRecencyAndTheFullListKeepsItsNumbers()
        {
            groups[0].Number = 3;
            groups[1].Number = 1;
            List<ScenePickerRow> rows = ScenePickerRows.ForGroups(groups);
            List<string> recent = new List<string> { "guid-MainMenuGroup", "guid-Level 10Group" };

            List<ScenePickerRow> withRecent = ScenePickerRecent.WithRecentFirst(rows, recent);

            Assert.AreEqual("MainMenuGroup", withRecent[1].Title);
            Assert.AreEqual(1, withRecent[1].Number, "1 is the last used");
            Assert.AreEqual("Level 10Group", withRecent[2].Title);
            Assert.AreEqual(2, withRecent[2].Number);
            Assert.AreEqual(3, FindRow(rows, "Level 10Group").Number, "the full list still shows the Debugger number");
        }

        [TestCase("InitTestScene637")]
        [TestCase("Package Scene")]
        [TestCase("Loading Scene")]
        public void TestRunnerAndOtherPackageScenesAreLeftOut(string _scene)
        {
            Assert.IsNull(FindRow(ScenePickerRows.ForScenes(scenes, groups, sets), _scene));
        }

        [Test]
        public void ScenesOfTheGameComeFirstThenScenesInNoGroupThenSamplesAndNamesSortAsPeopleReadThem()
        {
            List<ScenePickerRow> rows = ScenePickerRows.ForScenes(scenes, groups, sets);

            Assert.AreEqual("GameBootScene,Level 2,Level 10,MainMenu,ManagerBootScene,[Scenes in no group],CarTest,[Samples],Sample 05 Area A,Sample 05 Shared", Titles(rows));
            Assert.IsTrue(FindRow(rows, "Sample 05 Shared").IsSample);
            Assert.IsFalse(FindRow(rows, "CarTest").IsSample);
        }

        [Test]
        public void GroupRowsShowTheirMainSceneAndSceneSetWithSamplesLast()
        {
            List<ScenePickerRow> rows = ScenePickerRows.ForGroups(groups);

            Assert.AreEqual("BrokenGroup,Level 2Group,Level 10Group,MainMenuGroup,[Samples],Sample 05 Area AGroup", Titles(rows));
            Assert.AreEqual("Level", FindRow(rows, "Level 2Group").Detail, "the label names the scene set");
            Assert.AreEqual("Level 2", FindRow(rows, "Level 2Group").MainSceneName, "the main scene moves to the tooltip");
            Assert.IsTrue(FindRow(rows, "Level 2Group").CanOpen);
        }

        [Test]
        public void AGroupWithAMissingMainSceneCanBeSelectedButNotOpened()
        {
            ScenePickerRow row = FindRow(ScenePickerRows.ForGroups(groups), "BrokenGroup");

            Assert.IsFalse(row.CanOpen);
            Assert.AreEqual(string.Empty, row.MainSceneName);
            Assert.IsFalse(string.IsNullOrEmpty(row.AssetGuid), "Select still finds the group asset");
        }

        [TestCase("level 2", "Level 2")]
        [TestCase("LEVEL 10", "Level 10")]
        [TestCase("driving car", "[Scenes in no group],CarTest")]
        [TestCase("tests CARTEST", "[Scenes in no group],CarTest")]
        [TestCase("10group", "Level 10")]
        [TestCase("menu  main", "MainMenu")]
        [TestCase("boot manager", "ManagerBootScene")]
        [TestCase("zzz", "")]
        [TestCase("shared", "[Samples],Sample 05 Shared")]
        public void SearchMatchesEveryWordInNameFolderOrGroupIgnoringCase(string _query, string _expected)
        {
            List<ScenePickerRow> rows = ScenePickerSearch.Filter(ScenePickerRows.ForScenes(scenes, groups, sets), _query);

            Assert.AreEqual(_expected, Titles(rows));
        }

        [Test]
        public void RowsWhoseNameHoldsEveryWordComeBeforeFolderOrGroupMatches()
        {
            scenes.Add(Scene("Level 1", "Assets/Levels/Stage1/Level 1.unity"));
            scenes[1].Path = "Assets/Levels/Stage1/Level 2.unity";

            List<ScenePickerRow> rows = ScenePickerSearch.Filter(ScenePickerRows.ForScenes(scenes, groups, sets), "lev 1");

            Assert.AreEqual("Level 10,Level 2,[Scenes in no group],Level 1", Titles(rows), "Level 2 matches only through its Stage1 folder, so it follows Level 10");
        }

        [Test]
        public void AnEmptySearchKeepsEveryRow()
        {
            List<ScenePickerRow> all = ScenePickerRows.ForScenes(scenes, groups, sets);

            Assert.AreEqual(Titles(all), Titles(ScenePickerSearch.Filter(all, "   ")));
        }

        [TestCase("levels", "Level 2Group,Level 10Group")]
        [TestCase("broken", "BrokenGroup")]
        public void GroupSearchMatchesNameAndFolder(string _query, string _expected)
        {
            List<ScenePickerRow> rows = ScenePickerSearch.Filter(ScenePickerRows.ForGroups(groups), _query);

            Assert.AreEqual(_expected, Titles(rows));
        }

        [TestCase("Level 2", "Level 2Group")]
        [TestCase("ManagerBootScene,GameBootScene,Level 10", "Level 10Group")]
        [TestCase("CarTest,ManagerBootScene", "CarTest (no group)")]
        [TestCase("ManagerBootScene", "ManagerBootScene (no group)")]
        [TestCase("Sample 05 Shared,Sample 05 Area A", "Sample 05 Area AGroup")]
        [TestCase("", "Untitled (no group)")]
        public void TheButtonNamesTheOpenGroupOrTheSceneInNoGroup(string _openScenes, string _expected)
        {
            List<KeyValuePair<string, string>> open = new List<KeyValuePair<string, string>>();
            foreach (string name in _openScenes.Split(','))
            {
                if (name.Length > 0)
                {
                    open.Add(new KeyValuePair<string, string>(name, GuidOf(name)));
                }
            }

            Assert.AreEqual(_expected, ScenePickerLabel.For(open, groups));
        }

        [Test]
        public void ASceneRenamedWhileThePickerIsOpenStillOpensAndADeletedOneOpensNothing()
        {
            ScenePickerRow row = FindRow(ScenePickerRows.ForScenes(scenes, groups, sets), "CarTest");
            Dictionary<string, string> project = new Dictionary<string, string> { { row.AssetGuid, "Assets/Tests/Driving/CarTest Renamed.unity" } };

            string renamed = ScenePickerTarget.CurrentPath(row, _guid => Lookup(project, _guid), _path => true);
            project.Clear();
            string deleted = ScenePickerTarget.CurrentPath(row, _guid => Lookup(project, _guid), _path => true);

            Assert.AreEqual("Assets/Tests/Driving/CarTest Renamed.unity", renamed);
            Assert.IsNull(deleted);
            Assert.IsNull(ScenePickerTarget.CurrentPath(ScenePickerRow.Heading("Samples"), _guid => "x", _path => true));
        }

        [Test]
        public void RecentRowsComeFirstNewestFirstOnlyFromThisTab()
        {
            List<ScenePickerRow> rows = ScenePickerRows.ForScenes(scenes, groups, sets);
            List<string> recent = new List<string> { GuidOf("CarTest"), "guid-Level 2Group", GuidOf("Level 10"), "deleted-guid" };

            List<ScenePickerRow> withRecent = ScenePickerRecent.WithRecentFirst(rows, recent);

            StringAssert.StartsWith("[Recent],CarTest,Level 10,[All scenes],GameBootScene", Titles(withRecent));
            Assert.AreEqual(Titles(rows), Titles(ScenePickerRecent.WithRecentFirst(rows, new List<string>())));
        }

        [Test]
        public void UnderRecentAListThatStartsWithItsOwnHeadingGetsNoEmptyAllHeading()
        {
            List<ScenePickerRow> rows = ScenePickerRows.ForScenes(new List<ScenePickerSceneInfo> { Scene("CarTest", "Assets/Tests/CarTest.unity") }, groups, sets);

            List<ScenePickerRow> withRecent = ScenePickerRecent.WithRecentFirst(rows, new List<string> { GuidOf("CarTest") });

            Assert.AreEqual("[Recent],CarTest,[Scenes in no group],CarTest", Titles(withRecent));
        }

        [TestCase("Level 10", "lev 10", "<b>Lev</b>el <b>10</b>")]
        [TestCase("MainMenu", "MENU", "Main<b>Menu</b>")]
        [TestCase("Car<b>Test", "car", "Car<b>Test")]
        [TestCase("CarTest", "", "CarTest")]
        public void SearchedWordsShowInBoldInTheName(string _title, string _query, string _expected)
        {
            Assert.AreEqual(_expected, ScenePickerSearch.BoldMatches(_title, ScenePickerSearch.Words(_query)));
        }

        private static string Lookup(Dictionary<string, string> _project, string _guid)
        {
            string path;
            _project.TryGetValue(_guid, out path);
            return path ?? string.Empty;
        }

        private static ScenePickerSceneInfo Scene(string _name, string _path)
        {
            ScenePickerSceneInfo scene = new ScenePickerSceneInfo(_name, _path, GuidOf(_name));
            scene.Status = SceneStatus.InBuild;
            return scene;
        }

        private static ScenePickerGroupInfo Group(string _name, string _mainScene, string _setName, bool _isSample)
        {
            string folder = "Assets/Other/";
            if (_isSample)
            {
                folder = LibrarySamples;
            }
            else if (_name.StartsWith("Level"))
            {
                folder = "Assets/Levels/";
            }
            ScenePickerGroupInfo group = new ScenePickerGroupInfo(_name, folder + _name + ".asset", "guid-" + _name);
            group.IsSample = _isSample;
            group.SceneSetName = _setName;
            group.SceneSetGuid = _setName.Length > 0 ? "guid-" + _setName : string.Empty;
            if (_mainScene.Length > 0)
            {
                group.MainSceneName = _mainScene;
                group.MainSceneGuid = GuidOf(_mainScene);
                group.MainSceneStatus = SceneStatus.InBuild;
            }
            else
            {
                group.MainSceneStatus = SceneStatus.NoScene;
            }
            return group;
        }

        private static string GuidOf(string _sceneName)
        {
            return "scene-" + _sceneName.ToLowerInvariant().Replace(' ', '-');
        }

        private static ScenePickerRow FindRow(List<ScenePickerRow> _rows, string _title)
        {
            foreach (ScenePickerRow row in _rows)
            {
                if (!row.IsHeading && row.Title == _title)
                {
                    return row;
                }
            }
            return null;
        }

        private static string Titles(List<ScenePickerRow> _rows)
        {
            List<string> titles = new List<string>();
            foreach (ScenePickerRow row in _rows)
            {
                titles.Add(row.IsHeading ? "[" + row.Title + "]" : row.Title);
            }
            return string.Join(",", titles);
        }
    }
}
