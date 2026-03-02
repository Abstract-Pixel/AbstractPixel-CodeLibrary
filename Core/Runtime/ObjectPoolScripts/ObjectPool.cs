using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] // Added for serialization
public class ObjectPool<Type> where Type : Component
{
    [SerializeField] private GameObject objectPrefab;
    [SerializeField] private int minToSpawn;
    [SerializeField] private bool canExpand;
    [SerializeField] private Transform parent;
    [SerializeField] private int maxToSpawn;

    [SerializeField] private Stack<Type> inactivePool = new Stack<Type>();
    [SerializeField] private List<Type> activePool = new List<Type>();

    public int AvailableCount => inactivePool.Count;
    public int ActiveCount => activePool.Count;
    public int TotalCount => inactivePool.Count + activePool.Count;

    public Action OnObjectRecievedFromPool;

    public ObjectPool(GameObject _objectPrefab, int _amountToSpawn, bool _canExpand, int _maxCapacity, Transform _parent = null)
    {
        if (_objectPrefab == null)
        {
            Debug.LogError($"ObjectPool<{typeof(Type).Name}>: Prefab cannot be null.");
            return;
        }
        objectPrefab = _objectPrefab; 
        minToSpawn = Mathf.Max(0, _amountToSpawn);
        canExpand = _canExpand;

        if (!_canExpand)
        {
           maxToSpawn = minToSpawn;
        }
        else
        {
            maxToSpawn = (_maxCapacity <= 0) ? int.MaxValue : Mathf.Max(minToSpawn, _maxCapacity);
        }

        activePool = new List<Type>(); 
        inactivePool = new Stack<Type>();
        parent = _parent;
    }

    // Default constructor for serialization - Unity needs this for [System.Serializable] classes if they have other constructors.
    // However, for generic classes, this can be tricky. If you create an instance of ObjectPool<T>
    // as a field in a MonoBehaviour, Unity will handle its serialization.
    // If values are set in Inspector, they should persist.
    // If you initialize via the parameterized constructor in Awake/Start, those values will take precedence at runtime.
    public ObjectPool() { }


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
                CreateObjectAndPlaceInInactivePool();
            }
        }
    }

    public virtual Type GetFromPool()
    {
        Type objectFromPool = null;

        if (inactivePool.Count > 0)
        {
            objectFromPool = inactivePool.Peek();
            if (objectFromPool == null)
            {
                Debug.LogError("ObjectPool: Peeked a null object from the inactive pool. Removing it.");
                inactivePool.Pop();
            }
            else
            {
                inactivePool.Pop();
            }
        }

        if (objectFromPool == null)
        {
            if (!CanCreateObject())
            {
                return null;
            }
            objectFromPool = CreateObjectAndReturnDirectly();
            if (objectFromPool == null)
            {
                Debug.LogError("ObjectPool: Failed to create a new object when inactive pool was empty.");
                return null;
            }
        }

        if (objectFromPool == null)
        {
            Debug.LogError("ObjectPool: No objects available in the pool and cannot create new ones (unexpected).");
            return null;
        }

        objectFromPool.gameObject.SetActive(true);
        activePool.Add(objectFromPool);
        OnObjectRecievedFromPool?.Invoke();
        return objectFromPool;
    }

    public void ReturnToPool(Type usedObject)
    {
        if (usedObject == null) return;

        if (inactivePool.Contains(usedObject))
        {
            return;
        }

        if (activePool.Remove(usedObject))
        {
            inactivePool.Push(usedObject);
            usedObject.gameObject.SetActive(false);
        }
    }

    public void ClearPool()
    {
        List<Type> objectsToDestroy = new List<Type>();
        objectsToDestroy.AddRange(new List<Type>(inactivePool));
        objectsToDestroy.AddRange(activePool);

        foreach (Type obj in objectsToDestroy)
        {
            if (obj != null && obj.gameObject != null)
            {
                GameObject.Destroy(obj.gameObject);
            }
        }
        inactivePool.Clear();
        activePool.Clear();
    }

    private void CreateObjectAndPlaceInInactivePool()
    {
        if (objectPrefab == null)
        {
            Debug.LogError("ObjectPool: objectPrefab is null. Cannot create object.");
            return;
        }
        GameObject newCreatedObject = GameObject.Instantiate(objectPrefab, parent);
        Type desiredType = newCreatedObject.GetComponent<Type>();
        if (desiredType == null)
        {
            Debug.LogError($"ObjectPool<{typeof(Type).Name}>: Prefab does not contain component {typeof(Type).FullName}. Destroying object.");
            GameObject.Destroy(newCreatedObject);
            return;
        }

        inactivePool.Push(desiredType);
        newCreatedObject.gameObject.SetActive(false);
        newCreatedObject.gameObject.name = $"{typeof(Type).FullName}_PoolObj_{TotalCount}";
    }

    private Type CreateObjectAndReturnDirectly()
    {
        if (objectPrefab == null) { Debug.LogError("ObjectPool: objectPrefab is null."); return null; }
        GameObject newCreatedObject = GameObject.Instantiate(objectPrefab, parent);
        Type desiredType = newCreatedObject.GetComponent<Type>();
        if (desiredType == null)
        {
            Debug.LogError($"ObjectPool<{typeof(Type).Name}>: Prefab does not contain component {typeof(Type).FullName}. Destroying object.");
            GameObject.Destroy(newCreatedObject);
            return null;
        }
        newCreatedObject.gameObject.SetActive(false);
        newCreatedObject.gameObject.name = $"{typeof(Type).FullName}_PoolObj_Active_{ActiveCount}";
        return desiredType;
    }

    private bool CanCreateObject()
    {
        if (objectPrefab == null) return false;
        int currentTotal = TotalCount;

        if (!this.canExpand)
        {
            return currentTotal < this.maxToSpawn;
        }

        return currentTotal < this.maxToSpawn;
    }
}