using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.Tooltip
{
    public class FollowCursorStrategy : TooltipPositioningStrategy
    {
        public TooltipPivot PivotType = TooltipPivot.TopLeft;

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
            Vector2 screenPointerPosition = GetPointerPosition(activeCamera, _target);

            if (_isWorldSpace)
            {
                _tooltipRect.localScale = WorldScale;

                // Calculate mouse position in 3D world space at the target's depth
                float depthToTarget = Vector3.Distance(activeCamera.transform.position, _target.position);
                Vector3 mouseWorldPosition = activeCamera.ScreenToWorldPoint(new Vector3(screenPointerPosition.x, screenPointerPosition.y, depthToTarget));

                // Apply offset in target local space for scale consistency
                Vector3 scaledLocalOffset = _target.TransformVector(customScaledOffset);
                _tooltipRect.position = mouseWorldPosition + scaledLocalOffset;

                _tooltipRect.rotation = activeCamera.transform.rotation; // Billboard
            }
            else
            {
                _tooltipRect.localScale = Vector3.one;

                _tooltipRect.position = new Vector3(screenPointerPosition.x + customScaledOffset.x, screenPointerPosition.y + customScaledOffset.y, customScaledOffset.z);
            }
        }

        private Vector2 GetPointerPosition(Camera _activeCamera, Transform _target)
        {
            if (Application.isPlaying && Mouse.current != null)
            {
                return Mouse.current.position.ReadValue();
            }

            // Edit Mode Preview Fallback: Use target's projected screen position
            if (_activeCamera != null && _target != null)
            {
                return _activeCamera.WorldToScreenPoint(_target.position);
            }

            return Vector2.zero;
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