using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.Core
{
    /// <summary>Provides a global mechanism for registering and retrieving service instances by type. Enables decoupled access
    /// to shared services within the application.</summary>
    /// <remarks>ServiceLocator is commonly used to implement a simple dependency resolution pattern, allowing
    /// components to access services without direct references. Services should be registered early in the application
    /// lifecycle, typically in Awake, and retrieved after all registrations are complete, such as in Start. This class
    /// is static and thread-unsafe; concurrent access may result in race conditions. All registered services are
    /// cleared automatically when the domain reloads (e.g., when entering Play Mode in Unity).</remarks>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> services = new();

        /// <summary>
        /// Registers a service instance. 
        /// Recommended to call this in Awake().
        /// </summary>
        public static void Register<T>(T service)
        {
            Type type = typeof(T);

            if (services.ContainsKey(type))
            {
                Debug.LogWarning($"ServiceLocator: A service of type {type.Name} is already registered. Overwriting with new instance.");
                services[type] = service;
            }
            else
            {
                services.Add(type, service);
            }
        }

        /// <summary></summary>
        /// Registers a service instance by its type.
        /// </summary>
        /// <param name="type">The type of the service.</param>
        /// <param name="service">The service instance.</param>
        public static void Register(Type type, object service)
        {
            if (services.ContainsKey(type))
            {
                Debug.LogWarning($"ServiceLocator: A service of type {type.Name} is already registered. Overwriting with new instance.");
                services[type] = service;
            }
            else
            {
                services.Add(type, service);
            }
        }

        /// <summary>
        /// Unregisters a service instance.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <param name="service">The service instance to unregister.</param>
        public static void Unregister<T>(T service)
        {
            Type type = typeof(T);
            if (services.ContainsKey(type))
            {
                if (services[type] == (object)service)
                {
                    services.Remove(type);
                }
            }
        }

        /// <summary>
        /// Unregisters a service by its type.
        /// </summary>
        /// <param name="type">The type of the service to unregister.</param>
        public static void Unregister(Type type)
        {
            if (services.ContainsKey(type))
            {
                services.Remove(type);
                Debug.Log($"ServiceLocator: Successfully unregistered service of type {type.Name}.");
            }
            else
            {
                Debug.LogWarning($"ServiceLocator: Attempted to unregister {type.Name}, but it was not found.");
            }
        }


        /// <summary>
        /// Retrieves a service. 
        /// Recommended to call this in Start() or later (not Awake).
        /// </summary>
        public static T Get<T>()
        {
            Type type = typeof(T);

            if (!services.TryGetValue(type, out object service))
            {
                Debug.LogError($"ServiceLocator: Critical Error! Service of type {type.Name} was requested but not found.\n" +
                               "1. Did you forget to Register it in Awake?\n" +
                               "2. Are you calling Get() too early (in Awake instead of Start)?\n" +
                               "3. Is the GameObject holding the service active?");
                return default;
            }

            return (T)service;
        }

        /// <summary>
        /// Safer version of Get that doesn't log an error if missing.
        /// </summary>
        public static bool TryGet<T>(out T service)
        {
            Type type = typeof(T);
            if (services.TryGetValue(type, out object instance))
            {
                service = (T)instance;
                return true;
            }

            service = default;
            return false;
        }

        /// <summary>
        /// Clears all services. Automatically called when Domain Reloads (Play Mode starts).
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            services.Clear();
        }
    }
}