using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Where a new spawn point position stands on the ground: its preview's lowest point 5 cm above the surface, facing
    /// a given forward. Upright on a slope, it is raised by how much the ground climbs under its footprint, so no edge
    /// starts inside the ground; with Align to surface it tilts to the ground instead. Shift+click, "Add spawn point to
    /// this scene group" and the sinking check use the same rule.
    /// </summary>
    internal static class GroundPlacement
    {
        public const float GroundGap = 0.05f;
        private const float SteepestSlopeToRaise = 80f;

        public static Pose StandingPose(RaycastHit _hit, Vector3 _forward, PreviewShape _shape, bool _alignToSurface)
        {
            Vector3 flatForward = Vector3.ProjectOnPlane(_forward, Vector3.up);
            if (flatForward.sqrMagnitude < 0.0001f)
            {
                flatForward = Vector3.forward;
            }
            Quaternion rotation = Quaternion.LookRotation(flatForward.normalized, Vector3.up);
            if (_alignToSurface)
            {
                rotation = Quaternion.FromToRotation(Vector3.up, _hit.normal) * rotation;
                Vector3 alongNormal = _hit.normal * (GroundGap - _shape.LowestPoint);
                return new Pose(_hit.point + alongNormal, rotation);
            }
            float lift = GroundGap - _shape.LowestPoint + SlopeRiseUnder(_shape, _hit.normal);
            return new Pose(_hit.point + Vector3.up * lift, rotation);
        }

        /// <summary>How much the ground can climb under the footprint of an upright object on this slope.</summary>
        public static float SlopeRiseUnder(PreviewShape _shape, Vector3 _normal)
        {
            float slope = Mathf.Min(EditorGround.SlopeOf(_normal), SteepestSlopeToRaise);
            return FootprintRadius(_shape) * Mathf.Tan(slope * Mathf.Deg2Rad);
        }

        /// <summary>How far, in metres, the preview's bottom sinks into the ground at its lowest corner; 0 when it clears it.</summary>
        public static float SinkDepth(PreviewShape _shape, Pose _pose)
        {
            Bounds bounds = _shape.LocalBounds;
            float deepest = 0f;
            for (int corner = 0; corner < 4; corner++)
            {
                float x = (corner & 1) == 0 ? bounds.min.x : bounds.max.x;
                float z = (corner & 2) == 0 ? bounds.min.z : bounds.max.z;
                Vector3 world = _pose.position + _pose.rotation * new Vector3(x, bounds.min.y, z);
                Ray down = new Ray(world + Vector3.up * 3f, Vector3.down);
                if (!EditorGround.TryHit(down, out RaycastHit hit) || hit.distance > 6f)
                {
                    continue;
                }
                deepest = Mathf.Max(deepest, hit.point.y - world.y);
            }
            return deepest;
        }

        private static float FootprintRadius(PreviewShape _shape)
        {
            Bounds bounds = _shape.LocalBounds;
            float x = Mathf.Max(Mathf.Abs(bounds.min.x), Mathf.Abs(bounds.max.x));
            float z = Mathf.Max(Mathf.Abs(bounds.min.z), Mathf.Abs(bounds.max.z));
            return Mathf.Sqrt(x * x + z * z);
        }
    }
}
