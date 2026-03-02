using UnityEngine;

namespace AbstractPixel.Core
{
    public class PersistentSingleton<T> : MonoBehaviour where T : Component
    {
        public bool AutoUnparentOnAwake = true;

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
            if (AutoUnparentOnAwake)
            {
                transform.SetParent(null);
            }

            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
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