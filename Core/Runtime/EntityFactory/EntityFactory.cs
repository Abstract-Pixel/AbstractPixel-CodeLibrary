using System.Collections.Generic;
using UnityEngine;
using AbstractPixel.Core;

public class EntityFactory<TData, TResult> : MonoSingleton<EntityFactory<TData, TResult>>
    where TData : class
    where TResult : MonoBehaviour, IInitializable<TData>
{

    [Header("Configuration")]
    [SerializeField] private GameObject prefab;

    public TResult Create(TData _providedData, Vector3 _spawnPosition = default, Transform _parentTransform = null)
    {
        if (_providedData == null)
        {
            Debug.LogError($"{name}: Data provided is null.");
            return null;
        }

        GameObject newInstance = Instantiate(prefab, _parentTransform);
        newInstance.transform.localPosition = _spawnPosition;

        TResult component = newInstance.GetComponent<TResult>();
        if (component == null)
        {
            // This should ideally not happen if the prefab is set up correctly
            Debug.LogError($"{name}: Prefab is missing the required component {typeof(TResult)}.");
            Destroy(newInstance);
            return null;
        }
        component.Initialize(_providedData);
        return component;
    }


    public List<TResult> CreateMultipleCards(IEnumerable<TData> _allDataProvided, Transform _parentTransform = null, Vector3 _spawnPosition = default)
    {
        List<TResult> results = new List<TResult>();
        foreach (var data in _allDataProvided)
        {
            results.Add(Create(data, Vector3.zero, _parentTransform));
        }
        return results;
    }
}