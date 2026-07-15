using UnityEngine;

namespace AbstractPixel.Tooltip
{
    public class TooltipPositioner : MonoBehaviour
    {
        private RectTransform rectTransform;
        private TooltipPositioningStrategy currentStrategy;
        private Transform targetTransform;
        private bool isPositioning = false;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        // Called by TooltipView during Initialize()
        public void Setup(RectTransform _toolTipTransform,TooltipConfig _toolTipConfig, Transform _hoveredTarget)
        {
            rectTransform = _toolTipTransform;
            currentStrategy = _toolTipConfig.PositioningStrategy;
            targetTransform = _hoveredTarget;
            currentStrategy = _toolTipConfig.PositioningStrategy.Clone();
        }

        public void EnablePositioning() => isPositioning = true;
        public void DisablePositioning() => isPositioning = false;

        private void LateUpdate()
        {
            if (!isPositioning || currentStrategy == null)
            {
                return;
            }
            currentStrategy.ExecutePositioning(rectTransform, targetTransform);
        }
    }
}