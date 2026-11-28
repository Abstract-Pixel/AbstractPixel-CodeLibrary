using System.Linq;
using AbstractPixel.GameManagement.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AbstractPixel.GameManagement.Tests
{
    /// <summary>Presses each button of the copied menus against a real state machine built from the set-up states, so a button
    /// the machine would refuse (a lower state while a higher one is on top) is caught without starting Play mode.</summary>
    public class OnboardingMenuButtonTests
    {
        private const string TestFolder = "Assets/ZZ_OnboardingButtonTest";

        private Object[] preloadedBefore;
        private GameManagementSettings settings;
        private GameStateMachine machine;
        private readonly System.Collections.Generic.List<GameObject> instances = new System.Collections.Generic.List<GameObject>();

        [SetUp]
        public void SetUp()
        {
            preloadedBefore = PlayerSettings.GetPreloadedAssets();
            AssetDatabase.DeleteAsset(TestFolder);
            OnboardingChoices choices = new OnboardingChoices();
            choices.SettingsFolder = TestFolder;
            choices.Get(OnboardingMenuKind.GameWin).Ticked = true;
            settings = OnboardingBuilder.Run(choices).Settings;
            machine = new GameStateMachine(settings, new FakeGameClock(), new RecordingEffectSink());
            GameStates.Use(machine);
        }

        [TearDown]
        public void TearDown()
        {
            GameStates.Use(null);
            foreach (GameObject instance in instances)
            {
                Object.DestroyImmediate(instance);
            }
            PlayerSettings.SetPreloadedAssets(preloadedBefore);
            AssetDatabase.DeleteAsset(TestFolder);
            AssetDatabase.Refresh();
        }

        private StateSO State(string _name)
        {
            return settings.Catalog.States.First(_state => _state.name == _name);
        }

        private void Request(string _name)
        {
            machine.Request(State(_name), "test game code");
            machine.SkipDelay();
        }

        private void Press(string _prefab, string _button)
        {
            GameObject instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(TestFolder + "/Menus/" + _prefab));
            instances.Add(instance);
            Button button = instance.GetComponentsInChildren<Button>(true).FirstOrDefault(_candidate => _candidate.name == _button);
            Assert.IsNotNull(button, "'" + _prefab + "' has no '" + _button + "'.");
            RunInEditMode(button);
            button.onClick.Invoke();
        }

        // A button's calls run only in Play mode by default; this test presses them in Edit mode, so let them run in both.
        private static void RunInEditMode(Button _button)
        {
            SerializedObject data = new SerializedObject(_button);
            SerializedProperty calls = data.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
            for (int index = 0; index < calls.arraySize; index++)
            {
                calls.GetArrayElementAtIndex(index).FindPropertyRelative("m_CallState").intValue = (int)UnityEngine.Events.UnityEventCallState.EditorAndRuntime;
            }
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private void AssertTop(string _name, string _step)
        {
            Assert.AreEqual(State(_name), machine.Top, "After '" + _step + "' the top state should be " + _name + " but is " + (machine.Top == null ? "none" : machine.Top.name));
        }

        [TestCase("Game Over Menu .prefab", "Game Over")]
        [TestCase("Game Win Menu.prefab", "Game Win")]
        public void Restart_LeavesTheEndStateAndEntersPlay(string _prefab, string _state)
        {
            Request("Play");
            Request(_state);
            Press(_prefab, "Restart Button");
            AssertTop("Play", _state + " > Restart");
        }

        [TestCase("Game Over Menu .prefab", "Game Over")]
        [TestCase("Game Win Menu.prefab", "Game Win")]
        public void MainMenuButton_LeavesTheEndStateAndEntersMainMenu(string _prefab, string _state)
        {
            Request("Play");
            Request(_state);
            Press(_prefab, "Main Menu Button");
            AssertTop("Main Menu", _state + " > Main Menu");
        }

        [Test]
        public void PauseMenu_ResumeSettingsAndMainMenuAllWork()
        {
            Request("Play");
            Request("Pause");
            Press("Pause Menu.prefab", "Resume Button");
            AssertTop("Play", "Pause > Resume");

            Request("Pause");
            Press("Pause Menu.prefab", "Settings Button");
            AssertTop("Settings", "Pause > Settings");

            machine.Leave(State("Settings"), "back");
            AssertTop("Pause", "Settings > Back");
            Press("Pause Menu.prefab", "Main Menu Button");
            AssertTop("Main Menu", "Pause > Main Menu");
        }

        [Test]
        public void StartMenu_StartEntersPlay()
        {
            Request("Main Menu");
            Press("StartMenu.prefab", "Start Game Button");
            AssertTop("Play", "Main Menu > Start");
        }

        [Test]
        public void SettingsMenu_BackLeavesSettings()
        {
            Request("Play");
            Request("Pause");
            Request("Settings");
            Press("Settings Menu.prefab", "Back Button");
            AssertTop("Pause", "Settings > Back");
        }
    }
}
