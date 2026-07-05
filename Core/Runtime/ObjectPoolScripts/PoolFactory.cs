using UnityEngine;

namespace AbstractPixel.Core
{
    
    [System.Serializable]
    public class PoolFactory<TResult, TData> : ObjectPool<TResult>
        where TResult : Component, IInitializable<TData>
    {
        // Constructor just passes arguments up to your ObjectPool logic
        public PoolFactory(GameObject prefab, int amount, bool canExpand, int maxCapacity, Transform parent = null)
            : base(prefab, amount, canExpand, maxCapacity, parent)
        { }

        // OVERLOAD: This hides/wraps the base pool call, instantly doing data binding!
        public TResult GetFromPool(TData data, Transform newParent = null, Vector3 spawnPosition = default)
        {
            // 1. Let the base ObjectPool do the heavy lifting of fetching memory safely
            TResult instance = base.GetFromPool();

            if (instance == null) return null; 

            if (newParent != null)
            {
                instance.transform.SetParent(newParent, false); // false keeps local rect scale/anchors sane for UI
                instance.transform.position = spawnPosition;
            }

            instance.Initialize(data);

            return instance;
        }
    }
}