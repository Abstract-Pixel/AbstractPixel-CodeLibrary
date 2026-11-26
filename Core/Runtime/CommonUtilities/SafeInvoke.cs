using System;
using UnityEngine;

namespace AbstractPixel.Core
{
    /// <summary>
    /// Raises an event so that one listener that throws is logged and the others still run. For events the library
    /// raises in the middle of its own work, where an exception in game code must not stop that work.
    /// </summary>
    public static class SafeInvoke
    {
        public static void Each(Action _listeners)
        {
            if (_listeners == null)
            {
                return;
            }
            foreach (Delegate listener in _listeners.GetInvocationList())
            {
                try
                {
                    ((Action)listener).Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        public static void Each<T>(Action<T> _listeners, T _value)
        {
            if (_listeners == null)
            {
                return;
            }
            foreach (Delegate listener in _listeners.GetInvocationList())
            {
                try
                {
                    ((Action<T>)listener).Invoke(_value);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        public static void Each<TFirst, TSecond>(Action<TFirst, TSecond> _listeners, TFirst _first, TSecond _second)
        {
            if (_listeners == null)
            {
                return;
            }
            foreach (Delegate listener in _listeners.GetInvocationList())
            {
                try
                {
                    ((Action<TFirst, TSecond>)listener).Invoke(_first, _second);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
    }
}
