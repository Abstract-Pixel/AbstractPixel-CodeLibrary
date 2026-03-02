using UnityEngine;

public class LazySingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<T>();
                if(instance != null) return instance;
                instance = new GameObject(typeof(T).Name).AddComponent<T>();
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this as T;

    }

    protected virtual void OnDestroy()
    {
        instance = null;       
    }
}
