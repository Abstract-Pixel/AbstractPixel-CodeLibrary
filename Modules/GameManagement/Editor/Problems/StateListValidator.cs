using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>The one source of state list problems: the settings page and the build check both read it.</summary>
    internal static class StateListValidator
    {
        public static List<StateProblem> Find(GameStateCatalog _catalog)
        {
            List<StateProblem> problems = new List<StateProblem>();
            if (_catalog == null)
            {
                return problems;
            }

            foreach (int slot in _catalog.FindEmptySlots())
            {
                StateProblem empty = Make(StateProblemKind.EmptySlot, false, null, null,
                    "Rank " + StateListEdits.PageRank(_catalog, slot) + " in the state list is empty. It does nothing.", "Remove slot", "Removes the empty slot from the list");
                empty.Slot = slot;
                problems.Add(empty);
            }

            foreach (StateSO state in _catalog.FindDuplicates())
            {
                problems.Add(Duplicate(state));
            }

            HashSet<StateSO> inReportedLoop = new HashSet<StateSO>();
            foreach (StateSO state in _catalog.States.Distinct())
            {
                if (state == null)
                {
                    continue;
                }
                FindLoop(state, inReportedLoop, problems);
                FindParentSlotProblems(state, problems);
                FindMissingParents(_catalog, state, problems);
            }
            return problems;
        }

        /// <summary>The problems the state's Inspector shows, whether or not the state is in the list.</summary>
        public static List<StateProblem> FindForState(GameStateCatalog _catalog, StateSO _state)
        {
            List<StateProblem> problems = new List<StateProblem>();
            if (_catalog == null || _state == null)
            {
                return problems;
            }
            bool isListed = _catalog.Contains(_state);
            if (!isListed)
            {
                problems.Add(Make(StateProblemKind.NotInList, true, _state, null,
                    "Not in the state list, so every request for it is refused.", "Add to list", "Adds " + _state.name + " to the state list"));
            }
            if (_catalog.FindDuplicates().Contains(_state))
            {
                problems.Add(Duplicate(_state));
            }
            FindLoop(_state, new HashSet<StateSO>(), problems);
            FindParentSlotProblems(_state, problems);
            if (isListed)
            {
                FindMissingParents(_catalog, _state, problems);
            }
            return problems;
        }

        private static StateProblem Duplicate(StateSO _state)
        {
            return Make(StateProblemKind.Duplicate, false, _state, null,
                _state.name + " is listed twice. Only its higher place counts.", "Remove lower copy",
                "Keeps the place that counts and removes the other copies");
        }

        /// <summary>Applies the problem's fix with Undo, saves, and writes one log line.</summary>
        public static void Fix(GameStateCatalog _catalog, StateProblem _problem)
        {
            Object changed = _catalog;
            if (_problem.Kind == StateProblemKind.ParentLoop || _problem.Kind == StateProblemKind.EmptyParentSlot
                || _problem.Kind == StateProblemKind.SameParentTwice)
            {
                changed = _problem.State;
            }
            Undo.RecordObject(changed, _problem.FixLabel);

            switch (_problem.Kind)
            {
                case StateProblemKind.EmptySlot:
                    _catalog.States.RemoveAt(_problem.Slot);
                    break;
                case StateProblemKind.Duplicate:
                    RemoveLaterCopies(_catalog.States, _problem.State);
                    break;
                case StateProblemKind.ParentLoop:
                    _problem.State.Parents.RemoveAll(_parent => _parent == _problem.Parent);
                    break;
                case StateProblemKind.EmptyParentSlot:
                    _problem.State.Parents.RemoveAll(_parent => _parent == null);
                    break;
                case StateProblemKind.SameParentTwice:
                    RemoveLaterCopies(_problem.State.Parents, _problem.Parent);
                    break;
                case StateProblemKind.NotInList:
                    StateListEdits.Add(_catalog, _problem.State);
                    break;
                case StateProblemKind.MissingParents:
                    foreach (StateSO parent in MissingParents(_catalog, _problem.State))
                    {
                        StateListEdits.Add(_catalog, parent);
                    }
                    break;
            }

            EditorUtility.SetDirty(changed);
            AssetDatabase.SaveAssetIfDirty(changed);
            Debug.Log("[Game management] " + _problem.FixLabel + ": " + _problem.Message, changed);
        }

        private static void RemoveLaterCopies(List<StateSO> _list, StateSO _state)
        {
            int first = _list.IndexOf(_state);
            for (int i = _list.Count - 1; i > first; i--)
            {
                if (_list[i] == _state)
                {
                    _list.RemoveAt(i);
                }
            }
        }

        private static StateProblem Make(StateProblemKind _kind, bool _isError, StateSO _state, StateSO _parent,
            string _message, string _fixLabel, string _fixTooltip)
        {
            return new StateProblem
            {
                Kind = _kind,
                IsError = _isError,
                State = _state,
                Parent = _parent,
                Message = _message,
                FixLabel = _fixLabel,
                FixTooltip = _fixTooltip
            };
        }

        private static void FindLoop(StateSO _state, HashSet<StateSO> _inReportedLoop, List<StateProblem> _problems)
        {
            if (_state.Parents.Contains(_state))
            {
                _problems.Add(Make(StateProblemKind.ParentLoop, true, _state, _state,
                    _state.name + " lists itself as a parent, so it can never open.", "Remove parent",
                    "Removes " + _state.name + " from its own parents"));
                return;
            }
            if (_inReportedLoop.Contains(_state))
            {
                return;
            }
            foreach (StateSO parent in _state.Parents)
            {
                List<StateSO> path = new List<StateSO> { _state };
                if (parent == null || !LeadsBack(parent, _state, new HashSet<StateSO>(), path))
                {
                    continue;
                }
                foreach (StateSO member in path)
                {
                    _inReportedLoop.Add(member);
                }
                string ending = " open over each other in a loop, so none can open first.";
                if (path.Count == 2)
                {
                    ending = " open over each other, so neither can open first.";
                }
                _problems.Add(Make(StateProblemKind.ParentLoop, true, _state, parent, JoinNames(path) + ending, "Remove parent",
                    "Removes " + parent.name + " from the parents of " + _state.name));
                return;
            }
        }

        private static bool LeadsBack(StateSO _from, StateSO _target, HashSet<StateSO> _seen, List<StateSO> _path)
        {
            if (_from == _target)
            {
                return true;
            }
            if (!_seen.Add(_from))
            {
                return false;
            }
            _path.Add(_from);
            foreach (StateSO parent in _from.Parents)
            {
                if (parent != null && parent != _from && LeadsBack(parent, _target, _seen, _path))
                {
                    return true;
                }
            }
            _path.RemoveAt(_path.Count - 1);
            return false;
        }

        private static void FindParentSlotProblems(StateSO _state, List<StateProblem> _problems)
        {
            if (_state.Parents.Count > 0 && _state.Parents.All(_parent => _parent == null))
            {
                // The machine counts any parent slot as "this is a sub-state", so with none filled it can never open.
                _problems.Add(Make(StateProblemKind.EmptyParentSlot, true, _state, null,
                    _state.name + " has only empty parent slots, so it can never open.", "Remove slot",
                    "Removes the empty parent slots, which makes " + _state.name + " a main state"));
            }
            else if (_state.Parents.Any(_parent => _parent == null))
            {
                _problems.Add(Make(StateProblemKind.EmptyParentSlot, false, _state, null,
                    _state.name + " has an empty parent slot. It does nothing.", "Remove slot",
                    "Removes the empty parent slots of " + _state.name));
            }
            foreach (IGrouping<StateSO, StateSO> group in _state.Parents.Where(_parent => _parent != null).GroupBy(_parent => _parent))
            {
                if (group.Count() > 1 && group.Key != _state)
                {
                    _problems.Add(Make(StateProblemKind.SameParentTwice, false, _state, group.Key,
                        _state.name + " lists " + group.Key.name + " as a parent twice.", "Remove copy",
                        "Keeps one " + group.Key.name + " parent on " + _state.name));
                }
            }
        }

        private static List<StateSO> MissingParents(GameStateCatalog _catalog, StateSO _state)
        {
            return _state.Parents.Where(_parent => _parent != null && _parent != _state && !_catalog.Contains(_parent)).Distinct().ToList();
        }

        private static void FindMissingParents(GameStateCatalog _catalog, StateSO _state, List<StateProblem> _problems)
        {
            List<StateSO> missing = MissingParents(_catalog, _state);
            if (missing.Count == 0)
            {
                return;
            }
            List<StateSO> listed = _state.Parents.Where(_parent => _parent != _state && _catalog.Contains(_parent)).Distinct().ToList();
            string names = JoinNames(missing);
            string verb = " aren't";
            string label = "Add missing parents";
            if (missing.Count == 1)
            {
                verb = " isn't";
                label = "Add " + names;
            }
            string message = _state.name + " only opens over " + names + ", which" + verb + " in the state list, so " + _state.name + " can never open.";
            if (listed.Count > 0)
            {
                // Past two, a count reads better than a list of names; the Parents list shows which they are.
                string stillOver = listed.Count > 2 ? "its " + listed.Count + " other parents" : JoinNames(listed);
                message = _state.name + " opens over " + names + ", but " + names + verb + " in the state list. It still opens over " + stillOver + ".";
            }
            _problems.Add(Make(StateProblemKind.MissingParents, listed.Count == 0, _state, missing[0], message, label,
                "Adds " + names + " at the bottom of the main states"));
        }

        private static string JoinNames(List<StateSO> _states)
        {
            if (_states.Count == 1)
            {
                return _states[0].name;
            }
            return string.Join(", ", _states.Take(_states.Count - 1).Select(_state => _state.name)) + " and " + _states[_states.Count - 1].name;
        }
    }
}
