using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// Wait for ground below, one look: a ray cast down from just above a relocatable's position, 5 m long, against the
    /// Ground layers, ignoring triggers and the relocatable's own colliders. It never moves anything. It also measures
    /// how deep the relocatable's colliders start inside the ground it found. Allocates nothing per call.
    /// </summary>
    internal sealed class GroundCheck
    {
        /// <summary>The ray starts this far above the position, so a pivot resting on the ground still finds it.</summary>
        public const float RayLift = 0.5f;
        public const float RayLength = 5f;
        /// <summary>Colliders deeper than this inside the ground would be pushed out hard on release.</summary>
        public const float DeepestAllowedOverlap = 0.25f;

        private readonly RaycastHit[] hits = new RaycastHit[32];

        public bool TryFindGround(RelocatableHold _hold, Vector3 _position, int _groundLayers, out Collider _ground)
        {
            _ground = null;
            Vector3 origin = _position + Vector3.up * RayLift;
            int hitCount = Physics.RaycastNonAlloc(origin, Vector3.down, hits, RayLift + RayLength, _groundLayers, QueryTriggerInteraction.Ignore);
            float nearest = float.MaxValue;
            for (int index = 0; index < hitCount; index++)
            {
                RaycastHit hit = hits[index];
                if (_hold.Owns(hit.collider))
                {
                    continue;
                }
                if (hit.distance < nearest)
                {
                    nearest = hit.distance;
                    _ground = hit.collider;
                }
            }
            return _ground != null;
        }

        /// <summary>How deep, in metres, the relocatable's colliders start inside the ground collider.</summary>
        public float OverlapDepth(RelocatableHold _hold, Collider _ground)
        {
            float deepest = 0f;
            Transform groundTransform = _ground.transform;
            foreach (Collider own in _hold.OwnColliders)
            {
                if (own == null || !own.enabled || !CanMeasure(own))
                {
                    continue;
                }
                Transform ownTransform = own.transform;
                bool overlaps = Physics.ComputePenetration(own, ownTransform.position, ownTransform.rotation, _ground, groundTransform.position, groundTransform.rotation, out Vector3 _, out float depth);
                if (overlaps && depth > deepest)
                {
                    deepest = depth;
                }
            }
            return deepest;
        }

        // ComputePenetration needs a primitive or a convex mesh on its first side.
        private static bool CanMeasure(Collider _collider)
        {
            if (_collider is BoxCollider || _collider is SphereCollider || _collider is CapsuleCollider)
            {
                return true;
            }
            MeshCollider mesh = _collider as MeshCollider;
            return mesh != null && mesh.convex;
        }
    }
}
