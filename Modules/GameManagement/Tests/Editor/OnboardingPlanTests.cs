using System.Collections.Generic;
using AbstractPixel.GameManagement.Editor;
using NUnit.Framework;

namespace AbstractPixel.GameManagement.Tests
{
    public class OnboardingPlanTests
    {
        [TestCase("Assets/GameManagement", null)]
        [TestCase("Assets", null)]
        [TestCase("Assets/My Game/Settings/", null)]
        [TestCase("", "Type a folder inside Assets/.")]
        [TestCase("   ", "Type a folder inside Assets/.")]
        [TestCase("Packages/com.abstractpixel.library", "Must be inside Assets/.")]
        [TestCase("Library/Cache", "Must be inside Assets/.")]
        [TestCase("Assets/../Outside", "Use / between folders and no : * ? \" < > | characters.")]
        [TestCase("Assets/A:B", "Use / between folders and no : * ? \" < > | characters.")]
        public void FolderProblem_NamesWhatIsWrong(string _folder, string _expected)
        {
            Assert.AreEqual(_expected, OnboardingChoices.FolderProblem(_folder));
        }

        [Test]
        public void ListRank_PutsSubStatesAboveTheirParents()
        {
            Assert.Less(OnboardingMenuInfo.ListRank("Settings"), OnboardingMenuInfo.ListRank("Pause"));
            Assert.Less(OnboardingMenuInfo.ListRank("Pause"), OnboardingMenuInfo.ListRank("Play"));
            Assert.Less(OnboardingMenuInfo.ListRank("Game Over"), OnboardingMenuInfo.ListRank("Settings"));
            Assert.AreEqual(-1, OnboardingMenuInfo.ListRank("Shop"));
        }

        [Test]
        public void Defaults_TickMainMenuPauseAndGameOverOnly()
        {
            OnboardingChoices choices = new OnboardingChoices();
            List<string> names = OnboardingPlan.NewStateNames(choices, false);
            CollectionAssert.AreEqual(new[] { "Play", "Main Menu", "Pause", "Game Over", "Settings" }, names);
        }

        [Test]
        public void SettingsState_NeedsAMenuThatAsksForIt()
        {
            OnboardingChoices choices = new OnboardingChoices();
            choices.Get(OnboardingMenuKind.Pause).IncludeSettings = false;
            CollectionAssert.DoesNotContain(OnboardingPlan.NewStateNames(choices, false), "Settings");
            Assert.IsFalse(choices.MakesSettingsMenu);
        }

        [Test]
        public void SettingsState_IgnoresAMenuThatCannotHaveOne()
        {
            OnboardingChoices choices = new OnboardingChoices();
            choices.Get(OnboardingMenuKind.Pause).IncludeSettings = false;
            choices.Get(OnboardingMenuKind.GameOver).IncludeSettings = true;
            Assert.AreEqual(0, choices.SettingsParents().Count);
        }

        [Test]
        public void LockedMenus_AreNotMadeAgain()
        {
            OnboardingChoices choices = new OnboardingChoices();
            choices.Get(OnboardingMenuKind.MainMenu).Locked = true;
            CollectionAssert.DoesNotContain(OnboardingPlan.NewStateNames(choices, true), "Main Menu");
            CollectionAssert.DoesNotContain(OnboardingPlan.NewStateNames(choices, true), "Play");
        }

        // batch, playing, tests running, shown this session, turned off, project has settings -> opens
        [TestCase(false, false, false, false, false, false, true)]
        [TestCase(true, false, false, false, false, false, false)]
        [TestCase(false, true, false, false, false, false, false)]
        [TestCase(false, false, true, false, false, false, false)]
        [TestCase(false, false, false, true, false, false, false)]
        [TestCase(false, false, false, false, true, false, false)]
        [TestCase(false, false, false, false, false, true, false)]
        public void AutoOpen_OnlyOpensOncePerSessionInANewProjectOutsideBatchPlayAndTests(bool _batch, bool _playing, bool _tests, bool _shown, bool _off, bool _hasSettings, bool _expected)
        {
            Assert.AreEqual(_expected, OnboardingAutoOpen.ShouldOpen(_batch, _playing, _tests, _shown, _off, _hasSettings));
        }

        [Test]
        public void ReadyLines_ListTheChoicesInPlainWords()
        {
            OnboardingChoices choices = new OnboardingChoices();
            choices.SettingsFolder = "Assets/My Game/";
            choices.Get(OnboardingMenuKind.GameOver).UseOwnMenu = true;
            List<string> lines = OnboardingPlan.ReadyLines(choices, false);
            Assert.AreEqual("Settings asset in Assets/My Game", lines[0]);
            Assert.AreEqual("5 new states in the state list: Play, Main Menu, Pause, Game Over, Settings", lines[1]);
            Assert.AreEqual("Main Menu and Pause menus, copied from the library", lines[2]);
            Assert.AreEqual("Settings menu, copied from the library, opening over Pause", lines[3]);
            Assert.AreEqual("Game Over: only the state, you make the menu", lines[4]);
        }

        [Test]
        public void ToDoItems_ListEveryOwnMenuAndNothingElse()
        {
            OnboardingChoices choices = new OnboardingChoices();
            choices.Get(OnboardingMenuKind.GameOver).UseOwnMenu = true;
            choices.UseOwnSettingsMenu = true;
            List<OnboardingToDo> items = OnboardingPlan.ToDoItems(choices);
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual("Game Over", items[0].MenuName);
            Assert.AreEqual("Settings", items[1].MenuName);
        }

        [Test]
        public void ToDoItems_IsEmptyWhenEveryMenuIsCopied()
        {
            Assert.AreEqual(0, OnboardingPlan.ToDoItems(new OnboardingChoices()).Count);
        }
    }
}
