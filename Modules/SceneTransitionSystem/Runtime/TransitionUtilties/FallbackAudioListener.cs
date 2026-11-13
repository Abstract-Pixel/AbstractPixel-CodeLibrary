using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneTransitions
{
    /// <summary>
    /// Keeps exactly one AudioListener alive while scenes load and unload.
    /// The game's real listener sits on a camera inside a scene that is unloaded during a transition. Once the transition
    /// screen has covered the view, this listener takes over at the camera's pose and the camera's listener is switched
    /// off, so mixer fades stay audible instead of cutting when the camera is unloaded. As soon as a camera listener is
    /// running again, this one switches off in the same frame, so two listeners never run together.
    /// Lives in a persistent scene. The listener sits on a child that is deactivated while unused, so
    /// <c>FindAnyObjectByType&lt;AudioListener&gt;()</c> elsewhere never finds the fallback while it is off.
    /// </summary>
    [DefaultExecutionOrder(10000)]
    public class FallbackAudioListener : MonoBehaviour
    {
        [Tooltip("Child object holding the fallback AudioListener. Active only while no other listener runs.")]
        [SerializeField] private AudioListener fallbackListener;

        private AudioListener primaryListener;

        /// <summary>The camera listener this component switched off at takeover; switched back on if it survives.</summary>
        private AudioListener suspendedListener;

        private void Awake()
        {
            if (fallbackListener == null)
            {
                Debug.LogError("[FallbackAudioListener] No fallback listener assigned.", this);
                enabled = false;
                return;
            }

            SetFallbackActive(false);
        }

        private void OnEnable()
        {
            TransitionEventBus.OnTransitionInCompleted += TakeOverWhileScreenCovered;
            TransitionEventBus.OnTransitionOutStarted += GiveBackToSurvivingListener;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            TransitionEventBus.OnTransitionInCompleted -= TakeOverWhileScreenCovered;
            TransitionEventBus.OnTransitionOutStarted -= GiveBackToSurvivingListener;
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void LateUpdate()
        {
            ResolveListener();
        }

        /// <summary>New scene objects have run OnEnable here, so a new camera listener is handed control at once.</summary>
        private void HandleSceneLoaded(Scene _scene, LoadSceneMode _mode)
        {
            ResolveListener();
        }

        /// <summary>The scenes are about to unload. Take over now, before the camera disappears mid-frame.</summary>
        private void TakeOverWhileScreenCovered()
        {
            if (IsRunning(primaryListener) == false)
            {
                return;
            }

            FollowPrimaryPose();
            suspendedListener = primaryListener;
            suspendedListener.enabled = false;
            primaryListener = null;
            SetFallbackActive(true);
        }

        /// <summary>The screen is revealing again. If the old camera was never unloaded, it takes its listener back.</summary>
        private void GiveBackToSurvivingListener()
        {
            bool suspendedStillExists = suspendedListener != null;
            if (suspendedStillExists == false)
            {
                return;
            }

            bool anotherListenerRuns = FindOtherRunningListener() != null;
            if (anotherListenerRuns == false)
            {
                suspendedListener.enabled = true;
            }
            suspendedListener = null;
            ResolveListener();
        }

        /// <summary>Exactly one listener: the running camera listener if there is one, otherwise the fallback.</summary>
        private void ResolveListener()
        {
            bool primaryIsRunning = IsRunning(primaryListener);
            if (primaryIsRunning == false)
            {
                primaryListener = FindOtherRunningListener();
                primaryIsRunning = primaryListener != null;
            }

            if (primaryIsRunning)
            {
                FollowPrimaryPose();
            }

            bool fallbackShouldRun = primaryIsRunning == false;
            SetFallbackActive(fallbackShouldRun);
        }

        /// <summary>Copies the real listener's pose, so 3D sounds do not jump when the fallback takes over.</summary>
        private void FollowPrimaryPose()
        {
            Transform primaryTransform = primaryListener.transform;
            fallbackListener.transform.SetPositionAndRotation(primaryTransform.position, primaryTransform.rotation);
        }

        /// <summary>Only searched while no real listener is known, which is the short gap of a scene load.</summary>
        private AudioListener FindOtherRunningListener()
        {
            AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            foreach (AudioListener listener in listeners)
            {
                bool isFallback = listener == fallbackListener;
                if (isFallback == false && IsRunning(listener))
                {
                    return listener;
                }
            }
            return null;
        }

        private static bool IsRunning(AudioListener _listener)
        {
            return _listener != null && _listener.isActiveAndEnabled;
        }

        private void SetFallbackActive(bool _active)
        {
            GameObject fallbackObject = fallbackListener.gameObject;
            if (fallbackObject.activeSelf != _active)
            {
                fallbackObject.SetActive(_active);
            }
        }
    }
}
