using UnityEngine;

namespace AbstractPixel.Tooltip
{
    public abstract class TooltipFeedback : MonoBehaviour
    {
        public abstract void Initialize(TooltipView _tooltipView);
        public abstract void ExecuteShowFeedback();
        public abstract void ExecuteHideFeedback();
    }
}