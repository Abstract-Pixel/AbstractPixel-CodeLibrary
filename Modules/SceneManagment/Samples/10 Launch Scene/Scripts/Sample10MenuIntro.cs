using UnityEngine;

namespace AbstractPixel.SceneManagement.Samples
{
    /// <summary>
    /// Sample 10: the Menu waits for the launch scene to let go before its camera moves in. RunWhenLaunchSceneReleased
    /// runs the action at once when nothing is held.
    /// </summary>
    public class Sample10MenuIntro : MonoBehaviour
    {
        private const int HeldLine = 1;
        private const float MoveSeconds = 1.5f;

        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Vector3 startOffset = new Vector3(0f, 3f, -6f);
        [SerializeField] private SampleLabel label;

        /// <summary>How many times the intro started this Play (1 expected).</summary>
        public static int StartCount { get; private set; }

        private Vector3 endPosition;
        private float moveSeconds = -1f;
        private bool shownHeld;
        private int shownCount = -1;

        // Domain reload may be off: count from zero every Play.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCount()
        {
            StartCount = 0;
        }

        private void Awake()
        {
            endPosition = cameraTransform.position;
            cameraTransform.position = endPosition + startOffset;
        }

        private void OnEnable()
        {
            SceneActions.RunWhenLaunchSceneReleased(StartIntro);
        }

        private void OnDisable()
        {
            SceneActions.CancelRunWhenLaunchSceneReleased(StartIntro);
        }

        private void StartIntro()
        {
            StartCount++;
            moveSeconds = 0f;
        }

        private void Update()
        {
            ShowHeld();
            if (moveSeconds < 0f || moveSeconds >= MoveSeconds)
            {
                return;
            }
            moveSeconds += Time.unscaledDeltaTime;
            float share = Mathf.SmoothStep(0f, 1f, moveSeconds / MoveSeconds);
            cameraTransform.position = Vector3.Lerp(endPosition + startOffset, endPosition, share);
        }

        private void ShowHeld()
        {
            bool isHeld = SceneActions.IsLaunchSceneHeld;
            bool nothingChanged = isHeld == shownHeld && StartCount == shownCount;
            if (nothingChanged || label == null)
            {
                return;
            }
            // The splash has its own label on top while it holds; this one shows again at the hand-off.
            label.enabled = isHeld == false;
            string text = "IsLaunchSceneHeld: false (the intro ran " + StartCount + " time)";
            if (isHeld)
            {
                text = "IsLaunchSceneHeld: true (the splash still holds the Menu)";
            }
            label.SetLine(HeldLine, text);
            shownHeld = isHeld;
            shownCount = StartCount;
        }
    }
}
