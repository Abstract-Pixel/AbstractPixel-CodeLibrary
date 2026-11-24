using System;
using UnityEngine;
using UnityEngine.Events;

namespace AbstractPixel.SceneManagement
{
    /// <summary>A component that moves an object in a manager scene or shared scene to a spawn point position on every scene change.</summary>
    /// <remarks>
    /// - Example: the player's car or a pooled enemy.
    /// - Held while the scene changes: its Rigidbodies kinematic, CharacterControllers off, NavMeshAgents stopped.
    /// - Relocated once the new scene group is open, and released once the screen counts as visible again.
    /// - One component and its relocation label are all it needs: no physics lock, no unlock script, no code.
    /// </remarks>
    [DisallowMultipleComponent]
    [AddComponentMenu("Abstract Pixel/Scene System/Relocatable")]
    public class Relocatable : MonoBehaviour
    {
        [Tooltip("Relocatables go to spawn point positions with the same label. Empty means No label.")]
        [SerializeField] private RelocationLabel label;
        [Tooltip("Instance: picks one spawn point position with this label. 0 takes the next free one in Hierarchy order.")]
        [Min(0)] [SerializeField] private int instance = 0;
        [Tooltip("Wait for ground below: stay held, with the screen covered, until a collider on the Ground layers is found under the spawn point position (3 s at most). It never moves the object onto the ground.")]
        [SerializeField] private bool waitForGroundBelow = true;
        [SerializeField] private UnityEvent onRelocated = new UnityEvent();
        [SerializeField] private UnityEvent onReleased = new UnityEvent();

        /// <summary>Raised once this relocatable is at its spawn point position, still held.</summary>
        public event Action Relocated;

        /// <summary>Raised once per scene change, in the frame of release, after its physics is restored.</summary>
        public event Action Released;

        /// <summary>The scene change number this relocatable was last relocated in (or tried to be). Read by the broadcaster.</summary>
        internal int LastRelocationChange = -1;

        /// <summary>What this relocatable's hold changed, so Release restores it. Null when not held.</summary>
        internal RelocatableHold CurrentHold;

        public RelocationLabel Label
        {
            get { return label; }
            set { label = value; }
        }

        /// <summary>The instance this relocatable asks for, counted from 1; 0 means any. Code may set it before enabling a pooled object.</summary>
        public int Instance
        {
            get { return instance; }
            set { instance = Mathf.Max(0, value); }
        }

        public bool WaitForGroundBelow
        {
            get { return waitForGroundBelow; }
            set { waitForGroundBelow = value; }
        }

        /// <summary>True while its movement and physics are paused: during a scene change, or because it has no position.</summary>
        public bool IsHeld
        {
            get { return CurrentHold != null; }
        }

        private void OnEnable()
        {
            RelocationBroadcaster broadcaster = RelocationBroadcaster.Current;
            if (broadcaster != null)
            {
                broadcaster.OnRelocatableEnabled(this);
            }
        }

        private void OnDisable()
        {
            RelocationBroadcaster broadcaster = RelocationBroadcaster.Current;
            if (broadcaster != null)
            {
                broadcaster.OnRelocatableDisabled(this);
            }
        }

        // Added in the Editor: wait for ground only when something here falls (a dynamic body or a CharacterController).
        private void Reset()
        {
            waitForGroundBelow = HasSomethingThatFalls();
        }

        internal bool HasSomethingThatFalls()
        {
            CharacterController controller = GetComponentInChildren<CharacterController>(true);
            if (controller != null)
            {
                return true;
            }
            Rigidbody[] bodies = GetComponentsInChildren<Rigidbody>(true);
            foreach (Rigidbody body in bodies)
            {
                if (!body.isKinematic)
                {
                    return true;
                }
            }
            return false;
        }

        internal void RaiseRelocated()
        {
            InvokeEach(Relocated, onRelocated);
        }

        internal void RaiseReleased()
        {
            InvokeEach(Released, onReleased);
        }

        // Each listener runs on its own, so one that throws is logged and the others still run.
        private void InvokeEach(Action _codeListeners, UnityEvent _inspectorListeners)
        {
            if (_codeListeners != null)
            {
                foreach (Delegate listener in _codeListeners.GetInvocationList())
                {
                    try
                    {
                        ((Action)listener).Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex, this);
                    }
                }
            }

            try
            {
                _inspectorListeners.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
            }
        }
    }
}
