using UnityEngine;

namespace AbstractPixel.Core
{
    /// <summary> A MonoBehaviour-based singleton that does not persist across scene loads. Automatically finds an instance if none exists when accessed.</summary>
    /// <typeparam name="T">The type of the singleton component.</typeparam>
    /// <remarks>MonoSingleton is commonly used to implement a simple singleton pattern for MonoBehaviour components,
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T instance;
        public static bool HasInstance => instance != null;
        public static T TryGetInstance() => HasInstance ? instance : null;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<T>();
                }

                return instance;
            }
        }

        /// <summary>
        /// Make sure to call base.Awake() in override if you need awake.
        /// </summary>
        protected virtual void Awake()
        {
            InitializeSingleton();
        }

        protected virtual void InitializeSingleton()
        {
            if (!Application.isPlaying) return;
            instance = this as T;
        }
    }
}

