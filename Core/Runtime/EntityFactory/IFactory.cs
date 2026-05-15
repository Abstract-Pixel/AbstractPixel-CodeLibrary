using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.Core
{
    public interface IFactory<TData, TResult>
    where TData : class
    where TResult : MonoBehaviour, IInitializable<TData>
    {
        // Standard Methods
        TResult Create(TData providedData, Vector3 spawnPosition = default, Transform parentTransform = null);
        List<TResult> CreateMultiple(IEnumerable<TData> allDataProvided, Transform parentTransform = null, Vector3 spawnPosition = default);

        // Advanced Methods (The Overloads)
        TResult Create(TResult customPrefab, TData providedData, Vector3 spawnPosition = default, Transform parentTransform = null);
        List<TResult> CreateMultiple(TResult customPrefab, IEnumerable<TData> allDataProvided, Transform parentTransform = null, Vector3 spawnPosition = default);
    }
}
