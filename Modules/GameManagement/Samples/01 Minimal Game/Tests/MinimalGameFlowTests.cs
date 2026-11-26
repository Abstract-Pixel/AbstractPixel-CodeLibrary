using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AbstractPixel.GameManagement.Samples.Tests
{
    /// <summary>
    /// Sample 01: walks every flow by pressing the sample's buttons, records what the player sees at each step and
    /// compares it with Parity/baseline.json. The first run (no baseline yet) writes the baseline instead.
    /// </summary>
    public class MinimalGameFlowTests
    {
        private const string SampleFolder = "Packages/com.abstractpixel.library/Modules/GameManagement/Samples/01 Minimal Game";
        private const string ScenePath = SampleFolder + "/Scenes/Minimal Game.unity";
        private const float SettleSeconds = 1.2f;

        private Keyboard keyboard;
        private Gamepad gamepad;
        private readonly List<MinimalGameFlowStep> steps = new List<MinimalGameFlowStep>();

        [UnityTest]
        public IEnumerator EveryFlowMatchesTheBaseline()
        {
            // The host project's own boot objects (Rally's save system, for example) can log while the sample scene
            // replaces theirs. Those logs are not the sample's; the recorded steps are what this test checks.
            LogAssert.ignoreFailingMessages = true;
            EditorSceneManager.LoadSceneInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return Record("Boot");
            yield return Press("Main Menu", "Settings Button", "Main Menu > Settings");
            Assert.IsFalse(IsMenuShown("Main Menu"), "Settings over Main Menu hides the Main Menu");
            yield return Press("Settings", "Back Button", "Settings > Back");
            yield return Press("Main Menu", "Play Button", "Main Menu > Play");
            Assert.Greater(Object.FindAnyObjectByType<MinimalGameCounter>().Seconds, 0f, "the counter runs in Play");
            yield return Press("HUD", "Pause Button", "Play > Pause");
            yield return Press("Pause", "Settings Button", "Pause > Settings");
            Assert.IsFalse(IsMenuShown("Pause"), "Settings over Pause hides the Pause menu");
            Assert.IsTrue(IsMenuShown("Settings"), "Settings is shown");
            yield return Press("Settings", "Back Button", "Pause Settings > Back");
            Assert.IsTrue(IsMenuShown("Pause"), "closing Settings shows the Pause menu again");
            yield return Press("Pause", "Resume Button", "Pause > Resume");

            // As a player would: the keyboard, not the buttons.
            yield return PressKey(Key.Escape, "Play > Esc");
            Assert.IsTrue(IsMenuShown("Pause"), "Esc in Play opens Pause");
            yield return Press("Pause", "Settings Button", "Esc Pause > Settings");
            yield return PressKey(Key.Backspace, "Settings > Backspace");
            Assert.IsTrue(IsMenuShown("Pause"), "Backspace closes Settings back to Pause");
            yield return PressKey(Key.Escape, "Pause > Esc");
            Assert.IsTrue(IsMenuShown("HUD"), "Esc in Pause resumes Play");

            // A controller: the left small button (Share, Select) pauses, the East button (Circle, B) goes back one step.
            yield return PressButton(GamepadButton.Select, "Play > Gamepad Select");
            Assert.IsTrue(IsMenuShown("Pause"), "Select in Play opens Pause");
            yield return Press("Pause", "Settings Button", "Gamepad Pause > Settings");
            yield return PressButton(GamepadButton.East, "Settings > Gamepad East");
            Assert.IsTrue(IsMenuShown("Pause"), "East in Settings goes back to Pause, not further");
            yield return PressButton(GamepadButton.East, "Pause > Gamepad East");
            Assert.IsTrue(IsMenuShown("HUD"), "East in Pause resumes Play");
            yield return Press("HUD", "Lose Button", "Play > Game Over");

            StateSO gameOver = GameStates.Top;
            yield return Press("Game Over", "Retry Button", "Game Over > Retry");
            Assert.IsFalse(GameStates.IsActive(gameOver), "the scene change left Game Over, which is marked to leave");

            yield return Press("HUD", "Win Button", "Play > Game Win");
            yield return Press("Game Win", "Quit Button", "Game Win > Quit");

            CompareWithBaseline();
        }

        private IEnumerator Press(string _menuName, string _buttonName, string _stepName)
        {
            Button button = FindShownButton(_menuName, _buttonName);
            Assert.IsNotNull(button, $"'{_menuName}/{_buttonName}' is not shown before step '{_stepName}'. Journal:\n{DescribeJournal()}");
            button.onClick.Invoke();
            yield return Record(_stepName);
        }

        private IEnumerator PressKey(Key _key, string _stepName)
        {
            if (keyboard == null)
            {
                keyboard = InputSystem.AddDevice<Keyboard>();
            }
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(_key));
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return Record(_stepName);
        }

        private IEnumerator PressButton(GamepadButton _button, string _stepName)
        {
            if (gamepad == null)
            {
                gamepad = InputSystem.AddDevice<Gamepad>();
            }
            InputSystem.QueueStateEvent(gamepad, new GamepadState(_button));
            yield return null;
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            yield return Record(_stepName);
        }

        [TearDown]
        public void RemoveTestDevices()
        {
            if (gamepad != null)
            {
                InputSystem.RemoveDevice(gamepad);
                gamepad = null;
            }
            if (keyboard != null)
            {
                InputSystem.RemoveDevice(keyboard);
                keyboard = null;
            }
        }

        private IEnumerator Record(string _stepName)
        {
            float end = Time.realtimeSinceStartup + SettleSeconds;
            while (Time.realtimeSinceStartup < end)
            {
                yield return null;
            }
            steps.Add(MinimalGameFlowStep.Read(_stepName));
        }

        private static bool IsMenuShown(string _menuName)
        {
            GameObject menu = GameObject.Find(_menuName);
            return menu != null && menu.activeInHierarchy;
        }

        private static string DescribeJournal()
        {
            if (!GameStates.HasMachine)
            {
                return "no machine";
            }
            List<string> lines = new List<string>();
            foreach (TransitionRecord record in GameStates.Machine.Journal.Records)
            {
                string stateName = record.State != null ? record.State.name : "none";
                lines.Add($"{record.Kind} {stateName} by {record.Requester}: {record.Reason}");
            }
            return string.Join("\n", lines);
        }

        private static Button FindShownButton(string _menuName, string _buttonName)
        {
            foreach (Button button in Object.FindObjectsByType<Button>(FindObjectsSortMode.None))
            {
                Transform parent = button.transform.parent;
                bool isInMenu = parent != null && parent.name == _menuName;
                if (button.name == _buttonName && isInMenu && button.gameObject.activeInHierarchy)
                {
                    return button;
                }
            }
            return null;
        }

        private void CompareWithBaseline()
        {
            string baselinePath = Path.GetFullPath(SampleFolder + "/Parity/baseline.json");
            string lastRunPath = Path.GetFullPath(SampleFolder + "/Parity/last-run.json");
            MinimalGameFlowRecording recording = new MinimalGameFlowRecording { UnityVersion = Application.unityVersion, Steps = steps };
            string json = JsonUtility.ToJson(recording, true);
            File.WriteAllText(lastRunPath, json);
            if (!File.Exists(baselinePath))
            {
                File.WriteAllText(baselinePath, json);
                Debug.Log($"[Sample 01] No baseline yet, so this run wrote one: {baselinePath}");
                return;
            }
            MinimalGameFlowRecording baseline = JsonUtility.FromJson<MinimalGameFlowRecording>(File.ReadAllText(baselinePath));
            Assert.AreEqual(baseline.Steps.Count, steps.Count, "step count");
            List<string> differences = new List<string>();
            for (int i = 0; i < steps.Count; i++)
            {
                string difference = steps[i].DescribeDifference(baseline.Steps[i]);
                if (difference != null)
                {
                    differences.Add(difference);
                }
            }
            Assert.IsEmpty(differences, "Differences from the baseline:\n" + string.Join("\n", differences));
        }
    }
}
