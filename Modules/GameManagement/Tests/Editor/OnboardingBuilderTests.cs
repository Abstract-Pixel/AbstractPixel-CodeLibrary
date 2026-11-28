using System.Collections.Generic;
using System.IO;
using System.Linq;
using AbstractPixel.GameManagement.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AbstractPixel.GameManagement.Tests
{
    /// <summary>Runs the real setup into a temporary folder. Each test removes what it made and puts Preloaded Assets back.</summary>
    public class OnboardingBuilderTests
    {
        private const string TestFolder = "Assets/ZZ_OnboardingTest";

        private Object[] preloadedBefore;

        [SetUp]
        public void SetUp()
        {
            preloadedBefore = PlayerSettings.GetPreloadedAssets();
            AssetDatabase.DeleteAsset(TestFolder);
        }

        [TearDown]
        public void TearDown()
        {
            PlayerSettings.SetPreloadedAssets(preloadedBefore);
            AssetDatabase.DeleteAsset(TestFolder);
            AssetDatabase.Refresh();
        }

        private static OnboardingChoices ChoicesIn(string _subFolder)
        {
            OnboardingChoices choices = new OnboardingChoices();
            choices.SettingsFolder = TestFolder + "/" + _subFolder;
            return choices;
        }

        private static List<string> Names(GameManagementSettings _settings)
        {
            return _settings.Catalog.States.Select(_state => _state.name).ToList();
        }

        private static StateSO State(GameManagementSettings _settings, string _name)
        {
            return _settings.Catalog.States.First(_state => _state.name == _name);
        }

        private static string ReactorState(GameObject _prefabRoot)
        {
            StateMenuReactor reactor = _prefabRoot.GetComponent<StateMenuReactor>();
            Assert.IsNotNull(reactor, _prefabRoot.name + " has no State Menu Reactor on its root.");
            SerializedProperty state = new SerializedObject(reactor).FindProperty("state");
            return state.objectReferenceValue == null ? null : state.objectReferenceValue.name;
        }

        [Test]
        public void CopyEverything_MakesStatesInOrderWithNoProblems()
        {
            OnboardingChoices choices = ChoicesIn("All");
            choices.Get(OnboardingMenuKind.GameWin).Ticked = true;
            choices.Get(OnboardingMenuKind.MainMenu).IncludeSettings = true;

            OnboardingResult result = OnboardingBuilder.Run(choices);

            CollectionAssert.AreEqual(new[] { "Game Win", "Game Over", "Settings", "Pause", "Play", "Main Menu" }, Names(result.Settings));
            Assert.AreEqual("Assets/ZZ_OnboardingTest/All/GameManagementSettings.asset", AssetDatabase.GetAssetPath(result.Settings));
            Assert.AreEqual(0, StateListValidator.Find(result.Settings.Catalog).Count, "The new state list has problems.");
            Assert.AreEqual(result.Settings, GameManagementSettingsAssets.Find());
            Assert.IsTrue(GameManagementSettingsAssets.IsPreloaded(result.Settings));
            Assert.AreEqual(0, result.ToDo.Count);
            Assert.AreEqual(0, result.Renamed.Count);
        }

        [Test]
        public void Pause_OpensOverPlay_AndSettingsOverTheMenusThatAskedForIt()
        {
            OnboardingChoices choices = ChoicesIn("Parents");
            choices.Get(OnboardingMenuKind.MainMenu).IncludeSettings = true;

            GameManagementSettings settings = OnboardingBuilder.Run(choices).Settings;

            CollectionAssert.AreEqual(new[] { State(settings, "Play") }, State(settings, "Pause").Parents);
            CollectionAssert.AreEqual(new[] { State(settings, "Main Menu"), State(settings, "Pause") }, State(settings, "Settings").Parents);
            Assert.AreEqual(0, State(settings, "Play").Parents.Count);
        }

        [Test]
        public void States_StartWithTheDefaultsOfTheirMenu()
        {
            GameManagementSettings settings = OnboardingBuilder.Run(ChoicesIn("Defaults")).Settings;

            StateEffectSettings pause = State(settings, "Pause").Effects;
            Assert.AreEqual(StateEffectSource.Own, pause.TimeScaleSource);
            Assert.AreEqual(0f, pause.TimeScale);
            Assert.AreEqual(GameCursorMode.Free, pause.Cursor);
            StateEffectSettings settingsState = State(settings, "Settings").Effects;
            Assert.AreEqual(StateEffectSource.SameAsParent, settingsState.TimeScaleSource);
            Assert.AreEqual(StateEffectSource.SameAsParent, settingsState.CursorSource);
            Assert.AreEqual(StateEffectSource.SameAsParent, settingsState.HudSource);
            Assert.AreEqual(1f, State(settings, "Play").Effects.TimeScale);
            Assert.AreEqual(GameCursorMode.LockedHidden, State(settings, "Play").Effects.Cursor);
            Assert.IsTrue(State(settings, "Play").Effects.HudVisible);
        }

        [Test]
        public void CopiedMenus_KeepTheLibraryNamesAndPointAtTheirState()
        {
            OnboardingChoices choices = ChoicesIn("Copies");
            choices.Get(OnboardingMenuKind.GameWin).Ticked = true;

            OnboardingBuilder.Run(choices);

            string menus = TestFolder + "/Copies/Menus/";
            string[] expected = { "StartMenu.prefab", "Pause Menu.prefab", "Game Over Menu .prefab", "Game Win Menu.prefab", "Settings Menu.prefab" };
            string[] states = { "Main Menu", "Pause", "Game Over", "Game Win", "Settings" };
            for (int index = 0; index < expected.Length; index++)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(menus + expected[index]);
                Assert.IsNotNull(prefab, expected[index] + " was not copied.");
                Assert.AreEqual(states[index], ReactorState(prefab));
                Assert.IsNotNull(prefab.transform.Find("Content").GetComponent<AbstractPixel.Core.UI.AutoSpatialNavigation>(), expected[index] + " has no gamepad navigation on its Content.");
                Assert.IsNotNull(prefab.transform.Find("Content").GetComponent<AbstractPixel.Core.UI.SelectOnKeyboardNavigation>(), expected[index] + " has nothing that lets a keyboard pick the first button.");
                Assert.IsTrue(prefab.activeSelf, expected[index] + " is switched off at its root, so its State Menu Reactor would never run.");
            }
        }

        [Test]
        public void CopiedMenus_HaveEveryButtonDoingSomething()
        {
            OnboardingBuilder.Run(ChoicesIn("Buttons"));

            foreach (string path in Directory.GetFiles(TestFolder + "/Buttons/Menus", "*.prefab"))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path.Replace('\\', '/'));
                Button[] buttons = prefab.GetComponentsInChildren<Button>(true);
                Assert.Greater(buttons.Length, 0, path + " has no buttons left.");
                foreach (Button button in buttons)
                {
                    Assert.Greater(button.onClick.GetPersistentEventCount(), 0, button.name + " in " + path + " does nothing when clicked.");
                }
            }
        }

        [Test]
        public void CopiedMenus_DropTheButtonsThatNeedAStateThatIsNotThere()
        {
            OnboardingChoices choices = ChoicesIn("NoMain");
            choices.Get(OnboardingMenuKind.MainMenu).Ticked = false;

            OnboardingBuilder.Run(choices);

            GameObject pause = AssetDatabase.LoadAssetAtPath<GameObject>(TestFolder + "/NoMain/Menus/Pause Menu.prefab");
            Assert.IsNull(pause.GetComponentsInChildren<Button>(true).FirstOrDefault(_button => _button.name == "Main Menu Button"));
            Assert.IsNotNull(pause.GetComponentsInChildren<Button>(true).FirstOrDefault(_button => _button.name == "Resume Button"));
        }

        [Test]
        public void UseMyOwn_WithoutAPrefab_MakesOnlyTheStateAndListsWhatIsLeft()
        {
            OnboardingChoices choices = ChoicesIn("OwnNone");
            choices.Get(OnboardingMenuKind.GameOver).UseOwnMenu = true;

            OnboardingResult result = OnboardingBuilder.Run(choices);

            Assert.IsFalse(File.Exists(TestFolder + "/OwnNone/Menus/Game Over Menu .prefab"));
            Assert.IsNotNull(State(result.Settings, "Game Over"));
            Assert.AreEqual(1, result.ToDo.Count);
            Assert.AreEqual("Game Over", result.ToDo[0].MenuName);
        }

        [Test]
        public void UseMyOwn_WithAnInactiveRoot_SaysTheReactorCannotRun()
        {
            GameObject own = new GameObject("My Inactive Menu");
            own.SetActive(false);
            AssetDatabase.CreateFolder("Assets", "ZZ_OnboardingTest");
            GameObject ownPrefab = PrefabUtility.SaveAsPrefabAsset(own, TestFolder + "/My Inactive Menu.prefab");
            Object.DestroyImmediate(own);
            OnboardingChoices choices = ChoicesIn("OwnInactive");
            choices.Get(OnboardingMenuKind.GameOver).UseOwnMenu = true;
            choices.Get(OnboardingMenuKind.GameOver).OwnPrefab = ownPrefab;

            OnboardingResult result = OnboardingBuilder.Run(choices);

            Assert.AreEqual(1, result.Skipped.Count);
            StringAssert.Contains("switched off", result.Skipped[0]);
        }

        [Test]
        public void UseMyOwn_WithAPrefabThatAlreadyHasAReactor_LeavesItAsItIs()
        {
            GameObject own = new GameObject("My Menu With Reactor");
            new GameObject("Panel").transform.SetParent(own.transform);
            own.AddComponent<StateMenuReactor>();
            AssetDatabase.CreateFolder("Assets", "ZZ_OnboardingTest");
            string ownPath = TestFolder + "/My Menu With Reactor.prefab";
            GameObject ownPrefab = PrefabUtility.SaveAsPrefabAsset(own, ownPath);
            Object.DestroyImmediate(own);
            OnboardingChoices choices = ChoicesIn("OwnReactor");
            choices.Get(OnboardingMenuKind.GameOver).UseOwnMenu = true;
            choices.Get(OnboardingMenuKind.GameOver).OwnPrefab = ownPrefab;

            OnboardingResult result = OnboardingBuilder.Run(choices);

            Assert.IsNull(ReactorState(AssetDatabase.LoadAssetAtPath<GameObject>(ownPath)), "The reactor the developer set up was changed.");
            Assert.AreEqual(1, result.Skipped.Count);
            StringAssert.Contains("already has a State Menu Reactor", result.Skipped[0]);
        }

        [Test]
        public void Clashes_WarnBeforehandAboutAnOwnPrefabThatAlreadyHasAReactor()
        {
            GameObject own = new GameObject("My Menu With Reactor");
            own.AddComponent<StateMenuReactor>();
            AssetDatabase.CreateFolder("Assets", "ZZ_OnboardingTest");
            GameObject ownPrefab = PrefabUtility.SaveAsPrefabAsset(own, TestFolder + "/My Menu With Reactor.prefab");
            Object.DestroyImmediate(own);
            OnboardingChoices choices = ChoicesIn("Warn");
            choices.Get(OnboardingMenuKind.GameOver).UseOwnMenu = true;
            choices.Get(OnboardingMenuKind.GameOver).OwnPrefab = ownPrefab;

            System.Collections.Generic.List<string> notes = OnboardingClashes.Find(choices, TestFolder + "/Warn", false);

            Assert.AreEqual(1, notes.Count);
            StringAssert.Contains("already has a State Menu Reactor", notes[0]);
        }

        [Test]
        public void UseMyOwn_WithAPrefab_AddsTheReactorToItsRoot()
        {
            GameObject own = new GameObject("My Game Over");
            new GameObject("Panel").transform.SetParent(own.transform);
            AssetDatabase.CreateFolder("Assets", "ZZ_OnboardingTest");
            string ownPath = TestFolder + "/My Game Over.prefab";
            GameObject ownPrefab = PrefabUtility.SaveAsPrefabAsset(own, ownPath);
            Object.DestroyImmediate(own);
            OnboardingChoices choices = ChoicesIn("OwnPrefab");
            choices.Get(OnboardingMenuKind.GameOver).UseOwnMenu = true;
            choices.Get(OnboardingMenuKind.GameOver).OwnPrefab = ownPrefab;

            OnboardingBuilder.Run(choices);

            GameObject saved = AssetDatabase.LoadAssetAtPath<GameObject>(ownPath);
            Assert.AreEqual("Game Over", ReactorState(saved));
            SerializedProperty content = new SerializedObject(saved.GetComponent<StateMenuReactor>()).FindProperty("content");
            Assert.AreEqual("Panel", content.objectReferenceValue.name);
        }

        [Test]
        public void AnExistingFile_IsNeverWrittenOver_TheNewOneGetsAUniqueName()
        {
            AssetDatabase.CreateFolder("Assets", "ZZ_OnboardingTest");
            AssetDatabase.CreateFolder(TestFolder, "Clash");
            AssetDatabase.CreateFolder(TestFolder + "/Clash", "States");
            StateSO existing = ScriptableObject.CreateInstance<StateSO>();
            AssetDatabase.CreateAsset(existing, TestFolder + "/Clash/States/Pause.asset");
            byte[] before = File.ReadAllBytes(TestFolder + "/Clash/States/Pause.asset");

            OnboardingResult result = OnboardingBuilder.Run(ChoicesIn("Clash"));

            CollectionAssert.AreEqual(before, File.ReadAllBytes(TestFolder + "/Clash/States/Pause.asset"));
            Assert.IsTrue(File.Exists(TestFolder + "/Clash/States/Pause 1.asset"));
            Assert.AreEqual(1, result.Renamed.Count);
            StringAssert.Contains("Pause", result.Renamed[0]);
            Assert.AreEqual("Pause 1", State(result.Settings, "Pause 1").name);
        }

        [Test]
        public void RunningAgain_AddsOnlyTheSkippedMenu()
        {
            OnboardingChoices first = ChoicesIn("Again");
            first.Get(OnboardingMenuKind.Pause).Ticked = false;
            first.Get(OnboardingMenuKind.GameOver).Ticked = false;
            GameManagementSettings settings = OnboardingBuilder.Run(first).Settings;
            int statesBefore = settings.Catalog.States.Count;

            OnboardingChoices second = OnboardingChoices.ForExistingSettings(settings);
            Assert.IsTrue(second.Get(OnboardingMenuKind.MainMenu).Locked);
            Assert.IsTrue(second.Get(OnboardingMenuKind.MainMenu).Ticked);
            Assert.IsFalse(second.Get(OnboardingMenuKind.GameOver).Ticked);
            second.Get(OnboardingMenuKind.GameOver).Ticked = true;
            OnboardingBuilder.Run(second);

            Assert.AreEqual(statesBefore + 1, settings.Catalog.States.Count);
            CollectionAssert.AreEqual(new[] { "Game Over", "Play", "Main Menu" }, Names(settings));
            Assert.AreEqual(0, StateListValidator.Find(settings.Catalog).Count);
        }

        [Test]
        public void RunningAgain_AddsANewMenuAsAParentOfTheExistingSettingsState()
        {
            OnboardingChoices first = ChoicesIn("Parent");
            first.Get(OnboardingMenuKind.MainMenu).Ticked = false;
            GameManagementSettings settings = OnboardingBuilder.Run(first).Settings;

            OnboardingChoices second = OnboardingChoices.ForExistingSettings(settings);
            Assert.IsTrue(second.SettingsStateExists);
            second.Get(OnboardingMenuKind.MainMenu).Ticked = true;
            second.Get(OnboardingMenuKind.MainMenu).IncludeSettings = true;
            OnboardingBuilder.Run(second);

            CollectionAssert.AreEqual(new[] { State(settings, "Pause"), State(settings, "Main Menu") }, State(settings, "Settings").Parents);
            Assert.AreEqual(1, settings.Catalog.States.Count(_state => _state.name == "Settings"));
        }

        [Test]
        public void AFailedRun_RemovesWhatItMadeAndPutsPreloadedAssetsBack()
        {
            AssetDatabase.CreateFolder("Assets", "ZZ_OnboardingTest");
            File.WriteAllText(TestFolder + "/Blocked.txt", "a file where a folder is needed");
            AssetDatabase.Refresh();
            OnboardingChoices choices = ChoicesIn("Blocked.txt/Inside");

            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Failed to create folder"));
            System.Exception error = Assert.Catch<System.Exception>(() => OnboardingBuilder.Run(choices));

            StringAssert.Contains("Pick another settings folder", error.Message);

            Assert.AreEqual(0, AssetDatabase.FindAssets("t:GameStateCatalog", new[] { TestFolder }).Length, "A state list was left behind.");
            Assert.AreEqual(0, AssetDatabase.FindAssets("t:StateSO", new[] { TestFolder }).Length, "A state was left behind.");
            CollectionAssert.AreEqual(preloadedBefore, PlayerSettings.GetPreloadedAssets());
        }

        [Test]
        public void Input_IsStoredOnTheSettings()
        {
            OnboardingChoices choices = ChoicesIn("Input");
            choices.InputSource = InputSourceKind.ProjectWideActions;

            Assert.AreEqual(InputSourceKind.ProjectWideActions, OnboardingBuilder.Run(choices).Settings.InputSource);
        }

        [Test]
        public void Find_PrefersTheProjectsSettingsOverAPackageSample()
        {
            GameManagementSettings made = OnboardingBuilder.Run(ChoicesIn("Find")).Settings;
            PlayerSettings.SetPreloadedAssets(new Object[0]);

            Assert.AreEqual(made, GameManagementSettingsAssets.Find());
            Assert.AreEqual(made, GameManagementSettingsAssets.FindProjectSettings());
        }

        [TestCase("Assets/A/B.asset", true)]
        [TestCase("Assets", true)]
        [TestCase("Packages/com.abstractpixel.library/Modules/X.asset", false)]
        [TestCase("", false)]
        public void ProjectPath_IsUnderAssets(string _path, bool _expected)
        {
            Assert.AreEqual(_expected, GameManagementSettingsAssets.IsProjectPath(_path));
        }
    }
}
