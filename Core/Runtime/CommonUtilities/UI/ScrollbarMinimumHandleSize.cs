using UnityEngine;
using UnityEngine.UI;

namespace AbstractPixel.Core
{
    /// <summary>
    /// Keeps a Scrollbar handle from shrinking below a minimum size.
    /// A ScrollRect sets the handle size to viewport / content every LateUpdate, so long content
    /// makes the handle a tiny stub. This runs after the ScrollRect and raises the size back up.
    /// Dragging and scroll position still work, because the Scrollbar maps its value over the remaining track.
    /// </summary>
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(Scrollbar))]
    public sealed class ScrollbarMinimumHandleSize : MonoBehaviour
    {
        [Tooltip("Smallest handle size, as a fraction of the scrollbar track (0 to 1).")]
        [SerializeField, Range(0f, 1f)] private float minimumSize = 0.3f;

        private Scrollbar scrollbar;

        private void Awake()
        {
            scrollbar = GetComponent<Scrollbar>();
        }

        private void LateUpdate()
        {
            if (scrollbar == null)
            {
                return;
            }

            bool handleTooSmall = scrollbar.size < minimumSize;
            if (handleTooSmall)
            {
                scrollbar.size = minimumSize;
            }
        }
    }
}
