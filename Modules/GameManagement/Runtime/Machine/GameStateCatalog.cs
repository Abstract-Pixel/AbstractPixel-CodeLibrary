using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>The ordered list of every state. Higher in the list wins; ties cannot exist.</summary>
    [CreateAssetMenu(fileName = "GameStateCatalog", menuName = "Abstract Pixel/Game Management/State Catalog")]
    public class GameStateCatalog : ScriptableObject
    {
        [Tooltip("Every state of the game, top to bottom. A state higher in the list wins over the ones below it.")]
        public List<StateSO> States = new List<StateSO>();

        public bool Contains(StateSO _state)
        {
            return _state != null && States.Contains(_state);
        }

        /// <summary>True when the first state is strictly higher in the list than the second.</summary>
        public bool Outranks(StateSO _first, StateSO _second)
        {
            return States.IndexOf(_first) < States.IndexOf(_second);
        }

        // Helpers for the catalog validator (Session 6). Each returns an empty list when all is fine.

        /// <summary>Positions in the list that hold no state.</summary>
        public List<int> FindEmptySlots()
        {
            List<int> empty = new List<int>();
            for (int i = 0; i < States.Count; i++)
            {
                if (States[i] == null)
                {
                    empty.Add(i);
                }
            }
            return empty;
        }

        /// <summary>States listed more than once (each named once).</summary>
        public List<StateSO> FindDuplicates()
        {
            HashSet<StateSO> seen = new HashSet<StateSO>();
            List<StateSO> duplicates = new List<StateSO>();
            foreach (StateSO state in States)
            {
                if (state != null && !seen.Add(state) && !duplicates.Contains(state))
                {
                    duplicates.Add(state);
                }
            }
            return duplicates;
        }

        /// <summary>Parents of listed sub-states that are not in the catalog, so the sub-state can never open over them.</summary>
        public List<(StateSO subState, StateSO parent)> FindParentsMissingFromCatalog()
        {
            List<(StateSO, StateSO)> missing = new List<(StateSO, StateSO)>();
            foreach (StateSO state in States)
            {
                if (state == null)
                {
                    continue;
                }
                foreach (StateSO parent in state.Parents)
                {
                    if (!Contains(parent))
                    {
                        missing.Add((state, parent));
                    }
                }
            }
            return missing;
        }
    }
}
