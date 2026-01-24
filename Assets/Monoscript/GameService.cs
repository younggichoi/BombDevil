using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple service locator for managing global services and managers.
/// </summary>
public static class GameService
{
    private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

    public static void Register<T>(T service) where T : class
    {
        var type = typeof(T);
        if (services.ContainsKey(type))
        {
            Debug.LogWarning($"Service of type {type.Name} is already registered. Overwriting.");
            services[type] = service;
        }
        else
        {
            services.Add(type, service);
        }
    }

    public static T Get<T>() where T : class
    {
        var type = typeof(T);
        if (services.TryGetValue(type, out var service))
        {
            return service as T;
        }
        
        Debug.LogError($"Service of type {type.Name} not found.");
        return null;
    }

    public static void Clear()
    {
        services.Clear();
        Debug.Log("All services cleared.");
    }
}
