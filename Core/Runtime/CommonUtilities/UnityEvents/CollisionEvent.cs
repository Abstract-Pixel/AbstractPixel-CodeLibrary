using UnityEngine;

namespace AbstractPixel.Core
{
    using System;
    using UnityEngine;
    using UnityEngine.Events;

    namespace AbstractPixel.Core
    {
        /// <summary>
        /// Handles physical collision events (3D/2D) with layer filtering and optional one-shot execution.
        /// </summary>
        public class CollisionEvent : MonoBehaviour
        {
            [Header("Settings")]
            [SerializeField] private LayerMask collisionMask;
            [SerializeField] private bool collideOnce = true;

            [Header("Events")]
            [SerializeField] private UnityEvent onCollidedEvent;
            public Action OnCollided;

            private GameObject lastCollidedGameObject;
            private bool isCollided = false;

            // --- 3D Physics ---

            private void OnCollisionEnter(Collision _collision)
            {
                HandleCollisionEnter(_collision.gameObject);
            }

            private void OnCollisionExit(Collision _collision)
            {
                HandleCollisionExit(_collision.gameObject);
            }

            // --- 2D Physics ---

            private void OnCollisionEnter2D(Collision2D _collision)
            {
                HandleCollisionEnter(_collision.gameObject);
            }

            private void OnCollisionExit2D(Collision2D _collision)
            {
                HandleCollisionExit(_collision.gameObject);
            }

            // --- Core Logic ---

            private void HandleCollisionEnter(GameObject _target)
            {
                if (isCollided)
                {
                    return;
                }

                if ((collisionMask.value & (1 << _target.layer)) != 0)
                {
                    if (collideOnce && lastCollidedGameObject == _target)
                    {
                        return;
                    }

                    lastCollidedGameObject = _target;
                    onCollidedEvent?.Invoke();
                    OnCollided?.Invoke();

                    if (collideOnce)
                    {
                        isCollided = true;
                    }
                }
            }

            private void HandleCollisionExit(GameObject _target)
            {
                if (lastCollidedGameObject == _target)
                {
                    lastCollidedGameObject = null;
                    if (!collideOnce)
                    {
                        isCollided = false;
                    }
                }
            }
        }
    }
}
