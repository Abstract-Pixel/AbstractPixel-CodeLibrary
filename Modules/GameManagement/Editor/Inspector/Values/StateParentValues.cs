using System;
using System.Collections.Generic;
using System.Linq;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>What a sub-state does over each parent, worked out with the game's own resolver on every stack the parent can be on.</summary>
    internal static class StateParentValues
    {
        private const string Varies = "Varies";
        // About what the table's Input column can show whole. Longer lists are counted there and listed under the table.
        private const int LongestMapText = 12;
        // A line longer than this with names in it reads as a wall of text, so it points at parent numbers instead.
        private const int LongestSentence = 60;

        private struct StackResult
        {
            public string Stack;
            public GameEffects Parent;
            public GameEffects Own;
        }

        public static StateEffectCell Cell(StateSO _state, StateSO _parent, StateEffectKind _kind)
        {
            List<StackResult> results = Results(_state, _parent);
            if (!results.All(_result => StateEffectValues.Same(_kind, results[0].Own, _result.Own)))
            {
                return new StateEffectCell
                {
                    Text = Varies,
                    Tooltip = "Depends on what is under " + _parent.name + ":\n" + PerStack(_kind, results, _result => _result.Own)
                };
            }

            StateEffectCell cell = new StateEffectCell
            {
                Text = StateEffectValues.Text(_kind, results[0].Own),
                Tooltip = StateEffectValues.FullText(_kind, results[0].Own)
            };
            cell.IsChanged = IsOwn(_state, _kind) && results.Any(_result => !StateEffectValues.Same(_kind, _result.Parent, _result.Own));
            if (cell.IsChanged)
            {
                bool parentHasOne = results.All(_result => StateEffectValues.Same(_kind, results[0].Parent, _result.Parent));
                string parentValue = parentHasOne
                    ? StateEffectValues.FullText(_kind, results[0].Parent)
                    : PerStack(_kind, results, _result => _result.Parent);
                cell.Tooltip = "Changed by this state.\n\nThis state uses:\n" + cell.Tooltip + "\n\n" + _parent.name + " uses:\n" + parentValue;
            }
            cell.Value = results[0].Own;
            if (_kind == StateEffectKind.Input && cell.Value.InputMaps != null && cell.Text.Length > LongestMapText)
            {
                cell.ListedMaps = StateEffectValues.MapNames(cell.Value.InputMaps).ToList();
                cell.Text = cell.ListedMaps.Count == 1 ? "1 map" : cell.ListedMaps.Count + " maps";
            }
            return cell;
        }

        /// <summary>The muted line under a switch that is not on Own.</summary>
        public static string InheritedLine(StateSO _state, StateEffectKind _kind)
        {
            if (_state.Parents.Count == 0)
            {
                return "Keeps whatever was set before";
            }
            List<StateSO> parents = StateStacks.Parents(_state);
            if (parents.Count == 0)
            {
                return "No parent yet, so there is nothing to follow";
            }

            // One value per parent; null where the parent's own stacks differ.
            List<GameEffects?> values = parents.Select(_parent => ParentValue(_state, _parent, _kind)).ToList();
            string sentence = Sentence(_kind, parents, values, _index => parents[_index].name);
            if (sentence.Split('\n').All(_line => _line.Length <= LongestSentence))
            {
                return sentence;
            }
            return Sentence(_kind, parents, values, _index => (_state.Parents.IndexOf(parents[_index]) + 1).ToString(), true);
        }

        private static string Sentence(StateEffectKind _kind, List<StateSO> _parents, List<GameEffects?> _values,
            Func<int, string> _nameOf, bool _byNumber = false)
        {
            bool allAgree = _values.All(_value => SameParentValue(_kind, _values[0], _value));
            if (allAgree && _values[0].HasValue && SetsNothing(_kind, _values[0].Value))
            {
                return "No parent sets this, so it stays as it was";
            }
            if (allAgree && _values[0].HasValue && !SetsNothing(_kind, _values[0].Value))
            {
                string from = _parents.Count > 1 ? "each parent" : (_byNumber ? "parent " : "") + _nameOf(0);
                return Word(_kind, _values[0]) + ", from " + from;
            }
            if (_parents.Count <= 2 && !_byNumber)
            {
                return string.Join(", ", _parents.Select((_parent, _index) => Word(_kind, _values[_index]) + " over " + _nameOf(_index)));
            }

            // Naming each value once keeps it readable. Grouped by the real value, not by its words.
            List<string> lines = new List<string>();
            List<int> left = Enumerable.Range(0, _parents.Count).ToList();
            while (left.Count > 0)
            {
                GameEffects? value = _values[left[0]];
                List<int> group = left.Where(_index => SameParentValue(_kind, value, _values[_index])).ToList();
                string over = _byNumber ? (group.Count == 1 ? "parent " : "parents ") : "";
                // Where first, then what: "Over Play, Pause: Shown".
                lines.Add("Over " + over + string.Join(", ", group.Select(_nameOf)) + ": " + Word(_kind, value));
                left.RemoveAll(group.Contains);
            }
            return string.Join("\n", lines);
        }

        private static GameEffects? ParentValue(StateSO _state, StateSO _parent, StateEffectKind _kind)
        {
            List<StackResult> results = Results(_state, _parent);
            if (results.All(_result => StateEffectValues.Same(_kind, results[0].Parent, _result.Parent)))
            {
                return results[0].Parent;
            }
            return null;
        }

        private static bool SameParentValue(StateEffectKind _kind, GameEffects? _first, GameEffects? _second)
        {
            if (!_first.HasValue || !_second.HasValue)
            {
                return !_first.HasValue && !_second.HasValue;
            }
            return StateEffectValues.Same(_kind, _first.Value, _second.Value);
        }

        private static bool SetsNothing(StateEffectKind _kind, GameEffects _value)
        {
            return !StateEffectValues.IsSet(_kind, _value);
        }

        private static string Word(StateEffectKind _kind, GameEffects? _value)
        {
            if (!_value.HasValue)
            {
                return Varies;
            }
            return SetsNothing(_kind, _value.Value) ? "stays as it was" : StateEffectValues.Text(_kind, _value.Value);
        }

        /// <summary>The state whose own value the parent uses on every stack. Null when it keeps the last value or the stacks differ.</summary>
        public static StateSO ValueOwner(StateSO _parent, StateEffectKind _kind)
        {
            List<List<StateSO>> stacks = StateStacks.EndingAt(_parent);
            if (stacks.Any(_stack => Owner(_stack, _kind) == null))
            {
                return null;
            }
            GameEffects first = Resolve(stacks[0]);
            return stacks.All(_stack => StateEffectValues.Same(_kind, first, Resolve(_stack))) ? Owner(stacks[0], _kind) : null;
        }

        private static StateSO Owner(List<StateSO> _stack, StateEffectKind _kind)
        {
            return _stack.LastOrDefault(_state => IsOwn(_state, _kind));
        }

        private static bool IsOwn(StateSO _state, StateEffectKind _kind)
        {
            return StateOwnValues.SourceOf(_state.Effects, _kind) == StateEffectSource.Own;
        }

        private static GameEffects Resolve(List<StateSO> _stack)
        {
            return StateEffectResolver.Resolve(_stack, NoStateTimeScale.KeepLast);
        }

        private static List<StackResult> Results(StateSO _state, StateSO _parent)
        {
            List<StackResult> results = new List<StackResult>();
            foreach (List<StateSO> stack in StateStacks.EndingAt(_parent))
            {
                results.Add(new StackResult
                {
                    Stack = string.Join(" > ", stack.Select(_member => _member.name)),
                    Parent = Resolve(stack),
                    Own = Resolve(new List<StateSO>(stack) { _state })
                });
            }
            return results;
        }

        private static string PerStack(StateEffectKind _kind, List<StackResult> _results, Func<StackResult, GameEffects> _valueOf)
        {
            return string.Join("\n", _results.Select(_result => StateEffectValues.Text(_kind, _valueOf(_result)) + " over " + _result.Stack));
        }
    }
}
