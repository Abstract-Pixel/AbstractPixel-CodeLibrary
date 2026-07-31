using UnityEngine;

namespace AbstractPixel.Tooltip
{
    public class TooltipPositioner : MonoBehaviour
    {
        private RectTransform rectTransform;
        private TooltipPositioningStrategy currentStrategy;
        private Transform targetTransform;
        private bool isPositioning = false;
        private bool isWorldSpace = false;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void Setup(RectTransform _toolTipTransform, TooltipConfig _toolTipConfig, Transform _hoveredTarget)
        {
            rectTransform = _toolTipTransform;
            targetTransform = _hoveredTarget;
            isWorldSpace = _toolTipConfig.isWorldSpace;
            currentStrategy = _toolTipConfig.PositioningStrategy.Clone();
        }

        public void ForcePositionUpdate()
        {
            if (currentStrategy == null || rectTransform == null)
            {
                return;
            }

            currentStrategy.ExecutePositioning(rectTransform, targetTransform, isWorldSpace);
        }

        public void EnablePositioning()
        {
            isPositioning = true;
        }

        public void DisablePositioning()
        {
            isPositioning = false;
        }

        // Shifted from Update() to LateUpdate() to completely eliminate 1-frame jitter 
        // when the camera or the target object is moving.
        private void LateUpdate()
        {
            if (!isPositioning || currentStrategy == null)
            {
                return;
            }

            currentStrategy.ExecutePositioning(rectTransform, targetTransform, isWorldSpace);
        }
    }
}