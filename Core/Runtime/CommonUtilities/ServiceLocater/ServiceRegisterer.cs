using UnityEngine;
using System;

namespace AbstractPixel.Core
{
    public class ServiceRegisterer : MonoBehaviour
    {
        [SerializeField] Component referencedServiceToRegister;
        [SerializeField] bool unregisterOnDisable = true;
        [SerializeField] bool unregisterOnDestroy = true;


        private void Awake()
        {
            if(referencedServiceToRegister == null)
            {
                return;
            }
            Type trueType = referencedServiceToRegister.GetType();
            ServiceLocator.Register(trueType, referencedServiceToRegister);
        }


        private void OnDestroy()
        {
            Type trueType = referencedServiceToRegister.GetType();
            ServiceLocator.Unregister(trueType);
        }

        private void OnDisable()
        {
            Type trueType = referencedServiceToRegister.GetType();
            ServiceLocator.Unregister(trueType);
        }

    }
}