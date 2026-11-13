using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.Core
{
    // The base abstract class remains generic
    public abstract class DictionarySO<Tkey, Tvalue> : ScriptableObject
    {
        // Unity skips a Dictionary field without [SerializeField], even a public one, so the asset would never keep its entries.
        [SerializeField] public Dictionary<Tkey, Tvalue> dictionary = new Dictionary<Tkey, Tvalue>();
    }
}
