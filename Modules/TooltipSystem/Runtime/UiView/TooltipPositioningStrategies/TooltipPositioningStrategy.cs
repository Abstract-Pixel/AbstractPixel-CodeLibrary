using UnityEngine;

namespace AbstractPixel.Tooltip
{
    [System.Serializable]
    public abstract class TooltipPositioningStrategy
    {
        [Tooltip("Scale multiplier applied to the Tooltip ONLY when in World Space.")]
        public Vector3 WorldScale = Vector3.one;

        [Tooltip("Multiplier to apply to Custom Offset. To scale the offsets unit, if too big or small")]
        public float UnitMultiplier = 1f;

        [Tooltip("Custom offset to apply to the tooltip position. This offset is applied with the unit multiplier.")]
        public Vector3 CustomOffset;

        public abstract void ExecutePositioning(RectTransform _tooltipRect, Transform _target, bool _isWorldSpace);

        public TooltipPositioningStrategy Clone()
        {
            return (TooltipPositioningStrategy)this.MemberwiseClone();
        }
    }
}