using AbstractPixel.Core.Editor;
using NUnit.Framework;

namespace AbstractPixel.SceneManagement.Editor.Tests
{
    /// <summary>
    /// The scene picker's status button and right-click menu as plain data (session 5A2, decisions 4 to 9): the chip's
    /// colour, what a click does in each case, the two-line tooltip, and which menu entries apply.
    /// </summary>
    public class ScenePickerStatusRulesTests
    {
        private const string GroupGuid = "guid-level2Group";
        private const string SampleGroupGuid = "guid-Sample 05 Area AGroup";

        private static ScenePickerRow SceneRow(SceneStatus _status, string _groupGuid, string _groupName)
        {
            ScenePickerRow row = new ScenePickerRow();
            row.Kind = ScenePickerRowKind.Scene;
            row.Title = "level 2";
            row.AssetGuid = "scene-level-2";
            row.Status = _status;
            row.GroupGuid = _groupGuid ?? string.Empty;
            row.GroupName = _groupName ?? string.Empty;
            return row;
        }

        private static ScenePickerRow GroupRow(SceneStatus _mainStatus, int _problems, bool _makeAddressableProblem)
        {
            ScenePickerRow row = new ScenePickerRow();
            row.Kind = ScenePickerRowKind.SceneGroup;
            row.Title = "level2Group";
            row.AssetGuid = GroupGuid;
            row.Status = _mainStatus;
            row.ProblemCount = _problems;
            row.FirstProblem = _problems > 0 ? "Scene 'level 2' is Not in build." : string.Empty;
            row.HasMakeAddressableProblem = _makeAddressableProblem;
            return row;
        }

        private static ScenePickerProjectFacts Facts(bool _toAddressables, string _setupProblem)
        {
            ScenePickerProjectFacts facts = new ScenePickerProjectFacts();
            facts.NewScenesGoToAddressables = _toAddressables;
            facts.AddressablesSetupProblem = _setupProblem;
            return facts;
        }

        [TestCase(SceneStatus.InBuild, ScenePickerChip.InBuild)]
        [TestCase(SceneStatus.Addressable, ScenePickerChip.Addressable)]
        [TestCase(SceneStatus.NotInBuild, ScenePickerChip.Problem)]
        [TestCase(SceneStatus.Missing, ScenePickerChip.Problem)]
        [TestCase(SceneStatus.NoScene, ScenePickerChip.Problem)]
        public void ASceneChipFollowsItsStatus(SceneStatus _status, ScenePickerChip _expected)
        {
            Assert.AreEqual(_expected, ScenePickerStatusRules.Chip(SceneRow(_status, GroupGuid, "level2Group")));
        }

        [TestCase(SceneStatus.InBuild)]
        [TestCase(SceneStatus.NotInBuild)]
        public void ASampleSceneIsGreyWhateverItsStatus(SceneStatus _status)
        {
            ScenePickerRow row = SceneRow(_status, SampleGroupGuid, "Sample 05 Area AGroup");
            row.IsSample = true;

            Assert.AreEqual(ScenePickerChip.Sample, ScenePickerStatusRules.Chip(row));
            StringAssert.StartsWith(ScenePickerStatusRules.SampleText + "\n", ScenePickerStatusRules.Tooltip(row, Facts(false, "")));
        }

        // status, has a group, new scenes to Addressables, Addressables setup problem -> click, target
        [TestCase(SceneStatus.InBuild, true, false, "", ScenePickerClickKind.OpenBuildProfilesSceneList, "")]
        [TestCase(SceneStatus.Addressable, true, false, "", ScenePickerClickKind.OpenAddressablesGroups, "")]
        [TestCase(SceneStatus.NotInBuild, true, false, "", ScenePickerClickKind.SelectAsset, GroupGuid)]
        [TestCase(SceneStatus.Missing, true, false, "", ScenePickerClickKind.SelectAsset, GroupGuid)]
        [TestCase(SceneStatus.NotInBuild, false, false, "", ScenePickerClickKind.OpenBuildProfilesSceneList, "")]
        [TestCase(SceneStatus.NotInBuild, false, true, "", ScenePickerClickKind.OpenAddressablesGroups, "")]
        [TestCase(SceneStatus.NotInBuild, true, true, "", ScenePickerClickKind.SelectAsset, GroupGuid)]
        [TestCase(SceneStatus.NotInBuild, true, true, AddressablesSetup.NoSettingsText, ScenePickerClickKind.OpenAddressablesGroups, "")]
        [TestCase(SceneStatus.NotInBuild, false, true, AddressablesSetup.NoDefaultGroupText, ScenePickerClickKind.OpenAddressablesGroups, "")]
        [TestCase(SceneStatus.NotInBuild, true, false, AddressablesSetup.NoSettingsText, ScenePickerClickKind.SelectAsset, GroupGuid)]
        public void ASceneStatusClickGoesWhereTheFixIs(SceneStatus _status, bool _hasGroup, bool _toAddressables, string _setupProblem, ScenePickerClickKind _expected, string _expectedTarget)
        {
            ScenePickerRow row = _hasGroup ? SceneRow(_status, GroupGuid, "level2Group") : SceneRow(_status, null, null);

            ScenePickerClick click = ScenePickerStatusRules.Click(row, Facts(_toAddressables, _setupProblem));

            Assert.AreEqual(_expected, click.Kind);
            Assert.AreEqual(_expectedTarget, click.TargetGuid);
        }

        [Test]
        public void ASceneSetLabelWithNoGroupGoesToWhereNewScenesGo()
        {
            ScenePickerRow row = SceneRow(SceneStatus.NotInBuild, null, null);
            row.LabelKind = ScenePickerLabelKind.SceneSet;
            row.LabelGuid = "guid-Level";
            row.Detail = "Level (scene set)";

            Assert.AreEqual(ScenePickerClickKind.OpenBuildProfilesSceneList, ScenePickerStatusRules.Click(row, Facts(false, "")).Kind);
            Assert.AreEqual(ScenePickerClickKind.OpenAddressablesGroups, ScenePickerStatusRules.Click(row, Facts(true, "")).Kind);
        }

        [Test]
        public void ASampleSceneClickSelectsItsSampleGroup()
        {
            ScenePickerRow row = SceneRow(SceneStatus.NotInBuild, SampleGroupGuid, "Sample 05 Area AGroup");
            row.IsSample = true;

            ScenePickerClick click = ScenePickerStatusRules.Click(row, Facts(true, AddressablesSetup.NoSettingsText));

            Assert.AreEqual(ScenePickerClickKind.SelectAsset, click.Kind);
            Assert.AreEqual(SampleGroupGuid, click.TargetGuid);
        }

        [TestCase(SceneStatus.InBuild, 0, ScenePickerChip.InBuild)]
        [TestCase(SceneStatus.Addressable, 0, ScenePickerChip.Addressable)]
        [TestCase(SceneStatus.InBuild, 1, ScenePickerChip.Problem)]
        [TestCase(SceneStatus.Addressable, 3, ScenePickerChip.Problem)]
        [TestCase(SceneStatus.NotInBuild, 1, ScenePickerChip.Problem)]
        public void AGroupChipShowsTheGroupCheck(SceneStatus _mainStatus, int _problems, ScenePickerChip _expected)
        {
            Assert.AreEqual(_expected, ScenePickerStatusRules.Chip(GroupRow(_mainStatus, _problems, false)));
        }

        // main status, problems, Make Addressable problem, Addressables setup problem -> click
        [TestCase(SceneStatus.InBuild, 0, false, "", ScenePickerClickKind.OpenDebuggerAtGroup)]
        [TestCase(SceneStatus.Addressable, 0, false, "", ScenePickerClickKind.OpenDebuggerAtGroup)]
        [TestCase(SceneStatus.InBuild, 2, false, "", ScenePickerClickKind.SelectAsset)]
        [TestCase(SceneStatus.InBuild, 1, true, "", ScenePickerClickKind.SelectAsset)]
        [TestCase(SceneStatus.InBuild, 1, true, AddressablesSetup.NoDefaultGroupText, ScenePickerClickKind.OpenAddressablesGroups)]
        [TestCase(SceneStatus.InBuild, 1, false, AddressablesSetup.NoDefaultGroupText, ScenePickerClickKind.SelectAsset)]
        public void AGroupStatusClickGoesToTheOverviewOrTheFix(SceneStatus _mainStatus, int _problems, bool _makeAddressable, string _setupProblem, ScenePickerClickKind _expected)
        {
            ScenePickerClick click = ScenePickerStatusRules.Click(GroupRow(_mainStatus, _problems, _makeAddressable), Facts(true, _setupProblem));

            Assert.AreEqual(_expected, click.Kind);
            if (_expected == ScenePickerClickKind.SelectAsset || _expected == ScenePickerClickKind.OpenDebuggerAtGroup)
            {
                Assert.AreEqual(GroupGuid, click.TargetGuid);
            }
        }

        [Test]
        public void ABootGroupIsYellowLikeTheDebuggerAndOpensItsRow()
        {
            ScenePickerRow row = GroupRow(SceneStatus.NoScene, 0, false);
            row.IsBootGroup = true;

            Assert.AreEqual(ScenePickerChip.Unused, ScenePickerStatusRules.Chip(row));
            Assert.AreEqual(ScenePickerClickKind.OpenDebuggerAtGroup, ScenePickerStatusRules.Click(row, Facts(false, "")).Kind);
            StringAssert.StartsWith(ScenePickerStatusRules.BootGroupText + "\n", ScenePickerStatusRules.Tooltip(row, Facts(false, "")));
        }

        [Test]
        public void ASampleGroupIsGreyAndSelectsItself()
        {
            ScenePickerRow row = GroupRow(SceneStatus.NotInBuild, 0, false);
            row.IsSample = true;

            Assert.AreEqual(ScenePickerChip.Sample, ScenePickerStatusRules.Chip(row));
            Assert.AreEqual(ScenePickerClickKind.SelectAsset, ScenePickerStatusRules.Click(row, Facts(false, "")).Kind);
        }

        [Test]
        public void TheTooltipNamesTheStatusThenWhatAClickDoes()
        {
            string notInBuild = ScenePickerStatusRules.Tooltip(SceneRow(SceneStatus.NotInBuild, GroupGuid, "level2Group"), Facts(false, ""));
            string inBuild = ScenePickerStatusRules.Tooltip(SceneRow(SceneStatus.InBuild, GroupGuid, "level2Group"), Facts(false, ""));
            string problems = ScenePickerStatusRules.Tooltip(GroupRow(SceneStatus.InBuild, 2, false), Facts(false, ""));
            string healthy = ScenePickerStatusRules.Tooltip(GroupRow(SceneStatus.InBuild, 0, false), Facts(false, ""));

            Assert.AreEqual("Not in build\nClick to open level2Group in the Inspector, where you can add it.", notInBuild);
            Assert.AreEqual("In build\nClick to open Build Profiles at the scene list.", inBuild);
            Assert.AreEqual("2 problems. First: Scene 'level 2' is Not in build.\nClick to open level2Group in the Inspector, where you can fix it.", problems);
            Assert.AreEqual("In build\nClick to open the Scene Groups Debugger at level2Group.", healthy);
        }

        [Test]
        public void ABuildProfileWithItsOwnSceneListIsNamedInTheTooltip()
        {
            ScenePickerProjectFacts facts = Facts(false, "");
            facts.SceneListProfileName = "Steam Deck";

            string tooltip = ScenePickerStatusRules.Tooltip(SceneRow(SceneStatus.InBuild, GroupGuid, "level2Group"), facts);

            StringAssert.EndsWith("Build Profiles at Steam Deck's scene list.", tooltip);
        }

        [Test]
        public void TheMenuGreysOutWhatDoesNotApply()
        {
            ScenePickerRow noGroup = SceneRow(SceneStatus.NotInBuild, null, null);
            ScenePickerRow brokenGroup = GroupRow(SceneStatus.NoScene, 1, false);
            brokenGroup.CanOpen = false;

            ScenePickerRowMenu sceneMenu = ScenePickerRowMenu.For(noGroup, false);
            ScenePickerRowMenu groupMenu = ScenePickerRowMenu.For(brokenGroup, false);
            ScenePickerRowMenu playMenu = ScenePickerRowMenu.For(SceneRow(SceneStatus.InBuild, GroupGuid, "level2Group"), true);

            Assert.IsTrue(sceneMenu.CanOpen && sceneMenu.CanOpenAdditively && sceneMenu.CanSelectScene && sceneMenu.CanCopyPath);
            Assert.IsFalse(sceneMenu.CanSelectGroup, "no group");
            Assert.IsFalse(groupMenu.CanOpen || groupMenu.CanOpenAdditively || groupMenu.CanSelectScene, "no main scene");
            Assert.IsTrue(groupMenu.CanSelectGroup && groupMenu.CanCopyPath);
            Assert.AreEqual(GroupGuid, groupMenu.GroupGuid);
            Assert.IsFalse(playMenu.CanOpen || playMenu.CanOpenAdditively, "Play mode");
            Assert.IsTrue(playMenu.CanSelectScene && playMenu.CanSelectGroup);
        }
    }
}
