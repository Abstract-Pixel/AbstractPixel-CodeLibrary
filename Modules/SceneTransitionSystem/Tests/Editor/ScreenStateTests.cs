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
#if UNITY_7000_0_OR_NEWER
            FadeTransitionController[] clones = Object.FindObjectsByType<FadeTransitionController>(FindObjectsInactive.Include);
#else
            FadeTransitionController[] clones = Object.FindObjectsByType<FadeTransitionController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#endif
            foreach (FadeTransitionController clone in clones)
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

        // 5B-1: a listener that throws must not leave the fade broken or the screen stuck.
        [Test]
        public async Task AThrowingScreenListenerDoesNotBreakTheFade()
        {
            TransitionEventBus.OnStartHideScreenTransition += () => throw new System.InvalidOperationException("test screen listener");
            UnityEngine.TestTools.LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("test screen listener"));

            await manager.HideScreen();
            Assert.AreEqual(ScreenState.Hidden, TransitionActions.ScreenState);
            await manager.ShowScreen();

            Assert.AreEqual(ScreenState.Visible, TransitionActions.ScreenState);
        }

        // 5B-1: an effect that throws once must not make every later Hide and Show screen throw too.
        [Test]
        public async Task AnEffectThatThrewOnceDoesNotBreakTheNextStep()
        {
            ThrowingController controller = new ThrowingController(manager.gameObject);
            System.Reflection.FieldInfo field = typeof(TransitionManager).GetField("transitionController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            object realController = field.GetValue(manager);
            field.SetValue(manager, controller);
            System.Exception first = null;
            try
            {
                await manager.HideScreen();
            }
            catch (System.Exception exception)
            {
                first = exception;
            }
            field.SetValue(manager, realController);

            await manager.ShowScreen();

            Assert.IsNotNull(first);
            Assert.AreEqual(ScreenState.Visible, TransitionActions.ScreenState);
        }

        // 5B-1: a profile with no controller prefab is one clear error, not an exception.
        [Test]
        public void AProfileWithNoControllerPrefabLogsOneError()
        {
            ImageFadeProfile empty = ScriptableObject.CreateInstance<ImageFadeProfile>();
            empty.name = "Empty profile";
            createdObjects.Add(empty);
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Empty profile.*no controller prefab"));

            manager.SetTransitionProfile(empty);

            Assert.IsFalse(manager.IsInitialized);
        }

        private sealed class ThrowingController : ITransitionController
        {
            public ThrowingController(GameObject _owner)
            {
                gameObject = _owner;
            }

            public GameObject gameObject { get; private set; }

            public void Initialize(TransitionProfile _transitionProfile)
            {
            }

            public Task PlayHideScreen()
            {
                return Task.FromException(new System.InvalidOperationException("test effect"));
            }

            public Task PlayShowScreen(System.Action _screenCountsAsVisible)
            {
                return Task.CompletedTask;
            }
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
