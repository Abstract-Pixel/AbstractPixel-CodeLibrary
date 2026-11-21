using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace AbstractPixel.SceneManagement.SceneScreens
{
    /// <summary>
    /// Timeline marker for the launch scene's hand-off: the frame where the start scene group takes over. A
    /// <see cref="LaunchSceneReleaseReceiver"/> (or the launch scene's own director) releases the launch scene hold there.
    /// Retroactive and triggered once, so a long frame that jumps past it still fires it.
    /// </summary>
    [DisplayName("Launch scene/Release")]
    public sealed class LaunchSceneReleaseMarker : Marker, INotification, INotificationOptionProvider
    {
        private static readonly PropertyName notificationId = new PropertyName("LaunchSceneRelease");

        public PropertyName id => notificationId;

        public NotificationFlags flags => NotificationFlags.Retroactive | NotificationFlags.TriggerOnce;
    }
}
