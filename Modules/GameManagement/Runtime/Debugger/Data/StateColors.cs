using System.Collections.Generic;

namespace AbstractPixel.GameManagement.Debugging
{
    /// <summary>Which of the six debugger colours a state gets. The overlay and the debugger window both ask here.</summary>
    /// <remarks>The colour follows the state's place in the catalog, so six listed states always differ and a build matches
    /// the Editor. Reordering the list moves the colours with it.</remarks>
    internal static class StateColors
    {
        public const int Count = 6;
        public const int None = -1;

        public static int IndexFor(StateSO _state, GameStateCatalog _catalog)
        {
            if (_state == null)
            {
                return None;
            }

            int place = PlaceInCatalog(_state, _catalog);
            if (place >= 0)
            {
                return place % Count;
            }
            return (int)(StableHash(_state.name) % Count);
        }

        private static int PlaceInCatalog(StateSO _state, GameStateCatalog _catalog)
        {
            if (_catalog == null)
            {
                return -1;
            }

            // Empty slots and repeats take no place, or two of six states could share a colour.
            HashSet<StateSO> seen = new HashSet<StateSO>();
            foreach (StateSO listed in _catalog.States)
            {
                if (listed == _state)
                {
                    return seen.Count;
                }
                if (listed != null)
                {
                    seen.Add(listed);
                }
            }
            return -1;
        }

        // string.GetHashCode may differ between runtimes, so a state outside the catalog uses FNV-1a.
        private static uint StableHash(string _text)
        {
            uint hash = 2166136261;
            foreach (char letter in _text)
            {
                hash = unchecked((hash ^ letter) * 16777619);
            }
            return hash;
        }
    }
}
