using UnityEngine;

namespace AbstractPixel.Tooltip
{
    [System.Serializable]
    public abstract class TooltipPositioningStrategy
    {
        [Tooltip("Multiplier to apply to Custom Offset. To scale the offsets unit,if too big or small")]
        public float UnitMultiplier = 1f;
        [Tooltip("Custom offset to apply to the tooltip position. This offset is applied with the unit multiplier.")]
        public Vector3 CustomOffset;
        public abstract void ExecutePositioning(RectTransform tooltipRect, Transform target);

        public TooltipPositioningStrategy Clone()
        {
            return (TooltipPositioningStrategy)this.MemberwiseClone();
        }
    }
}
