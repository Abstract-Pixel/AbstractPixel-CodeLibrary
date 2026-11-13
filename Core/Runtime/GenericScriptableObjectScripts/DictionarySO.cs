using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.Core
{
    // The base abstract class remains generic
    public abstract class DictionarySO<Tkey, Tvalue> : ScriptableObject
    {
        public Dictionary<Tkey, Tvalue> dictionary  = new Dictionary<Tkey, Tvalue>();
    }
}
