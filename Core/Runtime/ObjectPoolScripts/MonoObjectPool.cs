using System.Collections.Generic;
using UnityEngine;

public class MonoObjectPool : MonoBehaviour
{

    public static MonoObjectPool Instance { get; private set; }

    [Header("Pool Configuration")]
    [SerializeField] private GameObject objectPrefab;
    [SerializeField] private int minToSpawn = 5;
    [SerializeField] private bool canExpand = true;
    [SerializeField] private Transform poolParent;
    [SerializeField] private int maxTotalCapacity = 20;
    [SerializeField] bool isPersistent = false;

    private Stack<GameObject> inactivePool = new Stack<GameObject>();
    private List<GameObject> activePool = new List<GameObject>();

    public int AvailableCount => inactivePool.Count;
    public int ActiveCount => activePool.Count;
    public int TotalCount => inactivePool.Count + activePool.Count;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (isPersistent)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }


        if (objectPrefab == null)
        {
            Debug.LogError($"ObjectPool: Object Prefab is not assigned on {gameObject.name}! Disabling pool.", this);
            enabled = false;
            return;
        }

        // Adjust maxTotalCapacity if not expandable
        if (!canExpand)
        {
            maxTotalCapacity = minToSpawn;
        }
        else
        {
            maxTotalCapacity = (maxTotalCapacity <= 0) ? int.MaxValue : Mathf.Max(minToSpawn, maxTotalCapacity);
        }

        // Default pool parent to this object's transform if not set
        if (poolParent == null)
        {
            poolParent = this.transform;
        }

        InitializePool();
    }

    public void InitializePool()
    {
        if (inactivePool.Count == 0 && activePool.Count == 0)
        {
            for (int i = 0; i < this.minToSpawn; i++)
            {
                if (!CanCreateObject())
                {
                    break;
                }
                CreateAndStoreGameObject();
            }
        }
    }

    public T GetFromPool<T>() where T : Component
    {
        GameObject pooledGameObject = null;

        if (inactivePool.Count > 0)
        {
            pooledGameObject = inactivePool.Peek();
            if (pooledGameObject == null)
            {
                Debug.LogError("ObjectPool: Peeked a null GameObject from the inactive pool. This should not happen. Removing it.");
                inactivePool.Pop();
            }
            else
            {
                inactivePool.Pop(); // Valid GameObject, so pop it
            }
        }

        if (pooledGameObject == null)
        {
            if (!CanCreateObject())
            {
                return null;
            }
            pooledGameObject = CreateNewGameObjectAndReturnDirectly();
            if (pooledGameObject == null)
            {
                Debug.LogError("ObjectPool: Failed to create a new GameObject when inactive pool was empty.");
                return null;
            }
        }
        T component = pooledGameObject.GetComponent<T>();
        if (component == null)
        {
            Debug.LogError($"ObjectPool: Pooled GameObject '{pooledGameObject.name}' does not have the requested component of type '{typeof(T).FullName}'. Returning GameObject to pool.", pooledGameObject);
            pooledGameObject.SetActive(false);
            inactivePool.Push(pooledGameObject); // Add it back to pool since iot was not able to get the component
            return null;
        }

        pooledGameObject.SetActive(true);
        activePool.Add(pooledGameObject);
        return component;
    }

    public void ReturnToPool(GameObject usedGameObject)
    {
        if (usedGameObject == null)
        {
            return;
        }

        if (inactivePool.Contains(usedGameObject)) // O(n)
        {
            return;
        }

        if (activePool.Remove(usedGameObject)) // O(n)
        {
            usedGameObject.SetActive(false);
            inactivePool.Push(usedGameObject); // O(1)
        }
        else
        {
            Debug.LogWarning($"ObjectPool: GameObject '{usedGameObject.name}' not found in active pool. Cannot return. It might have been destroyed or returned already.", usedGameObject);
        }
    }

    public void ClearPool()
    {
        while (inactivePool.Count > 0)
        {
            GameObject obj = inactivePool.Pop();
            if (obj != null) Destroy(obj);
        }

        foreach (GameObject obj in activePool)
        {
            if (obj != null) Destroy(obj);
        }
        activePool.Clear();
    }

    private void CreateAndStoreGameObject()
    {
        if (objectPrefab == null) return;
        GameObject newGameObject = Instantiate(objectPrefab, poolParent);
        if (newGameObject == null) // Instantiate can fail
        {
            Debug.LogError("ObjectPool: GameObject.Instantiate failed for prefab.", objectPrefab);
            return;
        }
        newGameObject.SetActive(false);
        newGameObject.name = $"{objectPrefab.name}_PoolObj_{TotalCount}";
        inactivePool.Push(newGameObject);
    }

    private GameObject CreateNewGameObjectAndReturnDirectly()
    {
        if (objectPrefab == null) return null;
        GameObject newGameObject = Instantiate(objectPrefab, poolParent);
        if (newGameObject == null)
        {
            Debug.LogError("ObjectPool: GameObject.Instantiate failed for prefab during on-demand creation.", objectPrefab);
            return null;
        }
        newGameObject.SetActive(false);
        newGameObject.name = $"{objectPrefab.name}_PoolObj_Active_{ActiveCount}"; // Differentiate name slightly
        return newGameObject;
    }

    private bool CanCreateObject()
    {
        if (objectPrefab == null) return false;
        int currentTotal = TotalCount;
        return currentTotal < maxTotalCapacity;
    }
}