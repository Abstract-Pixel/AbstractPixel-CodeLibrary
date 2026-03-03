using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.Core
{
    public abstract class ListSO<T> : ScriptableObject
    {
        public List<T> Items = new List<T>();
    }
}