using UnityEngine;

namespace AbstractPixel.Core
{
    /// <summary> A MonoBehaviour-based singleton that persists across scene loads. Automatically creates an instance if none exists when accessed.</summary>
    /// <typeparam name="T">The type of the singleton component.</typeparam>
    /// <remarks>PersistentSingleton is commonly used to implement a simple singleton pattern for MonoBehaviour components,
    /// allowing a single instance to persist across scene loads. The instance is automatically created if it doesn't exist
    /// and destroyed if a duplicate is found. This class is not thread-safe and should be used on the main Unity thread.</remarks> 
    public class PersistentSingleton<T> : MonoBehaviour where T : Component
    {
        public bool MarkAsDontdestroyOnLoad = true;

        protected static T instance;
        protected static bool isApplicationQuitting = false;

        public static bool HasInstance => instance != null;
        public static T TryGetInstance() => HasInstance ? instance : null;


        static PersistentSingleton()
        {
            StaticsResetter.OnResetStatics += ResetState;
        }

        private static void ResetState()
        {
            instance = null;
            isApplicationQuitting = false;
        }

        public static T Instance
        {
            get
            {
                if (instance == null && !isApplicationQuitting)
                {
                    instance = FindAnyObjectByType<T>();
                    if (instance == null)
                    {
                        var go = new GameObject(typeof(T).Name + " Auto-Generated");
                        instance = go.AddComponent<T>();
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Make sure to call base.Awake() in override if you need awake.
        /// </summary>
        protected virtual void Awake()
        {
            if (!Application.isPlaying || isApplicationQuitting) return;
            InitializeSingleton();
        }

        protected virtual void InitializeSingleton()
        {
            if (MarkAsDontdestroyOnLoad)
            {
                transform.SetParent(null);
            }

            if (instance == null)
            {
                instance = this as T;
                if (MarkAsDontdestroyOnLoad)
                {
                    transform.SetParent(null);
                    DontDestroyOnLoad(instance);
                }
            }
            else
            {
                if (instance != this)
                {
                    Destroy(gameObject);
                }
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private void OnApplicationQuit()
        {
            isApplicationQuitting = true;
        }
    }
}