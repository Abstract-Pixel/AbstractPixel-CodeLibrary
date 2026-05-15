using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.Core
{
    public class EntityFactory<TData, TResult> : MonoSingleton<EntityFactory<TData, TResult>> , IFactory<TData, TResult>
        where TData : class
        where TResult : MonoBehaviour, IInitializable<TData>
    {

        [Header("Configuration")]
        [SerializeField] private TResult prefab;


        public TResult Create(TResult customPrefab, TData _providedData, Vector3 _spawnPosition = default, Transform _parentTransform = null)
        {
            if (_providedData == null)
            {
                Debug.LogError($"{name}: Data provided is null.");
                return null;
            }
            TResult newInstance = Instantiate(customPrefab, _parentTransform);
            newInstance.transform.localPosition = _spawnPosition;
            newInstance.Initialize(_providedData);
            return newInstance;
        }

        public TResult Create(TData _providedData, Vector3 _spawnPosition = default, Transform _parentTransform = null)
        {
            TResult instance = Create(prefab, _providedData, _spawnPosition, _parentTransform);
            return instance;
        }

        public List<TResult> CreateMultiple(TResult customPrefab, IEnumerable<TData> _allDataProvided, Transform _parentTransform = null, Vector3 _spawnPosition = default)
        {
            List<TResult> results = new List<TResult>();
            foreach (var data in _allDataProvided)
            {
                results.Add(Create(customPrefab, data, _spawnPosition, _parentTransform));
            }
            return results;
        }
        public List<TResult> CreateMultiple(IEnumerable<TData> _allDataProvided, Transform _parentTransform = null, Vector3 _spawnPosition = default)
        {
            List<TResult> results =  CreateMultiple(prefab,_allDataProvided,_parentTransform,_spawnPosition);
            return results;
        }
    }
}