using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace AbstractPixel.SceneManagement.SceneScreens
{
    /// <summary>
    /// The part of a loading scene that shows loading progress to the player (glossary: loading progress view). It reads
    /// SceneActions.LoadingProgress, smooths it with its feel settings and passes it on through four UnityEvents (a
    /// designer can drag a progress bar or a feedback player in with no code) and to an ILoadingProgressDisplay written
    /// in code. The scene system tells it when Show screen starts and waits for its "full" beat and hold before Hide
    /// screen, so the screen effect and the loading scene never animate over each other.
    /// </summary>
    [DisallowMultipleComponent]
    public class LoadingProgressView : MonoBehaviour, ILoadingProgressView
    {
        [System.Serializable]
        public class ProgressEvent : UnityEvent<float>
        {
        }

        [SerializeField] private LoadingProgressFeel feel = new LoadingProgressFeel();

        [Tooltip("Optional: a component that implements ILoadingProgressDisplay. Empty uses one on this object, if any.")]
        [SerializeField] private MonoBehaviour display;

        [Tooltip("Show screen started on the loading scene: start the appear animation.")]
        [SerializeField] private UnityEvent onShown = new UnityEvent();
        [Tooltip("Every frame while shown: the smoothed loading progress, from 0 to 1.")]
        [SerializeField] private ProgressEvent onProgress = new ProgressEvent();
        [Tooltip("Every scene is open and the shown value reached 1: play the full beat.")]
        [SerializeField] private UnityEvent onFull = new UnityEvent();
        [Tooltip("The loading scene is closing.")]
        [SerializeField] private UnityEvent onHidden = new UnityEvent();

        private ILoadingProgressDisplay codeDisplay;
        private bool isShown;

        public LoadingProgressFeel Feel
        {
            get { return feel; }
        }

        /// <summary>Sets the display written in code; for setups and tests that build the view at runtime.</summary>
        public void SetDisplay(ILoadingProgressDisplay _display)
        {
            codeDisplay = _display;
        }

        private void OnEnable()
        {
            FindDisplay();
            SceneActions.RegisterLoadingProgressView(this);
        }

        private void OnDisable()
        {
            SceneActions.UnregisterLoadingProgressView(this);
            if (!isShown)
            {
                return;
            }
            isShown = false;
            if (codeDisplay != null)
            {
                codeDisplay.Hide();
            }
            onHidden.Invoke();
        }

        private void Update()
        {
            if (!isShown)
            {
                return;
            }
            float shown = feel.Step(SceneActions.LoadingProgress, Time.unscaledDeltaTime);
            onProgress.Invoke(shown);
            if (codeDisplay != null)
            {
                codeDisplay.SetProgress(shown);
            }
        }

        public void OnShowStarted()
        {
            FindDisplay();
            isShown = true;
            feel.Restart();
            if (codeDisplay != null)
            {
                codeDisplay.Show();
            }
            onShown.Invoke();
        }

        public async Task PlayFullAnimation()
        {
            // The shown value catches up first, so the bar visibly reaches the end.
            while (!feel.IsFull && this != null && isActiveAndEnabled)
            {
                await Task.Yield();
            }
            onFull.Invoke();
            if (codeDisplay != null)
            {
                await codeDisplay.PlayFull();
            }
            await WaitUnscaled(feel.HoldAtFullSeconds);
        }

        private void FindDisplay()
        {
            if (codeDisplay != null)
            {
                return;
            }
            codeDisplay = display as ILoadingProgressDisplay;
            if (codeDisplay == null)
            {
                codeDisplay = GetComponent<ILoadingProgressDisplay>();
            }
        }

        private static async Task WaitUnscaled(float _seconds)
        {
            double endTime = Time.realtimeSinceStartupAsDouble + _seconds;
            while (Time.realtimeSinceStartupAsDouble < endTime)
            {
                await Task.Yield();
            }
        }
    }
}
