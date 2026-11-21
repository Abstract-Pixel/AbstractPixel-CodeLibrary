using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Ground in the Editor, by the same rules as Wait for ground below at runtime: colliders on the Ground layers,
    /// triggers ignored. Relocatables are ignored too, so a crate standing on its own spawn point never counts as ground.
    /// </summary>
    internal static class EditorGround
    {
        private const float RayLift = 0.5f;
        private const float RayLength = 5f;
        private const float CameraRayLength = 5000f;

        /// <summary>The ground a Scene view ray hits first, if any.</summary>
        public static bool TryHit(Ray _ray, out RaycastHit _hit)
        {
            return TryHitAlong(_ray, CameraRayLength, out _hit);
        }

        /// <summary>The ground under a position, looking 5 m down from just above it.</summary>
        public static bool TryFindBelow(Vector3 _position, out RaycastHit _hit)
        {
            Ray down = new Ray(_position + Vector3.up * RayLift, Vector3.down);
            return TryHitAlong(down, RayLift + RayLength, out _hit);
        }

        /// <summary>The slope in degrees: the angle between the ground's normal and straight up.</summary>
        public static float SlopeOf(Vector3 _normal)
        {
            return Vector3.Angle(_normal, Vector3.up);
        }

        private static bool TryHitAlong(Ray _ray, float _length, out RaycastHit _hit)
        {
            _hit = default;
            int groundLayers = RelocationLabels.GroundLayers();
            RaycastHit[] hits = Physics.RaycastAll(_ray, _length, groundLayers, QueryTriggerInteraction.Ignore);
            float nearest = float.MaxValue;
            bool found = false;
            foreach (RaycastHit hit in hits)
            {
                bool isRelocatable = hit.collider.GetComponentInParent<Relocatable>() != null;
                if (isRelocatable || hit.distance >= nearest)
                {
                    continue;
                }
                nearest = hit.distance;
                _hit = hit;
                found = true;
            }
            return found;
        }
    }
}
