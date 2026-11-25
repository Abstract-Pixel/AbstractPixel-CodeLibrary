using System.Collections;
using System.Collections.Generic;
using AbstractPixel.SceneTransitions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AbstractPixel.SceneManagement.Samples.Tests
{
    /// <summary>Sample 01: the Room A button opens Room B, with the fade going Hiding, Hidden, Showing, Visible.</summary>
    public class Sample01ButtonChangesLevelTests
    {
        private const string Groups = "Packages/com.abstractpixel.library/Modules/SceneManagement/Samples/01 Button Changes Level/Scene Groups/";
        private const float TimeoutSeconds = 30f;

        private readonly List<ScreenState> states = new List<ScreenState>();

        [UnityTest]
        public IEnumerator TheRoomAButtonOpensRoomBThroughTheFade()
        {
            SceneGroup roomA = AssetDatabase.LoadAssetAtPath<SceneGroup>(Groups + "Sample 01 Room AGroup.asset");
            SceneGroup roomB = AssetDatabase.LoadAssetAtPath<SceneGroup>(Groups + "Sample 01 Room BGroup.asset");
            SampleStepResult step = new SampleStepResult();
            yield return SampleTestSteps.OpenGroup(roomA, step);
            Assert.IsNotNull(step.Result, step.Problem);
            Assert.IsTrue(step.Result.IsDone, step.Result.ToString());

            TransitionEventBus.OnScreenStateChanged += RecordState;
            try
            {
                Button button = GameObject.Find("Change Room Button").GetComponent<Button>();
                button.onClick.Invoke();

                float startTime = Time.realtimeSinceStartup;
                while (IsDone(roomB) == false && Time.realtimeSinceStartup - startTime < TimeoutSeconds)
                {
                    yield return null;
                }
            }
            finally
            {
                TransitionEventBus.OnScreenStateChanged -= RecordState;
            }

            Assert.AreSame(roomB, SceneActions.ActiveSceneGroup, "Room B is the active scene group");
            ScreenState[] expected = { ScreenState.Hiding, ScreenState.Hidden, ScreenState.Showing, ScreenState.Visible };
            CollectionAssert.AreEqual(expected, states, "screen states in order");
        }

        private bool IsDone(SceneGroup _target)
        {
            bool arrived = SceneActions.ActiveSceneGroup == _target;
            bool visibleAgain = states.Count > 0 && states[states.Count - 1] == ScreenState.Visible;
            return arrived && visibleAgain;
        }

        private void RecordState(ScreenState _state)
        {
            states.Add(_state);
        }
    }
}
