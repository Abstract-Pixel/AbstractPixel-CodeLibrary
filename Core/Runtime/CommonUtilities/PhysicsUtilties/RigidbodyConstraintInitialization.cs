using System.Collections;
using UnityEngine;

namespace AbstractPixel.Core
{
    [RequireComponent(typeof(Rigidbody))]
    [AddComponentMenu("AbstractPixel/Physics/Rigidbody Constraint Initialization")]
    public class RigidbodyConstraintInitialization : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private ConstraintLockMode lockMode = ConstraintLockMode.Permanent;
        [SerializeField] private float duration = 2.0f;
        [SerializeField] private bool makeItIsKinematic;

        [Header("Constraints")]
        [SerializeField] private PositionConstraints positionConstraints;
        [SerializeField] private RotationConstraints rotationConstraints;

        private Rigidbody targetRigidbody;
        private RigidbodyConstraints cachedConstraints;
        private bool cachedIsKinematic;
        private Coroutine timerCoroutine;

        private void Awake()
        {
            targetRigidbody = GetComponent<Rigidbody>();

            // Cache the state existing before this script modifies it
            cachedConstraints = targetRigidbody.constraints;
            cachedIsKinematic = targetRigidbody.isKinematic;
        }

        private void Start()
        {
            ApplyConstraints();
        }

        private void ApplyConstraints()
        {
            RigidbodyConstraints newConstraints = ConvertToRigidbodyConstraints();
            targetRigidbody.isKinematic = makeItIsKinematic;

            // Apply new constraints
            targetRigidbody.constraints = newConstraints;
            if (lockMode == ConstraintLockMode.Timed)
            {
                timerCoroutine = StartCoroutine(TimerRoutine());
            }
        }

        private IEnumerator TimerRoutine()
        {
            yield return new WaitForSeconds(duration);

            // Revert specifically to the cached state
            targetRigidbody.constraints = cachedConstraints;
            targetRigidbody.isKinematic = cachedIsKinematic;
            timerCoroutine = null;

        }

        private RigidbodyConstraints ConvertToRigidbodyConstraints()
        {
            RigidbodyConstraints result = RigidbodyConstraints.None;

            if (positionConstraints.HasFlag(PositionConstraints.FreezeX)) result |= RigidbodyConstraints.FreezePositionX;
            if (positionConstraints.HasFlag(PositionConstraints.FreezeY)) result |= RigidbodyConstraints.FreezePositionY;
            if (positionConstraints.HasFlag(PositionConstraints.FreezeZ)) result |= RigidbodyConstraints.FreezePositionZ;

            if (rotationConstraints.HasFlag(RotationConstraints.FreezeX)) result |= RigidbodyConstraints.FreezeRotationX;
            if (rotationConstraints.HasFlag(RotationConstraints.FreezeY)) result |= RigidbodyConstraints.FreezeRotationY;
            if (rotationConstraints.HasFlag(RotationConstraints.FreezeZ)) result |= RigidbodyConstraints.FreezeRotationZ;

            return result;
        }

        private void OnDisable()
        {
            // Safety: Ensure we don't leave the object in a locked state if it's disabled/destroyed
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                targetRigidbody.constraints = cachedConstraints;
            }
        }
    }
}