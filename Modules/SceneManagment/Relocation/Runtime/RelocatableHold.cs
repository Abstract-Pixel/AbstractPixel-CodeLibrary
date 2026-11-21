using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// What one relocatable's hold changed, so Release gives each component back its own state. Hold covers every
    /// Rigidbody, CharacterController and NavMeshAgent in the relocatable's hierarchy that is active, except those under
    /// a nested relocatable (that one holds its own), plus bodies joined to them from outside, which move with it.
    /// </summary>
    internal sealed class RelocatableHold
    {
        private const float PositionTolerance = 0.01f;
        private const float RotationToleranceDegrees = 0.5f;

        private struct HeldBody
        {
            public Rigidbody Body;
            public bool WasKinematic;
            public RigidbodyInterpolation Interpolation;
        }

        private struct HeldController
        {
            public CharacterController Controller;
            public bool WasEnabled;
        }

        private struct HeldAgent
        {
            public NavMeshAgent Agent;
            public bool WasStopped;
        }

        private struct JoinedBody
        {
            public Rigidbody Body;
            public Vector3 PositionFromRoot;
            public Quaternion RotationFromRoot;
        }

        private readonly Relocatable owner;
        private readonly List<HeldBody> bodies = new List<HeldBody>();
        private readonly List<HeldController> controllers = new List<HeldController>();
        private readonly List<HeldAgent> agents = new List<HeldAgent>();
        private readonly List<IRelocationHold> hooks = new List<IRelocationHold>();
        private readonly List<JoinedBody> joinedBodies = new List<JoinedBody>();
        private readonly List<Collider> ownColliders = new List<Collider>();

        private RelocatableHold(Relocatable _owner)
        {
            owner = _owner;
        }

        /// <summary>Holds everything active in the relocatable's hierarchy and calls OnHold on its IRelocationHold components.</summary>
        public static RelocatableHold Start(Relocatable _owner)
        {
            RelocatableHold hold = new RelocatableHold(_owner);
            hold.HoldWhatIsActiveNow();
            return hold;
        }

        /// <summary>The relocatable's own colliders, which the ground check ignores.</summary>
        public List<Collider> OwnColliders
        {
            get { return ownColliders; }
        }

        /// <summary>
        /// Holds bodies, controllers and agents that are active now and not held yet, such as a Rigidbody added since
        /// Hold. Relocate calls it again just before it moves anything.
        /// </summary>
        public void HoldWhatIsActiveNow()
        {
            foreach (Rigidbody body in owner.GetComponentsInChildren<Rigidbody>(false))
            {
                if (BelongsToOwner(body) && !IsHeld(body))
                {
                    HoldBody(body);
                }
            }
            foreach (CharacterController controller in owner.GetComponentsInChildren<CharacterController>(false))
            {
                if (BelongsToOwner(controller) && !IsHeld(controller))
                {
                    controllers.Add(new HeldController { Controller = controller, WasEnabled = controller.enabled });
                    controller.enabled = false;
                }
            }
            foreach (NavMeshAgent agent in owner.GetComponentsInChildren<NavMeshAgent>(false))
            {
                if (BelongsToOwner(agent) && !IsHeld(agent))
                {
                    HoldAgent(agent);
                }
            }
            CollectOwnColliders();
            HoldNewHooks();
        }

        /// <summary>
        /// Adds bodies joined to this relocatable's bodies from outside its hierarchy, following joints in both
        /// directions until no new body is found, so a trailer or a chain moves as one with it. A body that belongs to
        /// another relocatable is left to that one, with a warning naming both.
        /// </summary>
        public void HoldJoinedBodies(Joint[] _jointsInScenes)
        {
            bool foundOne = true;
            while (foundOne)
            {
                foundOne = false;
                foreach (Joint joint in _jointsInScenes)
                {
                    Rigidbody joinedBody = FindBodyJoinedToThisOne(joint);
                    if (joinedBody == null)
                    {
                        continue;
                    }
                    Relocatable otherRelocatable = joinedBody.GetComponentInParent<Relocatable>();
                    if (otherRelocatable != null)
                    {
                        Debug.LogWarning("[Relocation] '" + owner.name + "' is joined to '" + otherRelocatable.name + "', which is relocated on its own. Their joint may pull them together on release.", owner);
                        continue;
                    }
                    HoldBody(joinedBody);
                    joinedBodies.Add(new JoinedBody { Body = joinedBody });
                    foundOne = true;
                }
            }
        }

        /// <summary>
        /// Writes the target pose to the transform and to every held body, and moves joined bodies by the same offset
        /// and turn. NavMeshAgents are moved with Warp. Returns false, with the reason, when an agent cannot be placed.
        /// </summary>
        public bool MoveTo(Pose _target, out string _problem)
        {
            _problem = null;
            Transform root = owner.transform;
            RememberJoinedOffsets(root);

            root.SetPositionAndRotation(_target.position, _target.rotation);
            foreach (HeldBody held in bodies)
            {
                if (held.Body == null)
                {
                    continue;
                }
                held.Body.position = held.Body.transform.position;
                held.Body.rotation = held.Body.transform.rotation;
            }
            MoveJoinedBodies(_target);
            return WarpAgents(out _problem);
        }

        /// <summary>True when the relocatable is at the target and every held body agrees with its transform.</summary>
        public bool IsAt(Pose _target)
        {
            Transform root = owner.transform;
            if (!IsClose(root.position, root.rotation, _target))
            {
                return false;
            }
            foreach (HeldBody held in bodies)
            {
                if (held.Body == null)
                {
                    continue;
                }
                Pose bodyTransformPose = new Pose(held.Body.transform.position, held.Body.transform.rotation);
                if (!IsClose(held.Body.position, held.Body.rotation, bodyTransformPose))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>Gives each held component back its own state and calls OnRelease on the IRelocationHold components that got OnHold.</summary>
        public void Restore()
        {
            foreach (HeldBody held in bodies)
            {
                if (held.Body == null)
                {
                    continue;
                }
                held.Body.isKinematic = held.WasKinematic;
                held.Body.interpolation = held.Interpolation;
            }
            foreach (HeldController held in controllers)
            {
                if (held.Controller != null)
                {
                    held.Controller.enabled = held.WasEnabled;
                }
            }
            foreach (HeldAgent held in agents)
            {
                if (held.Agent != null && held.Agent.isActiveAndEnabled && held.Agent.isOnNavMesh)
                {
                    held.Agent.isStopped = held.WasStopped;
                }
            }
            foreach (IRelocationHold hook in hooks)
            {
                CallHook(hook, false);
            }
        }

        public bool Owns(Collider _collider)
        {
            if (_collider.transform.IsChildOf(owner.transform))
            {
                return true;
            }
            foreach (JoinedBody joined in joinedBodies)
            {
                if (joined.Body != null && _collider.attachedRigidbody == joined.Body)
                {
                    return true;
                }
            }
            return false;
        }

        // A component belongs to the nearest relocatable above it, so a nested relocatable holds its own bodies once.
        private bool BelongsToOwner(Component _component)
        {
            Relocatable nearest = _component.GetComponentInParent<Relocatable>(true);
            return nearest == owner;
        }

        private void HoldBody(Rigidbody _body)
        {
            bodies.Add(new HeldBody { Body = _body, WasKinematic = _body.isKinematic, Interpolation = _body.interpolation });
            // Velocities are zeroed while the body is still dynamic: Unity warns when they are set on a kinematic body.
            if (!_body.isKinematic)
            {
                _body.linearVelocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;
            }
            _body.isKinematic = true;
            // Interpolation off, so Unity never writes an older pose back over the relocated transform.
            _body.interpolation = RigidbodyInterpolation.None;
        }

        private void HoldAgent(NavMeshAgent _agent)
        {
            bool wasStopped = false;
            if (_agent.isOnNavMesh)
            {
                wasStopped = _agent.isStopped;
                _agent.isStopped = true;
                _agent.velocity = Vector3.zero;
            }
            agents.Add(new HeldAgent { Agent = _agent, WasStopped = wasStopped });
        }

        private bool IsHeld(Rigidbody _body)
        {
            foreach (HeldBody held in bodies)
            {
                if (held.Body == _body)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsHeld(CharacterController _controller)
        {
            foreach (HeldController held in controllers)
            {
                if (held.Controller == _controller)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsHeld(NavMeshAgent _agent)
        {
            foreach (HeldAgent held in agents)
            {
                if (held.Agent == _agent)
                {
                    return true;
                }
            }
            return false;
        }

        // The other end of a joint when exactly one end is held by this relocatable and the other is not.
        private Rigidbody FindBodyJoinedToThisOne(Joint _joint)
        {
            if (_joint == null || _joint.connectedBody == null)
            {
                return null;
            }
            Rigidbody jointBody = _joint.GetComponent<Rigidbody>();
            Rigidbody connectedBody = _joint.connectedBody;
            bool holdsJointEnd = jointBody != null && IsHeld(jointBody);
            bool holdsConnectedEnd = IsHeld(connectedBody);
            if (holdsJointEnd && !holdsConnectedEnd)
            {
                return connectedBody;
            }
            if (holdsConnectedEnd && !holdsJointEnd)
            {
                return jointBody;
            }
            return null;
        }

        private void RememberJoinedOffsets(Transform _root)
        {
            Quaternion inverseRootRotation = Quaternion.Inverse(_root.rotation);
            for (int index = 0; index < joinedBodies.Count; index++)
            {
                JoinedBody joined = joinedBodies[index];
                if (joined.Body == null)
                {
                    continue;
                }
                Transform joinedTransform = joined.Body.transform;
                joined.PositionFromRoot = inverseRootRotation * (joinedTransform.position - _root.position);
                joined.RotationFromRoot = inverseRootRotation * joinedTransform.rotation;
                joinedBodies[index] = joined;
            }
        }

        private void MoveJoinedBodies(Pose _target)
        {
            foreach (JoinedBody joined in joinedBodies)
            {
                if (joined.Body == null)
                {
                    continue;
                }
                Vector3 position = _target.position + _target.rotation * joined.PositionFromRoot;
                Quaternion rotation = _target.rotation * joined.RotationFromRoot;
                joined.Body.transform.SetPositionAndRotation(position, rotation);
                joined.Body.position = position;
                joined.Body.rotation = rotation;
            }
        }

        private bool WarpAgents(out string _problem)
        {
            _problem = null;
            foreach (HeldAgent held in agents)
            {
                if (held.Agent == null || !held.Agent.isActiveAndEnabled)
                {
                    continue;
                }
                bool placed = held.Agent.Warp(held.Agent.transform.position);
                if (!placed)
                {
                    _problem = "its NavMeshAgent '" + held.Agent.name + "' could not be placed on the NavMesh at the spawn point position";
                    return false;
                }
            }
            return true;
        }

        private void CollectOwnColliders()
        {
            ownColliders.Clear();
            foreach (Collider collider in owner.GetComponentsInChildren<Collider>(false))
            {
                if (!collider.isTrigger)
                {
                    ownColliders.Add(collider);
                }
            }
        }

        // IRelocationHold components that are new since the last look get OnHold; each gets it once per hold.
        private void HoldNewHooks()
        {
            foreach (IRelocationHold hook in owner.GetComponentsInChildren<IRelocationHold>(false))
            {
                if (hooks.Contains(hook) || !BelongsToOwner((Component)hook))
                {
                    continue;
                }
                hooks.Add(hook);
                CallHook(hook, true);
            }
        }

        private void CallHook(IRelocationHold _hook, bool _hold)
        {
            if (_hook is Object unityObject && unityObject == null)
            {
                return;
            }
            try
            {
                if (_hold)
                {
                    _hook.OnHold();
                }
                else
                {
                    _hook.OnRelease();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex, owner);
            }
        }

        private static bool IsClose(Vector3 _position, Quaternion _rotation, Pose _target)
        {
            float distance = Vector3.Distance(_position, _target.position);
            float angle = Quaternion.Angle(_rotation, _target.rotation);
            return distance <= PositionTolerance && angle <= RotationToleranceDegrees;
        }
    }
}
