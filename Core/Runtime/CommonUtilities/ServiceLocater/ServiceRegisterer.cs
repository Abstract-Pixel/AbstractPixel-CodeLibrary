using UnityEngine;
using System;

namespace AbstractPixel.Core
{
    public class ServiceRegisterer : MonoBehaviour
    {
        [SerializeField] Component referencedServiceToRegister;
        // Not read yet: the component always unregisters on disable and on destroy.
#pragma warning disable 0414
        [SerializeField] bool unregisterOnDisable = true;
        [SerializeField] bool unregisterOnDestroy = true;
#pragma warning restore 0414


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