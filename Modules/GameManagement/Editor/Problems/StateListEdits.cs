using UnityEditor;
using UnityEngine;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>Changes to the state list. The page shows main states lowest priority first, so the catalog's start is the page's end.</summary>
    internal static class StateListEdits
    {
        /// <summary>Adds a state at the page's end (the lowest priority). A listed state is left where it is.</summary>
        public static void Add(GameStateCatalog _catalog, StateSO _state)
        {
            if (!CanWrite(_catalog))
            {
                return;
            }
            if (_state == null || _catalog.Contains(_state))
            {
                return;
            }
            Undo.RecordObject(_catalog, "Add state");
            _catalog.States.Insert(0, _state);
            Save(_catalog);
        }

        public static void RemoveAt(GameStateCatalog _catalog, int _index)
        {
            if (!CanWrite(_catalog))
            {
                return;
            }
            Undo.RecordObject(_catalog, "Remove state from list");
            _catalog.States.RemoveAt(_index);
            Save(_catalog);
        }

        /// <summary>Puts the state at one catalog position into another.</summary>
        public static void Move(GameStateCatalog _catalog, int _from, int _to)
        {
            if (!CanWrite(_catalog) || _from == _to)
            {
                return;
            }
            Undo.RecordObject(_catalog, "Change state rank");
            StateSO state = _catalog.States[_from];
            _catalog.States.RemoveAt(_from);
            _catalog.States.Insert(_to, state);
            Save(_catalog);
        }

        public static void Replace(GameStateCatalog _catalog, int _index, StateSO _state)
        {
            if (!CanWrite(_catalog))
            {
                return;
            }
            Undo.RecordObject(_catalog, "Change listed state");
            _catalog.States[_index] = _state;
            Save(_catalog);
        }

        /// <summary>False for a state list inside a package (a sample's): the library's own assets are never written to from a project.</summary>
        public static bool CanWrite(GameStateCatalog _catalog)
        {
            string path = AssetDatabase.GetAssetPath(_catalog);
            if (path != null && path.StartsWith("Packages/"))
            {
                Debug.LogWarning("[Game management] The state list '" + _catalog.name + "' is inside a package, so it is read-only. Create settings of your own in Project Settings > Abstract Pixel > Game Management first.", _catalog);
                return false;
            }
            return true;
        }

        /// <summary>The rank number the page shows beside a main state or an empty row. Messages use it, because the
        /// catalog position counts the other way and includes sub-states.</summary>
        public static int PageRank(GameStateCatalog _catalog, int _index)
        {
            int rank = 0;
            for (int i = _catalog.States.Count - 1; i >= _index; i--)
            {
                if (HasRank(_catalog.States[i]))
                {
                    rank++;
                }
            }
            return rank;
        }

        /// <summary>An empty row keeps a rank, so removing a state from its field does not renumber the page under the designer.</summary>
        public static bool HasRank(StateSO _state)
        {
            return _state == null || _state.Parents.Count == 0;
        }

        private static void Save(GameStateCatalog _catalog)
        {
            EditorUtility.SetDirty(_catalog);
            AssetDatabase.SaveAssetIfDirty(_catalog);
        }
    }
}
