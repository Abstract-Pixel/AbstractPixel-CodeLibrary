#if ABSTRACTPIXEL_TIMELINE
// Needs the Timeline package (com.unity.timeline); the asmdef defines ABSTRACTPIXEL_TIMELINE when it is installed.
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace AbstractPixel.SceneManagement.SceneScreens
{
    /// <summary>Releases the launch scene hold when the Timeline reaches a <see cref="LaunchSceneReleaseMarker"/>.</summary>
    /// <remarks>
    /// - Put it next to the launch scene's PlayableDirector.
    /// - Waits until start-up has finished (or failed) and one more frame, then calls SceneActions.ReleaseLaunchScene.
    /// - Then raises On Released, where the launch scene turns its own camera and listener off and closes itself.
    /// - It also releases when it is destroyed, so leaving Play never leaves the start scene group held.
    /// </remarks>
    [DisallowMultipleComponent]
    public class LaunchSceneReleaseReceiver : MonoBehaviour, INotificationReceiver
    {
        [Tooltip("Raised once, right after the launch scene hold is released.")]
        [SerializeField] private UnityEvent onReleased = new UnityEvent();

        private bool isCued;

        public void OnNotify(Playable _origin, INotification _notification, object _context)
        {
            if (_notification is LaunchSceneReleaseMarker && !isCued)
            {
                isCued = true;
                StartCoroutine(ReleaseWhenStartUpIsDone());
            }
        }

        private IEnumerator ReleaseWhenStartUpIsDone()
        {
            while (!SceneActions.HasGameStarted && !SceneActions.HasStartUpFailed)
            {
                yield return null;
            }
            // One more frame, so the start scene group's first frame is ready before it shows itself.
            yield return null;
            SceneActions.ReleaseLaunchScene();
            onReleased.Invoke();
        }

        private void OnDestroy()
        {
            SceneActions.ReleaseLaunchScene();
        }
    }
}
#endif
