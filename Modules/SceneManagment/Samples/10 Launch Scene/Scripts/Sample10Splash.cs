using AbstractPixel.SceneManagement.SceneScreens;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ABSTRACTPIXEL_TIMELINE
using UnityEngine.Playables;
#endif

namespace AbstractPixel.SceneManagement.Samples
{
    /// <summary>
    /// Sample 10: the launch scene. While start-up opens the Menu group behind it, the logo grows in, the sting plays at
    /// the reveal, and the Timeline runs to its LaunchSceneReleaseMarker; the LaunchSceneReleaseReceiver then releases the
    /// hold and calls OnReleased here, which hands the screen over and closes this scene.
    /// </summary>
    public class Sample10Splash : MonoBehaviour
    {
        private const float RevealSeconds = 0.6f;
        private const float MaximumStepSeconds = 1f / 30f;
        private const string NeedsTimeline = "Sample 10's Timeline part needs the Timeline package (com.unity.timeline).";
        private const float ReleaseWithoutTimelineSeconds = 2f;

        [SerializeField] private Transform logo;
        [SerializeField] private SampleRuntimeSound sting;
        [SerializeField] private Camera splashCamera;
        [SerializeField] private AudioListener splashListener;
        [SerializeField] private SampleLabel label;

        /// <summary>Whether the launch scene hold was on at the splash's first frame.</summary>
        public static bool WasHeldAtStart { get; private set; }

        /// <summary>How many times the splash handed over this Play (1 expected).</summary>
        public static int HandOverCount { get; private set; }

        // Domain reload may be off: start every Play from zero.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRecord()
        {
            WasHeldAtStart = false;
            HandOverCount = 0;
        }

        private readonly LaunchSceneRenderSettingsGuard renderSettingsGuard = new LaunchSceneRenderSettingsGuard();
        private float shownSeconds;
        private bool stingPlayed;
        private bool released;

#if ABSTRACTPIXEL_TIMELINE
        [SerializeField] private PlayableDirector timeline;
        private LaunchSceneTimelineClock clock;
#endif

        private void Start()
        {
            WasHeldAtStart = SceneActions.IsLaunchSceneHeld;
        }

        private void Awake()
        {
            // The Menu's RenderSettings would light the splash once its scene is active: keep them dark until the hand-off.
            renderSettingsGuard.BeginGuarding();
            LaunchSceneStartUpLog.Milestone("Sample 10 splash awake");
#if ABSTRACTPIXEL_TIMELINE
            clock = new LaunchSceneTimelineClock(timeline, MaximumStepSeconds);
#else
            if (label != null)
            {
                label.SetLine(1, NeedsTimeline);
            }
#endif
            logo.localScale = Vector3.zero;
        }

        private void Update()
        {
            shownSeconds += Time.unscaledDeltaTime;
            RevealLogo();
#if ABSTRACTPIXEL_TIMELINE
            clock.Tick();
#else
            if (shownSeconds >= ReleaseWithoutTimelineSeconds)
            {
                SceneActions.ReleaseLaunchScene();
                OnReleased();
            }
#endif
        }

        // The logo grows in; the sting plays once, when it is fully shown.
        private void RevealLogo()
        {
            float share = Mathf.Clamp01(shownSeconds / RevealSeconds);
            logo.localScale = Vector3.one * share;
            bool revealed = share >= 1f;
            if (revealed && stingPlayed == false && sting != null)
            {
                stingPlayed = true;
                sting.Play();
                LaunchSceneStartUpLog.Milestone("Sample 10 logo revealed");
            }
        }

        /// <summary>Called by the LaunchSceneReleaseReceiver's On Released: hand the screen to the Menu and close.</summary>
        public void OnReleased()
        {
            if (released)
            {
                return;
            }
            released = true;
            HandOverCount++;
            renderSettingsGuard.RestoreMenuSettings();
            splashCamera.enabled = false;
            if (splashListener != null)
            {
                splashListener.enabled = false;
            }
            SceneManager.UnloadSceneAsync(gameObject.scene);
        }

        private void OnDestroy()
        {
            renderSettingsGuard.RestoreMenuSettings();
        }
    }
}
