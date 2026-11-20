using UnityEngine;
using UnityEngine.UI;

namespace AbstractPixel.Core.UI
{
    /// <summary>
    /// Shows only a horizontal band of a RectMask2D's children, from Visible Start to Visible End (0 = the mask's left
    /// edge, 1 = its right edge). The children never move or scale; only the hard-edged clip changes.
    /// Animate the two values (Timeline, Animator) for wipes: grow Visible End for a left-to-right reveal, then grow
    /// Visible Start for a left-to-right wipe-out. Works in edit mode so Timeline previews it.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectMask2D))]
    public sealed class RectMaskRangeWipe : MonoBehaviour
    {
        [Tooltip("Left edge of the visible band, as a fraction of the mask's width.")]
        [SerializeField, Range(0f, 1f)] private float visibleStart = 0f;

        [Tooltip("Right edge of the visible band, as a fraction of the mask's width.")]
        [SerializeField, Range(0f, 1f)] private float visibleEnd = 1f;

        private RectMask2D mask;
        private RectTransform maskRect;
        private float appliedStart = -1f;
        private float appliedEnd = -1f;
        private float appliedWidth = -1f;

        public float VisibleStart
        {
            get { return visibleStart; }
            set { visibleStart = Mathf.Clamp01(value); ApplyIfChanged(); }
        }

        public float VisibleEnd
        {
            get { return visibleEnd; }
            set { visibleEnd = Mathf.Clamp01(value); ApplyIfChanged(); }
        }

        private void OnEnable()
        {
            appliedWidth = -1f;
            ApplyIfChanged();
        }

        private void OnValidate()
        {
            appliedWidth = -1f;
            ApplyIfChanged();
        }

        // Called by the animation system right after it writes the serialized values.
        private void OnDidApplyAnimationProperties()
        {
            ApplyIfChanged();
        }

        private void LateUpdate()
        {
            ApplyIfChanged();
        }

        private void ApplyIfChanged()
        {
            if (CacheComponents() == false)
            {
                return;
            }

            float width = maskRect.rect.width;
            bool isUnchanged = Mathf.Approximately(appliedStart, visibleStart) &&
                               Mathf.Approximately(appliedEnd, visibleEnd) &&
                               Mathf.Approximately(appliedWidth, width);
            if (isUnchanged)
            {
                return;
            }

            appliedStart = visibleStart;
            appliedEnd = visibleEnd;
            appliedWidth = width;
            mask.padding = BuildPadding(width);
        }

        // RectMask2D padding: x = left, y = bottom, z = right, w = top, in local units cut from each edge.
        private Vector4 BuildPadding(float _width)
        {
            float clampedEnd = Mathf.Max(visibleStart, visibleEnd);
            float leftCut = visibleStart * _width;
            float rightCut = (1f - clampedEnd) * _width;
            return new Vector4(leftCut, 0f, rightCut, 0f);
        }

        private bool CacheComponents()
        {
            if (mask == null)
            {
                mask = GetComponent<RectMask2D>();
                maskRect = transform as RectTransform;
            }

            return mask != null && maskRect != null;
        }
    }
}
