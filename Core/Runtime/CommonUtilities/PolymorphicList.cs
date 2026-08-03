using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.Core
{
    [Serializable]
    public class PolymorphicList<T> : IEnumerable<T>, IReadOnlyList<T>
    {
        [SerializeReference]
        public List<T> List = new List<T>();

        public int Count => List != null ? List.Count : 0;

        public T this[int _index]
        {
            get => List[_index];
            set => List[_index] = value;
        }

        public List<T>.Enumerator GetEnumerator()
        {
            if (List == null)
            {
                List = new List<T>();
            }
            return List.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T _item)
        {
            if (List == null)
            {
                List = new List<T>();
            }
            List.Add(_item);
        }

        public bool Remove(T _item)
        {
            return List != null && List.Remove(_item);
        }

        public void Clear()
        {
            List?.Clear();
        }

        public bool Contains(T _item)
        {
            return List != null && List.Contains(_item);
        }

        public int FindIndex(Predicate<T> _match)
        {
            return List != null ? List.FindIndex(_match) : -1;
        }
    }
}