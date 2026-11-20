using UnityEngine;

namespace AbstractPixel.Tooltip
{
    public class FixedTargetStrategy : TooltipPositioningStrategy
    {
        public TooltipPivot PivotType = TooltipPivot.Center;

        public override void ExecutePositioning(RectTransform _tooltipRect, Transform _target, bool _isWorldSpace)
        {
            if (_target == null || _tooltipRect == null)
            {
                return;
            }

            Camera activeCamera = Camera.main;

#if UNITY_EDITOR
            if (!Application.isPlaying && UnityEditor.SceneView.lastActiveSceneView != null)
            {
                activeCamera = UnityEditor.SceneView.lastActiveSceneView.camera;
            }
#endif

            if (activeCamera == null)
            {
                return;
            }

            _tooltipRect.pivot = GetPivotVector(PivotType);
            Vector3 customScaledOffset = CustomOffset * UnitMultiplier;

            if (_isWorldSpace)
            {
                _tooltipRect.localScale = WorldScale;

                // Anchor directly to target position, scaled through local transform space
                Vector3 scaledLocalOffset = _target.TransformVector(customScaledOffset);
                _tooltipRect.position = _target.position + scaledLocalOffset;

                _tooltipRect.rotation = activeCamera.transform.rotation; // Billboard
            }
            else
            {
                _tooltipRect.localScale = Vector3.one;

                Vector3 projectedScreenPosition = activeCamera.WorldToScreenPoint(_target.position);

                if (projectedScreenPosition.z < 0f)
                {
                    _tooltipRect.position = new Vector3(-9999f, -9999f, 0f);
                    return;
                }

                _tooltipRect.position = new Vector3(projectedScreenPosition.x + customScaledOffset.x, projectedScreenPosition.y + customScaledOffset.y, customScaledOffset.z);
            }
        }

        private Vector2 GetPivotVector(TooltipPivot _pivot)
        {
            return _pivot switch
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