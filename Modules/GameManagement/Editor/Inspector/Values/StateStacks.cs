using System.Collections.Generic;
using System.Linq;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>The stacks a state can be on top of, read from the parents. The machine opens a sub-state only while one of its parents is on top.</summary>
    internal static class StateStacks
    {
        /// <summary>Every stack with this state on top, bottom first. A parent loop gives the state alone.</summary>
        public static List<List<StateSO>> EndingAt(StateSO _state)
        {
            List<List<StateSO>> stacks = new List<List<StateSO>>();
            Walk(_state, new List<StateSO>(), stacks);
            if (stacks.Count == 0)
            {
                stacks.Add(new List<StateSO> { _state });
            }
            return stacks;
        }

        /// <summary>The state's parents without empty slots and repeats, in list order.</summary>
        public static List<StateSO> Parents(StateSO _state)
        {
            return _state.Parents.Where(_parent => _parent != null).Distinct().ToList();
        }

        private static void Walk(StateSO _state, List<StateSO> _fromTop, List<List<StateSO>> _stacks)
        {
            _fromTop.Add(_state);
            List<StateSO> parents = Parents(_state);
            if (parents.Count == 0)
            {
                List<StateSO> stack = new List<StateSO>(_fromTop);
                stack.Reverse();
                _stacks.Add(stack);
            }
            foreach (StateSO parent in parents)
            {
                if (!_fromTop.Contains(parent))
                {
                    Walk(parent, _fromTop, _stacks);
                }
            }
            _fromTop.RemoveAt(_fromTop.Count - 1);
        }
    }
}
