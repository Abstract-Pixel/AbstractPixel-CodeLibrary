using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace AbstractPixel.SceneTransitions.Tests
{
    /// <summary>Session 4C-1, decision 4: the manager reports Visible, Hiding, Hidden, Showing, Visible and never touches game time.</summary>
    public class ScreenStateTests
    {
        private readonly List<Object> createdObjects = new List<Object>();
        private readonly List<ScreenState> seenStates = new List<ScreenState>();
        private TransitionManager manager;

        [SetUp]
        public void CreateManagerWithAQuickEffect()
        {
            TransitionActions.ResetStatics();
            TransitionEventBus.ResetStatics();
            seenStates.Clear();
            TransitionEventBus.OnScreenStateChanged += seenStates.Add;

            // A fade with no delay and a one-frame duration: quick, but still a real effect with its own awaits.
            GameObject controllerTemplate = new GameObject("Effect template");
            controllerTemplate.AddComponent<FadeTransitionController>();
            createdObjects.Add(controllerTemplate);
            ImageFadeProfile profile = ScriptableObject.CreateInstance<ImageFadeProfile>();
            profile.StartDelay = 0f;
            profile.Duration = 0.02f;
            profile.TransitionControllerPrefab = controllerTemplate;
            createdObjects.Add(profile);

            GameObject managerObject = new GameObject("TransitionManager under test");
            createdObjects.Add(managerObject);
            manager = managerObject.AddComponent<TransitionManager>();
            manager.SetTransitionProfile(profile);
        }

        [TearDown]
        public void DestroyCreatedObjects()
        {
            manager.SetTransitionProfile(null);
            foreach (FadeTransitionController clone in Object.FindObjectsByType<FadeTransitionController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(clone.gameObject);
            }
            TransitionEventBus.ResetStatics();
            TransitionActions.ResetStatics();
            foreach (Object createdObject in createdObjects)
            {
                Object.DestroyImmediate(createdObject);
            }
            createdObjects.Clear();
        }

        [Test]
        public async Task HideThenShowGoesThroughTheFourStatesInOrder()
        {
            Assert.AreEqual(ScreenState.Visible, TransitionActions.ScreenState);

            Task hide = manager.HideScreen();
            Assert.AreEqual(ScreenState.Hiding, TransitionActions.ScreenState);
            await hide;
            Assert.AreEqual(ScreenState.Hidden, TransitionActions.ScreenState);
            Task show = manager.ShowScreen();
            Assert.AreEqual(ScreenState.Showing, TransitionActions.ScreenState);
            await show;

            Assert.AreEqual("Hiding,Hidden,Showing,Visible", string.Join(",", seenStates));
            Assert.AreEqual(ScreenState.Visible, TransitionActions.ScreenState);
        }

        [Test]
        public async Task TheStepEventsUseTheGlossaryOrder()
        {
            List<string> steps = new List<string>();
            TransitionEventBus.OnStartHideScreenTransition += () => steps.Add("start hide");
            TransitionEventBus.OnEndHideScreenTransition += () => steps.Add("end hide");
            TransitionEventBus.OnStartShowScreenTransition += () => steps.Add("start show");
            TransitionEventBus.OnEndShowScreenTransition += () => steps.Add("end show");

            await manager.HideScreen();
            await manager.ShowScreen();

            Assert.AreEqual("start hide,end hide,start show,end show", string.Join(",", steps));
        }

        // Stage 7: a scene change asked for while the start-up Show screen still runs must still cover the screen.
        [Test]
        public async Task HideScreenAskedForWhileShowScreenRunsWaitsAndThenHides()
        {
            Task show = manager.ShowScreen();
            Task hide = manager.HideScreen();
            await show;
            await hide;

            Assert.AreEqual(ScreenState.Hidden, TransitionActions.ScreenState, string.Join(",", seenStates));
            Assert.AreEqual("Showing,Visible,Hiding,Hidden", string.Join(",", seenStates));
        }

        [Test]
        public async Task HidingTheScreenLeavesTimeScaleAlone()
        {
            float configuredTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            try
            {
                await manager.HideScreen();
                await manager.ShowScreen();

                Assert.AreEqual(0f, Time.timeScale, "game management owns time (ADR 0010)");
            }
            finally
            {
                Time.timeScale = configuredTimeScale;
            }
        }
    }
}
