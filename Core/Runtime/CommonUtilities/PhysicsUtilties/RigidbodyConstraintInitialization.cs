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
        [SerializeField] private bool canOnlyUseOnce;
        [SerializeField] private bool useDelayBeforeUnlocking;

        [Header("Constraints")]
        [SerializeField] private PositionConstraints positionConstraints;
        [SerializeField] private RotationConstraints rotationConstraints;

        private Rigidbody targetRigidbody;
        private RigidbodyConstraints cachedConstraints;
        private bool cachedIsKinematic;
        private Coroutine timerCoroutine;
        private bool isUsedOnce; // Persists if the scene manager retains this object state

        private void Awake()
        {
            targetRigidbody = GetComponent<Rigidbody>();
            cachedConstraints = targetRigidbody.constraints;
            cachedIsKinematic = targetRigidbody.isKinematic;
            if (canOnlyUseOnce && isUsedOnce) return;
            ApplyConstraints();
        }

        private void ApplyConstraints()
        {
            targetRigidbody.isKinematic = makeItIsKinematic;
            targetRigidbody.constraints = ConvertToRigidbodyConstraints();
            targetRigidbody.linearVelocity = Vector3.zero;
            targetRigidbody.angularVelocity = Vector3.zero;
            if (lockMode == ConstraintLockMode.Timed)
            {
                timerCoroutine = StartCoroutine(TimerRoutine());
            }
        }

        private IEnumerator TimerRoutine()
        {
            yield return new WaitForSeconds(duration);
            PerformUnlock();
        }

        public void UnlockConstraints()
        {
            if (canOnlyUseOnce && isUsedOnce) return;

            if (useDelayBeforeUnlocking)
            {
                if (timerCoroutine == null) timerCoroutine = StartCoroutine(TimerRoutine());
                return;
            }

            if (lockMode == ConstraintLockMode.Permanent)
            {
                PerformUnlock();
            }
        }

        private void PerformUnlock()
        {
            targetRigidbody.constraints = cachedConstraints;
            targetRigidbody.isKinematic = cachedIsKinematic;
            isUsedOnce = true;
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
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                PerformUnlock(); // Safely reset state if disabled prematurely during a load
            }
        }
    }
}