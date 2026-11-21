using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// Sorts objects in Hierarchy order: by the scene's place in the loaded scenes, then by sibling order from the root
    /// down, so a parent comes before its children. Spawn point positions and relocatables both fill in this order.
    /// Used once per Relocate, never per frame.
    /// </summary>
    internal static class HierarchyOrder
    {
        public static void Sort<T>(List<T> _items, Func<T, Transform> _transformOf)
        {
            List<KeyValuePair<List<int>, T>> keyed = new List<KeyValuePair<List<int>, T>>(_items.Count);
            foreach (T item in _items)
            {
                List<int> path = PathOf(_transformOf(item));
                keyed.Add(new KeyValuePair<List<int>, T>(path, item));
            }

            keyed.Sort((_first, _second) => Compare(_first.Key, _second.Key));

            _items.Clear();
            foreach (KeyValuePair<List<int>, T> pair in keyed)
            {
                _items.Add(pair.Value);
            }
        }

        // The scene's index among loaded scenes, then each sibling index from the root down to the object.
        private static List<int> PathOf(Transform _transform)
        {
            List<int> path = new List<int>();
            Transform current = _transform;
            while (current != null)
            {
                path.Add(current.GetSiblingIndex());
                current = current.parent;
            }
            path.Add(SceneIndexOf(_transform.gameObject.scene));
            path.Reverse();
            return path;
        }

        private static int SceneIndexOf(Scene _scene)
        {
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                if (SceneManager.GetSceneAt(index) == _scene)
                {
                    return index;
                }
            }
            return SceneManager.sceneCount;
        }

        private static int Compare(List<int> _first, List<int> _second)
        {
            int shorter = Math.Min(_first.Count, _second.Count);
            for (int index = 0; index < shorter; index++)
            {
                int difference = _first[index].CompareTo(_second[index]);
                if (difference != 0)
                {
                    return difference;
                }
            }
            return _first.Count.CompareTo(_second.Count);
        }
    }
}
