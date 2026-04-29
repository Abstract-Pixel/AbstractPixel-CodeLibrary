using System;
using UnityEngine;
using UnityEngine.Events;

namespace AbstractPixel.Core
{
    public class TriggerEvent : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] LayerMask triggerMask;
        [SerializeField] bool triggerOnce = true;

        [Header("Events")]
        [SerializeField] UnityEvent onTriggeredEvent;
        public Action OnTriggered;

        GameObject lastTriggeredGameObject;
        bool isTriggered = false;

        private void OnTriggerEnter(Collider other)
        {
            if (isTriggered)
            {
                return;
            }
            if ((triggerMask.value & (1 << other.gameObject.layer)) != 0)
            {
                if (triggerOnce && lastTriggeredGameObject == other.gameObject)
                {
                    return;
                }
                lastTriggeredGameObject = other.gameObject;
                onTriggeredEvent?.Invoke();
                OnTriggered?.Invoke();
                if (triggerOnce)
                {
                    isTriggered = true;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (lastTriggeredGameObject == other.gameObject)
            {
                lastTriggeredGameObject = null;
                if (!triggerOnce)
                {
                    isTriggered = false;
                }
            }
        }


        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isTriggered)
            {
                return;
            }
            if ((triggerMask.value & (1 << collision.gameObject.layer)) != 0)
            {
                if (triggerOnce && lastTriggeredGameObject == collision.gameObject)
                {
                    return;
                }
                lastTriggeredGameObject = collision.gameObject;
                onTriggeredEvent?.Invoke();
                OnTriggered?.Invoke();
                if (triggerOnce)
                {
                    isTriggered = true;
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (lastTriggeredGameObject == collision.gameObject)
            {
                lastTriggeredGameObject = null;
                if (!triggerOnce)
                {
                    isTriggered = false;
                }
            }
        }
    }

}
