using System;
using System.Collections.Generic;
using UnityEngine;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> services = new();

    /// <summary>
    /// Registers a service instance. 
    /// Recommended to call this in Awake().
    /// </summary>
    public static void Register<T>(T service)
    {
        var type = typeof(T);

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
    /// Unregisters a service. 
    /// Call this in OnDestroy() if the service is a MonoBehaviour that gets destroyed.
    /// </summary>
    public static void Unregister<T>(T service)
    {
        var type = typeof(T);
        if (services.ContainsKey(type))
        {
            if (services[type] == (object)service)
            {
                services.Remove(type);
            }
        }
    }

    /// <summary>
    /// Retrieves a service. 
    /// Recommended to call this in Start() or later (not Awake).
    /// </summary>
    public static T Get<T>()
    {
        var type = typeof(T);

        if (!services.TryGetValue(type, out var service))
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
        var type = typeof(T);
        if (services.TryGetValue(type, out var instance))
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
