using TMPro;
using UnityEngine;

namespace AbstractPixel.Core.UI
{
    /// <summary>
    /// Shows a single-line text in full at a readable size, even when it is longer than its slot.
    /// - Text that fits (down to its auto-size minimum) stays still and auto-sizes as usual.
    /// - Longer text keeps its largest size and slides slowly: hold at the start, slide to the end, hold, slide back.
    /// Put this on the text. Its parent is the visible slot and needs a RectMask2D so the sliding text is clipped.
    /// Call Refresh() after changing the text. Per frame only anchoredPosition moves, so nothing is allocated and
    /// no text mesh is rebuilt.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class TextMarquee : MonoBehaviour
    {
        [Tooltip("Slide speed in canvas units per second.")]
        [SerializeField, Min(1f)] private float scrollSpeed = 45f;

        [Tooltip("Seconds the text rests at each end before sliding again.")]
        [SerializeField, Min(0f)] private float holdSeconds = 1.2f;

        [Tooltip("Menus run on unscaled time so they keep moving while the game is paused.")]
        [SerializeField] private bool useUnscaledTime = true;

        private TMP_Text text;
        private RectTransform textRect;
        private RectTransform slotRect;

        private bool isScrolling;
        private float measuredSlotWidth;
        private float scrollDistance;
        private float cycleTime;
        private float slideSeconds;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (text != null)
            {
                return;
            }

            text = GetComponent<TMP_Text>();
            textRect = text.rectTransform;
            slotRect = (RectTransform)transform.parent;

            // Left-anchored inside the slot, so a wider text starts at the slot's left edge and slides left.
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(0f, 1f);
            textRect.pivot = new Vector2(0f, 0.5f);
        }

        private void OnDisable()
        {
            ResetPosition();
        }

        /// <summary>Measures the current text against the slot and starts or stops sliding.</summary>
        public void Refresh()
        {
            EnsureInitialized();

            float slotWidth = slotRect.rect.width;
            measuredSlotWidth = slotWidth;
            float widthAtLargestSize = MeasureWidth(MaxFontSize());
            float widthAtSmallestSize = widthAtLargestSize * MinFontSize() / MaxFontSize();

            bool fitsInSlot = widthAtSmallestSize <= slotWidth;
            isScrolling = !fitsInSlot;
            cycleTime = 0f;

            if (fitsInSlot)
            {
                SetTextWidth(slotWidth);
            }
            else
            {
                // As wide as the whole text at its largest size, so auto-size keeps the largest (most readable) size.
                SetTextWidth(widthAtLargestSize);
                scrollDistance = widthAtLargestSize - slotWidth;
                slideSeconds = scrollDistance / scrollSpeed;
            }

            ResetPosition();
        }

        private void Update()
        {
            // The slot was resized (e.g. its first layout pass after switching on): measure again.
            bool slotWidthChanged = slotRect.rect.width != measuredSlotWidth;
            if (slotWidthChanged)
            {
                Refresh();
            }

            if (!isScrolling)
            {
                return;
            }

            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float cycleLength = (holdSeconds + slideSeconds) * 2f;
            cycleTime = Mathf.Repeat(cycleTime + deltaTime, cycleLength);

            float offset = -scrollDistance * GetSlideProgress(cycleTime);
            textRect.anchoredPosition = new Vector2(offset, textRect.anchoredPosition.y);
        }

        /// <summary>0 at the start, 1 at the end: hold, slide out, hold, slide back (smoothed at each end).</summary>
        private float GetSlideProgress(float _cycleTime)
        {
            float slideOutEnd = holdSeconds + slideSeconds;
            float secondHoldEnd = slideOutEnd + holdSeconds;

            if (_cycleTime < holdSeconds)
            {
                return 0f;
            }
            if (_cycleTime < slideOutEnd)
            {
                return Mathf.SmoothStep(0f, 1f, (_cycleTime - holdSeconds) / slideSeconds);
            }
            if (_cycleTime < secondHoldEnd)
            {
                return 1f;
            }
            return Mathf.SmoothStep(1f, 0f, (_cycleTime - secondHoldEnd) / slideSeconds);
        }

        private float MeasureWidth(float _fontSize)
        {
            float currentSize = text.fontSize;
            text.fontSize = _fontSize;
            float preferredWidth = text.GetPreferredValues(text.text, float.PositiveInfinity, float.PositiveInfinity).x;
            text.fontSize = currentSize;
            return preferredWidth;
        }

        private float MaxFontSize()
        {
            return text.enableAutoSizing ? text.fontSizeMax : text.fontSize;
        }

        private float MinFontSize()
        {
            return text.enableAutoSizing ? text.fontSizeMin : text.fontSize;
        }

        private void SetTextWidth(float _width)
        {
            textRect.sizeDelta = new Vector2(_width, 0f);
        }

        private void ResetPosition()
        {
            if (textRect == null)
            {
                return;
            }
            textRect.anchoredPosition = new Vector2(0f, textRect.anchoredPosition.y);
        }
    }
}
