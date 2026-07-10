using UnityEngine;

namespace AbstractPixel.Tooltip
{
    public class InstantActivationToolTipFeedback : TooltipFeedback
    {
        GameObject toolTipHolder;
        public override void ExecuteHideFeedback()
        {
            toolTipHolder.SetActive(false);
        }

        public override void ExecuteShowFeedback()
        {
            toolTipHolder.SetActive(true);
        }

        public override void Initialize(TooltipView _tooltipView)
        {
            toolTipHolder = _tooltipView.tooltipHolder.gameObject;
        }

        
    }
}
