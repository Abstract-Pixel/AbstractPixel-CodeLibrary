using UnityEngine;

namespace AbstractPixel.Tooltip
{
    public class FixedTargetStrategy : TooltipPositioningStrategy
    {
        public TooltipPivot PivotType = TooltipPivot.Center;

       
        public override void ExecutePositioning(RectTransform tooltipRect, Transform target)
        {
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(target.position);
            Vector2 pivotVector = GetPivotVector(PivotType);
            Vector3 customScaledOffset = CustomOffset * UnitMultiplier;
            Vector3 toolTipPosition = new Vector3(screenPosition.x + customScaledOffset.x, screenPosition.y + customScaledOffset.y, customScaledOffset.z);
            tooltipRect.pivot = pivotVector;
            tooltipRect.position = toolTipPosition;
        }

        

        private Vector2 GetPivotVector(TooltipPivot pivot)
        {
            return pivot switch
            {
                TooltipPivot.Top => new Vector2(0.5f, 1f),
                TooltipPivot.Bottom => new Vector2(0.5f, 0f),
                TooltipPivot.Left => new Vector2(0f, 0.5f),
                TooltipPivot.Right => new Vector2(1f, 0.5f),
                TooltipPivot.TopLeft => new Vector2(0f, 1f),
                TooltipPivot.TopRight => new Vector2(1f, 1f),
                TooltipPivot.BottomLeft => new Vector2(0f, 0f),
                TooltipPivot.BottomRight => new Vector2(1f, 0f),
                TooltipPivot.Center => new Vector2(0.5f, 0.5f),
                _ => new Vector2(0.5f, 0.5f)
            };
        }
    }
}
