using UnityEditor;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>Changes to the state list. The page shows main states lowest priority first, so the catalog's start is the page's end.</summary>
    public static class StateListEdits
    {
        /// <summary>Adds a state at the page's end (the lowest priority). A listed state is left where it is.</summary>
        public static void Add(GameStateCatalog _catalog, StateSO _state)
        {
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
            Undo.RecordObject(_catalog, "Remove state from list");
            _catalog.States.RemoveAt(_index);
            Save(_catalog);
        }

        /// <summary>Puts the state at one catalog position into another.</summary>
        public static void Move(GameStateCatalog _catalog, int _from, int _to)
        {
            if (_from == _to)
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
            Undo.RecordObject(_catalog, "Change listed state");
            _catalog.States[_index] = _state;
            Save(_catalog);
        }

        private static void Save(GameStateCatalog _catalog)
        {
            EditorUtility.SetDirty(_catalog);
            AssetDatabase.SaveAssetIfDirty(_catalog);
        }
    }
}
