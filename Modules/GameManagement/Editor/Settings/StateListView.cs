using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>The states card's list: Main states (with their sub-states grouped under each) and Sub-states tabs,
    /// the drop box, and the + / − footer. It remembers the open tab, folded groups and the selected row.</summary>
    internal class StateListView
    {
        private const string NoParentKey = "no listed parent";

        private readonly Action changed;
        private readonly HashSet<object> folded = new HashSet<object>();
        private int openTab;
        private int selectedIndex = -1;
        private GameManagementSettings settings;
        private GameStateCatalog catalog;
        private List<StateProblem> problems;
        private HashSet<StateProblem> shownProblems;
        private Button removeButton;
        private StateRowDrag drag;

        public StateListView(Action _changed)
        {
            changed = _changed;
        }

        public VisualElement Build(GameManagementSettings _settings, List<StateProblem> _problems)
        {
            settings = _settings;
            catalog = _settings.Catalog;
            problems = _problems;
            shownProblems = new HashSet<StateProblem>();
            if (selectedIndex >= catalog.States.Count)
            {
                selectedIndex = -1;
            }
            drag = new StateRowDrag((_from, _to) =>
            {
                selectedIndex = -1;
                StateListEdits.Move(catalog, _from, _to);
                changed();
            });

            VisualElement root = new VisualElement();
            VisualElement bar = new VisualElement();
            bar.AddToClassList("gm-tab-bar");
            bar.Add(TabButton("Main states", "States with no parent. Higher rank wins.", 0));
            bar.Add(TabButton("Sub-states", "States that open over a parent.", 1));
            int rankCount = catalog.States.Count(StateListEdits.HasRank);
            if (rankCount > 1)
            {
                Label hint = new Label("Higher rank wins, so rank " + rankCount + " beats rank 1.");
                hint.tooltip = hint.text;
                hint.AddToClassList("gm-tab-hint");
                bar.Add(hint);
            }
            root.Add(bar);

            VisualElement box = new VisualElement();
            box.AddToClassList("gm-drop-box");
            VisualElement rows = new VisualElement();
            rows.AddToClassList("gm-list");
            if (openTab == 0)
            {
                BuildMainRows(rows);
            }
            else
            {
                BuildSubRows(rows);
            }
            box.Add(rows);
            box.Add(Footer());
            StateListDropBox.Add(box, _dropped =>
            {
                foreach (StateSO state in _dropped)
                {
                    StateListEdits.Add(catalog, state);
                }
                changed();
            });
            root.Add(box);
            if (openTab == 1)
            {
                Label how = new Label("Add a sub-state from the row of the main state it opens over.");
                how.AddToClassList("gm-hint");
                root.Add(how);
            }
            return root;
        }

        private static bool IsMain(StateSO _state)
        {
            return _state != null && _state.Parents.Count == 0;
        }

        private Button TabButton(string _text, string _tooltip, int _tab)
        {
            Button tab = new Button(() =>
            {
                openTab = _tab;
                changed();
            });
            tab.text = _text;
            tab.tooltip = _tooltip;
            tab.AddToClassList("gm-tab");
            tab.EnableInClassList("gm-tab-open", openTab == _tab);
            return tab;
        }

        // The page counts up from the lowest priority, so it walks the catalog backwards. Empty slots show here too.
        private void BuildMainRows(VisualElement _rows)
        {
            List<int> subStates = new List<int>();
            for (int i = catalog.States.Count - 1; i >= 0; i--)
            {
                StateSO state = catalog.States[i];
                if (state != null && state.Parents.Count > 0)
                {
                    subStates.Add(i);
                }
            }

            HashSet<int> grouped = new HashSet<int>();
            for (int i = catalog.States.Count - 1; i >= 0; i--)
            {
                StateSO state = catalog.States[i];
                if (state == null)
                {
                    _rows.Add(Line(null, Row(i, StateListEdits.PageRank(catalog, i).ToString(), StateRowKind.Empty, false)));
                    continue;
                }
                if (state.Parents.Count > 0)
                {
                    continue;
                }
                int rank = StateListEdits.PageRank(catalog, i);
                // A sub-state shows once, under the first main state on the page that it opens over.
                List<int> children = subStates.Where(_index => !grouped.Contains(_index) && catalog.States[_index].Parents.Contains(state)).ToList();
                grouped.UnionWith(children);
                bool open = !folded.Contains(state);
                Toggle arrow = children.Count > 0 ? Arrow(state, open) : null;
                _rows.Add(Line(arrow, Row(i, rank.ToString(), StateRowKind.Main, true)));
                if (children.Count > 0 && open)
                {
                    _rows.Add(Group(children, rank.ToString()));
                }
            }

            List<int> loose = subStates.Where(_index => !grouped.Contains(_index) && !OpensOverListedMain(catalog.States[_index])).ToList();
            if (loose.Count > 0)
            {
                bool open = !folded.Contains(NoParentKey);
                Label header = new Label("Sub-states with no listed parent");
                header.AddToClassList("gm-group-header");
                _rows.Add(Line(Arrow(NoParentKey, open), header));
                if (open)
                {
                    _rows.Add(Group(loose, "?"));
                }
            }
            if (catalog.States.Count == 0)
            {
                Label none = new Label("No states yet.");
                none.AddToClassList("gm-hint");
                none.AddToClassList("gm-empty");
                _rows.Add(none);
            }
        }

        private bool OpensOverListedMain(StateSO _state)
        {
            return _state.Parents.Any(_parent => IsMain(_parent) && catalog.Contains(_parent));
        }

        private Toggle Arrow(object _key, bool _open)
        {
            Foldout foldout = new Foldout();
            Toggle arrow = foldout.Q<Toggle>();
            arrow.SetValueWithoutNotify(_open);
            arrow.AddToClassList(Foldout.toggleUssClassName);
            arrow.RegisterValueChangedCallback(_change =>
            {
                if (_change.newValue)
                {
                    folded.Remove(_key);
                }
                else
                {
                    folded.Add(_key);
                }
                changed();
            });
            return arrow;
        }

        private static VisualElement Line(VisualElement _arrow, VisualElement _content)
        {
            VisualElement line = new VisualElement();
            line.AddToClassList("gm-line");
            VisualElement slot = new VisualElement();
            slot.AddToClassList("gm-arrow-slot");
            if (_arrow != null)
            {
                slot.Add(_arrow);
            }
            line.Add(slot);
            _content.AddToClassList("gm-grow");
            line.Add(_content);
            return line;
        }

        private VisualElement Group(List<int> _indices, string _parentRank)
        {
            VisualElement group = new VisualElement();
            group.AddToClassList("gm-sub-group");
            Label header = new Label("Sub-states");
            header.AddToClassList("gm-sub-header");
            group.Add(header);
            for (int n = 0; n < _indices.Count; n++)
            {
                group.Add(Row(_indices[n], _parentRank + "." + (n + 1), StateRowKind.Sub, true));
            }
            return Line(null, group);
        }

        private void BuildSubRows(VisualElement _rows)
        {
            int number = 0;
            for (int i = catalog.States.Count - 1; i >= 0; i--)
            {
                StateSO state = catalog.States[i];
                if (state != null && state.Parents.Count > 0)
                {
                    number++;
                    _rows.Add(Row(i, number.ToString(), StateRowKind.Sub, true));
                }
            }
            if (number == 0)
            {
                Label none = new Label("No sub-states yet.");
                none.AddToClassList("gm-hint");
                none.AddToClassList("gm-empty");
                _rows.Add(none);
            }
        }

        private StateRowView Row(int _index, string _badge, StateRowKind _kind, bool _canDrag)
        {
            StateRowView row = new StateRowView(catalog, _index, _badge, _kind);
            row.StateChanged += _state =>
            {
                StateListEdits.Replace(catalog, _index, _state);
                changed();
            };
            row.AddSubStateClicked += _parent => NewStateMenu.Create(settings, _parent, changed);
            row.RegisterCallback<PointerDownEvent>(_event => Select(row, _index), TrickleDown.TrickleDown);
            row.EnableInClassList("gm-selected", selectedIndex == _index);
            if (_canDrag)
            {
                drag.Register(row, _index);
            }
            ShowProblems(row, _index);
            return row;
        }

        // Each problem shows once, under the first row it belongs to.
        private void ShowProblems(StateRowView _row, int _index)
        {
            StateSO state = catalog.States[_index];
            foreach (StateProblem problem in problems)
            {
                bool belongs = state == null ? problem.Slot == _index : problem.State == state;
                if (belongs && shownProblems.Add(problem))
                {
                    _row.AddProblem(problem, () =>
                    {
                        selectedIndex = -1;
                        StateListValidator.Fix(catalog, problem);
                        changed();
                    });
                }
            }
        }

        private void Select(VisualElement _row, int _index)
        {
            VisualElement list = _row.GetFirstAncestorOfType<ScrollView>();
            list?.Query(className: "gm-selected").ForEach(_other => _other.RemoveFromClassList("gm-selected"));
            _row.AddToClassList("gm-selected");
            selectedIndex = _index;
            removeButton.SetEnabled(true);
        }

        private VisualElement Footer()
        {
            VisualElement footer = new VisualElement();
            footer.AddToClassList("gm-list-footer");
            Label hint = new Label("Drag State assets here to add them");
            hint.tooltip = hint.text;
            hint.AddToClassList("gm-drop-hint");
            footer.Add(hint);

            Button add = new Button();
            add.text = "+";
            add.tooltip = "Add a state to the list";
            add.AddToClassList("gm-small-button");
            add.clicked += () => NewStateMenu.Show(settings, add.worldBound, changed);
            footer.Add(add);

            removeButton = new Button(() =>
            {
                int index = selectedIndex;
                selectedIndex = -1;
                StateListEdits.RemoveAt(catalog, index);
                changed();
            });
            removeButton.text = "−";
            removeButton.tooltip = "Remove the selected state from the list. The asset is kept. Click a row to select it.";
            removeButton.AddToClassList("gm-small-button");
            removeButton.SetEnabled(selectedIndex >= 0);
            footer.Add(removeButton);
            return footer;
        }
    }
}
