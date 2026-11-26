using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>Plays an AudioSource while its state is active and stops it when the state leaves.</summary>
    [AddComponentMenu("Abstract Pixel/Game Management/State Audio")]
    public class StateAudioReactor : MonoBehaviour
    {
        [SerializeField] private StateSO state;
        [SerializeField] private AudioSource source;
        [Tooltip("Start on enter (before the delay) instead of when the state's effects apply.")]
        [SerializeField] private bool playOnEnter;

        private StateWatch watch;

        private void Awake()
        {
            watch = new StateWatch(state);
            watch.Entered += HandleEntered;
            watch.Applied += HandleApplied;
            watch.Left += Stop;
        }

        private void OnEnable()
        {
            watch.Bind();
        }

        private void OnDisable()
        {
            watch.Unbind();
        }

        private void HandleEntered()
        {
            if (playOnEnter)
            {
                Play();
            }
        }

        private void HandleApplied()
        {
            if (!playOnEnter)
            {
                Play();
            }
        }

        private void Play()
        {
            if (source != null && !source.isPlaying)
            {
                source.Play();
            }
        }

        private void Stop()
        {
            if (source != null)
            {
                source.Stop();
            }
        }
    }
}
