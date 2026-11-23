using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbstractPixel.Core;
using AbstractPixel.SceneTransitions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SceneManagement.Samples.Tests
{
    /// <summary>
    /// Sample 02: one change sends its messages in the order the module README gives; twice fast gives Done then
    /// Refused Busy; the open group gives Refused AlreadyOpen.
    /// </summary>
    public class Sample02ChangeLevelFromCodeTests
    {
        private const string Groups = "Packages/com.abstractpixel.library/Modules/SceneManagment/Samples/02 Change Level From Code/Scene Groups/";

        // The module README, "Order, per entry point", A change: Hide screen, close (OnSceneGroupUnloaded), open the
        // main scene (OnMainSceneLoaded), OnNewSceneGroupTransitionTo, then Show screen.
        private static readonly string[] ChangeOrder =
        {
            "Screen Hiding", "Screen Hidden", "OnSceneGroupUnloaded", "OnMainSceneLoaded", "OnNewSceneGroupTransitionTo", "Screen Showing", "Screen Visible"
        };

        private readonly List<string> messages = new List<string>();

        [UnityTest]
        public IEnumerator ResultsAndMessageOrder()
        {
            SceneGroup hub = AssetDatabase.LoadAssetAtPath<SceneGroup>(Groups + "Sample 02 HubGroup.asset");
            SampleStepResult step = new SampleStepResult();
            yield return SampleTestSteps.OpenGroup(hub, step);
            Assert.IsNotNull(step.Result, step.Problem);
            Assert.IsTrue(step.Result.IsDone, step.Result.ToString());

            Listen();
            Task<SceneChangeResult> change = Changer().ChangeToOther();
            yield return WaitFor(change);
            Unlisten();
            Assert.IsTrue(change.Result.IsDone, change.Result.ToString());
            CollectionAssert.AreEqual(ChangeOrder, messages, "messages: " + string.Join(", ", messages));

            Task<SceneChangeResult[]> twice = Changer().ChangeTwiceFast();
            yield return WaitFor(twice);
            Assert.AreEqual(SceneChangeOutcome.Done, twice.Result[0].Outcome, twice.Result[0].ToString());
            Assert.AreEqual(SceneChangeOutcome.Refused, twice.Result[1].Outcome, twice.Result[1].ToString());
            Assert.AreEqual(SceneChangeRefusal.Busy, twice.Result[1].Refusal);

            Task<SceneChangeResult> open = Changer().ChangeToTheOpenGroup();
            yield return WaitFor(open);
            Assert.AreEqual(SceneChangeOutcome.Refused, open.Result.Outcome, open.Result.ToString());
            Assert.AreEqual(SceneChangeRefusal.AlreadyOpen, open.Result.Refusal);
        }

        private static Sample02LevelChanger Changer()
        {
            Sample02LevelChanger changer = Object.FindAnyObjectByType<Sample02LevelChanger>();
            Assert.IsNotNull(changer, "a Sample02LevelChanger in the open main scene");
            return changer;
        }

        private static IEnumerator WaitFor(Task _task)
        {
            float startTime = Time.realtimeSinceStartup;
            while (_task.IsCompleted == false && Time.realtimeSinceStartup - startTime < SampleTestSteps.DefaultTimeoutSeconds)
            {
                yield return null;
            }
            Assert.IsTrue(_task.IsCompleted, "the change did not end in time");
            Assert.IsFalse(_task.IsFaulted, "the change threw: " + _task.Exception);
        }

        private void Listen()
        {
            messages.Clear();
            SceneEventBus.OnSceneGroupUnloaded += OnUnloaded;
            SceneEventBus.OnMainSceneLoaded += OnMainSceneLoaded;
            SceneEventBus.OnNewSceneGroupTransitionTo += OnNewGroup;
            TransitionEventBus.OnScreenStateChanged += OnScreenState;
        }

        private void Unlisten()
        {
            SceneEventBus.OnSceneGroupUnloaded -= OnUnloaded;
            SceneEventBus.OnMainSceneLoaded -= OnMainSceneLoaded;
            SceneEventBus.OnNewSceneGroupTransitionTo -= OnNewGroup;
            TransitionEventBus.OnScreenStateChanged -= OnScreenState;
        }

        private void OnUnloaded(SceneGroup _group)
        {
            messages.Add("OnSceneGroupUnloaded");
        }

        private void OnMainSceneLoaded(SceneReference _scene)
        {
            messages.Add("OnMainSceneLoaded");
        }

        private void OnNewGroup(SceneGroup _group)
        {
            messages.Add("OnNewSceneGroupTransitionTo");
        }

        private void OnScreenState(ScreenState _state)
        {
            messages.Add("Screen " + _state);
        }
    }
}
