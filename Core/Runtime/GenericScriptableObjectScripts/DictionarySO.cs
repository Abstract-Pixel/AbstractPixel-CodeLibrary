using CustomInspector;
using UnityEngine;

namespace AbstractPixel.Core
{
    public class DictionarySO<Tkey, Tvalue> : ScriptableObject
    {
        [field:SerializeField] public ReorderableDictionary<Tkey, Tvalue> Dictionary { get; private set; }

    }
}
