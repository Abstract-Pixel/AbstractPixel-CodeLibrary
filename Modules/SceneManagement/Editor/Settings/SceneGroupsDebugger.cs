using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The Scene System page's Scene Groups Debugger: one row per scene group in the project with its name (click to
    /// select it), its main scene, its scene set, its problems with their fixes, and Open group. Edits go through the
    /// same code as the inspector, with Undo, so overrides, the save-time fix and rename tracking still apply.
    /// </summary>
    public static class SceneGroupsDebugger
    {
        public const string Title = "Scene Groups Debugger";
        public const string Tooltip = "Every scene group in the project in one place: change its main scene or scene set, see its problems and fix them, and choose whether a build checks them first. Changes have Undo.";

        // Columns start at their width and share the spare width in proportion to their grow, up to their maximum; in a
        // narrow window they give way down to their minimum. Header and rows use the same numbers, so they line up.
        private const float ColumnGap = 14;
        private const float IndexWidth = 30;
        private const float NameWidth = 170;
        private const float NameMinWidth = 60;
        private const float NameMaxWidth = 280;
        private const float MainSceneWidth = 220;
        private const float MainSceneMinWidth = 60;
        private const float MainSceneMaxWidth = 420;
        private const float SetWidth = 220;
        private const float SetMinWidth = 200;
        private const float SetMaxWidth = 320;
        private const float StatusWidth = 170;
        private const float StatusMinWidth = 90;
        private const float StatusMaxWidth = 210;
        private const float StatusGrow = 0.5f;
        private const float ButtonGap = 6;
        // Below this card width the two buttons move to a second line under their row, so nothing leaves the card.
        private const float CompactBelowWidth = 1100;
        private const string BlockName = "group-block";
        private const string RowName = "group-row";
        private const string ButtonsName = "row-buttons";
        private const string HeaderButtonsName = "header-buttons";
        private const float ButtonWidth = 92;
        private const float ProblemsIndent = IndexWidth + ColumnGap;

        /// <summary>
        /// The scene groups this list shows, in its order: every game scene group (sample scene groups have their own
        /// section on the page). A group's number is its place here plus one; the scene picker shows the same number.
        /// </summary>
        public static List<SceneGroup> ListedGroups()
        {
            List<SceneGroup> groups = SceneGroupAssets.FindAll();
            groups.RemoveAll(_group => _group == null || _group.IsSample);
            return groups;
        }

        /// <summary>
        /// The group's main scene asset for the main scene field. A scene reference whose asset link did not load (found
        /// in Rally Shift on 2026-11-23: the level groups' links were empty in memory although their files hold them)
        /// is found again by its GUID, then its path, as the group check and start-up find it.
        /// </summary>
        public static SceneAsset MainSceneAssetOf(SceneGroup _group)
        {
            if (_group == null || _group.MainScene == null)
            {
                return null;
            }
            SceneAsset linked = _group.MainScene.SceneFieldReference.SceneAssetInEditor;
            if (linked != null)
            {
                return linked;
            }
            SceneAsset byGuid = LoadSceneByGuid(_group.MainScene.SceneGuid);
            if (byGuid != null)
            {
                return byGuid;
            }
            string path = _group.MainScene.ScenePath;
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            return AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
        }

        private static SceneAsset LoadSceneByGuid(string _guid)
        {
            if (string.IsNullOrEmpty(_guid))
            {
                return null;
            }
            string path = AssetDatabase.GUIDToAssetPath(_guid);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            return AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
        }

        /// <summary>The block (row and its problems) of the group with this asset GUID in a filled card, or null.</summary>
        public static VisualElement FindGroupBlock(VisualElement _card, string _groupGuid)
        {
            if (_card == null || string.IsNullOrEmpty(_groupGuid))
            {
                return null;
            }
            foreach (VisualElement child in _card.Children())
            {
                bool isThatGroup = child.name == BlockName && (child.userData as string) == _groupGuid;
                if (isThatGroup)
                {
                    return child;
                }
            }
            return null;
        }

        /// <summary>Fills the section's card and the summary next to its title ("16 groups, 0 problems").</summary>
        public static void Fill(VisualElement _card, VisualElement _summary, SceneSystemSettings _settings, Action _requestRefresh)
        {
            _card.Clear();
            _card.style.overflow = Overflow.Hidden;
            _card.userData = null;
            _card.UnregisterCallback<GeometryChangedEvent>(OnCardResized);
            _card.RegisterCallback<GeometryChangedEvent>(OnCardResized);
            if (_settings != null)
            {
                _card.Add(BeforeBuildToggle(_settings));
            }
            List<SceneGroup> groups = ListedGroups();
            SceneGroupMigrationPlan plan = SceneGroupMigration.Plan(groups);
            if (_settings != null && plan.HasWork)
            {
                _card.Add(MigrationBox(_settings, plan));
            }

            if (groups.Count == 0)
            {
                _card.Add(SceneSystemUI.Hint("No scene groups yet. Right-click a scene in the Project window and pick Create Scene Group."));
            }
            else
            {
                _card.Add(HeaderRow());
            }

            int problemCount = 0;
            int number = 1;
            foreach (SceneGroup group in groups)
            {
                problemCount += AddGroupRow(_card, group, number, _settings, _requestRefresh);
                number++;
            }
            FillSummary(_summary, groups.Count, problemCount);
        }

        private static void OnCardResized(GeometryChangedEvent _event)
        {
            VisualElement card = _event.currentTarget as VisualElement;
            if (card == null)
            {
                return;
            }
            bool compact = card.contentRect.width < CompactBelowWidth;
            bool alreadyInThatMode = card.userData is bool && (bool)card.userData == compact;
            if (alreadyInThatMode)
            {
                return;
            }
            card.userData = compact;
            ApplyLayout(card, compact);
        }

        // Wide: the buttons end each row. Compact: they move under the row, below the scene group's name.
        private static void ApplyLayout(VisualElement _card, bool _compact)
        {
            VisualElement headerButtons = _card.Q(HeaderButtonsName);
            if (headerButtons != null)
            {
                headerButtons.style.display = _compact ? DisplayStyle.None : DisplayStyle.Flex;
            }
            foreach (VisualElement block in _card.Query(BlockName).ToList())
            {
                VisualElement row = block.Q(RowName);
                VisualElement buttons = block.Q(ButtonsName);
                if (row == null || buttons == null)
                {
                    continue;
                }
                buttons.RemoveFromHierarchy();
                if (_compact)
                {
                    buttons.style.marginLeft = ProblemsIndent;
                    buttons.style.marginTop = 2;
                    buttons.style.marginBottom = 4;
                    block.Insert(1, buttons);
                    continue;
                }
                buttons.style.marginLeft = 0;
                buttons.style.marginTop = 0;
                buttons.style.marginBottom = 0;
                row.Add(buttons);
            }
        }

        private static Toggle BeforeBuildToggle(SceneSystemSettings _settings)
        {
            Toggle beforeBuild = new Toggle("Check scene groups before a build");
            beforeBuild.value = _settings.CheckSceneGroupsBeforeBuild;
            beforeBuild.tooltip = "A build stops when any scene group below has a problem, a scene listed twice included, with a message naming each group and scene. Saved in the settings asset, so it is shared through version control.";
            beforeBuild.style.marginLeft = 0;
            beforeBuild.style.marginBottom = 8;
            beforeBuild.RegisterValueChangedCallback(_change =>
            {
                SetCheckBeforeBuild(_settings, _change.newValue);
            });
            return beforeBuild;
        }

        internal static void SetCheckBeforeBuild(SceneSystemSettings _settings, bool _check)
        {
            Undo.RecordObject(_settings, "Check scene groups before a build");
            _settings.CheckSceneGroupsBeforeBuild = _check;
            SceneSystemSettingsAssets.Save(_settings);
        }

        private static void FillSummary(VisualElement _summary, int _groupCount, int _problemCount)
        {
            _summary.Clear();
            Color colour = SceneSystemUI.Green;
            if (_problemCount > 0)
            {
                colour = SceneSystemUI.Red;
            }
            string text = _groupCount + " groups, " + _problemCount + " problems";
            VisualElement status = SceneSystemUI.StatusRow(colour, text, string.Empty);
            status.style.marginRight = 6;
            _summary.Add(status);
        }

        private static VisualElement HeaderRow()
        {
            VisualElement row = SceneSystemUI.Row();
            row.style.marginBottom = 3;
            Label number = HeaderCell("#", IndexWidth, 0, IndexWidth, IndexWidth, "The scene group's number in this list.");
            number.style.unityTextAlign = TextAnchor.MiddleRight;
            row.Add(number);
            row.Add(HeaderCell("Scene group", NameWidth, 1, NameMinWidth, NameMaxWidth, SceneSystemUI.SceneGroupTooltip));
            row.Add(HeaderCell("Main scene", MainSceneWidth, 1.5f, MainSceneMinWidth, MainSceneMaxWidth, SceneSystemUI.MainSceneTooltip));
            row.Add(HeaderCell("Scene set", SetWidth, 1, SetMinWidth, SetMaxWidth, SceneSystemUI.SceneSetTooltip));
            row.Add(HeaderCell("Status", StatusWidth, StatusGrow, StatusMinWidth, StatusMaxWidth, "Whether the scene group check finds problems in this group. Each problem is listed under the row with its fix."));
            VisualElement buttonsColumn = new VisualElement();
            buttonsColumn.name = HeaderButtonsName;
            float buttonsWidth = ButtonWidth * 2 + ButtonGap;
            Column(buttonsColumn, buttonsWidth, 0, buttonsWidth, buttonsWidth);
            buttonsColumn.style.marginRight = 0;
            row.Add(buttonsColumn);
            return row;
        }

        private static Label HeaderCell(string _text, float _width, float _grow, float _minWidth, float _maxWidth, string _tooltip)
        {
            Label cell = new Label(_text);
            cell.tooltip = _tooltip;
            cell.style.unityFontStyleAndWeight = FontStyle.Bold;
            cell.style.whiteSpace = WhiteSpace.NoWrap;
            cell.style.textOverflow = TextOverflow.Ellipsis;
            Column(cell, _width, _grow, _minWidth, _maxWidth);
            return cell;
        }

        private static void Column(VisualElement _cell, float _width, float _grow, float _minWidth, float _maxWidth)
        {
            _cell.style.width = _width;
            _cell.style.flexGrow = _grow;
            _cell.style.flexShrink = 1;
            _cell.style.minWidth = _minWidth;
            _cell.style.maxWidth = _maxWidth;
            _cell.style.overflow = Overflow.Hidden;
            _cell.style.marginLeft = 0;
            _cell.style.marginRight = ColumnGap;
        }

        // Adds the group's row and, under it, its problems with their fixes. Returns the number of problems.
        private static int AddGroupRow(VisualElement _card, SceneGroup _group, int _number, SceneSystemSettings _settings, Action _requestRefresh)
        {
            VisualElement block = new VisualElement();
            block.name = BlockName;
            // The scene picker scrolls to a group's row by its GUID.
            block.userData = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_group));
            block.style.marginTop = 1;
            block.style.marginBottom = 1;

            SceneGroupProblemsView problemsView = new SceneGroupProblemsView(false);
            problemsView.style.marginLeft = ProblemsIndent;
            problemsView.style.marginBottom = 2;
            int problemCount = problemsView.Refresh(_group, _requestRefresh);

            VisualElement row = SceneSystemUI.Row();
            row.name = RowName;
            Label index = SceneSystemUI.Hint(_number + ".");
            index.style.unityTextAlign = TextAnchor.MiddleRight;
            Column(index, IndexWidth, 0, IndexWidth, IndexWidth);
            row.Add(index);

            Label name = SceneSystemUI.AssetLink(_group, _group.name);
            Column(name, NameWidth, 1, NameMinWidth, NameMaxWidth);
            row.Add(name);
            row.Add(MainSceneField(_group, _requestRefresh));
            row.Add(SetField(_group, _settings, _requestRefresh));
            row.Add(StatusCell(_group, problemCount));
            VisualElement buttons = SceneSystemUI.Row();
            buttons.name = ButtonsName;
            buttons.style.flexShrink = 0;
            buttons.Add(RowButton("Select group", "Selects this scene group in the Project window and shows it in the Inspector.", () => SceneSystemUI.PingAndSelect(_group)));
            Button open = RowButton("Open group", SceneSystemUI.OpenGroupTooltip, () => SceneGroupOpener.Open(_group));
            open.style.marginLeft = ButtonGap;
            buttons.Add(open);
            row.Add(buttons);

            block.Add(row);
            block.Add(problemsView);
            _card.Add(block);
            return problemCount;
        }

        private static Button RowButton(string _text, string _tooltip, Action _clicked)
        {
            Button button = SceneSystemUI.Button(_text, _tooltip, _clicked);
            button.style.width = ButtonWidth;
            button.style.flexShrink = 0;
            button.style.marginLeft = 0;
            button.style.marginRight = 0;
            return button;
        }

        // A small object field with the main scene (its path in the tooltip) that takes a new scene by drag or picker.
        private static VisualElement MainSceneField(SceneGroup _group, Action _requestRefresh)
        {
            SceneAsset current = MainSceneAssetOf(_group);
            string path = string.Empty;
            if (_group.MainScene != null)
            {
                path = _group.MainScene.ScenePath;
            }

            ObjectField field = new ObjectField();
            field.objectType = typeof(SceneAsset);
            field.allowSceneObjects = false;
            field.SetValueWithoutNotify(current);
            // A fixed starting width, so an empty field ("None (Scene Asset)", longer text) sizes exactly like a filled
            // one and every row keeps the same column width.
            Column(field, MainSceneWidth, 1.5f, MainSceneMinWidth, MainSceneMaxWidth);
            field.tooltip = SceneSystemUI.MainSceneTooltip;
            if (!string.IsNullOrEmpty(path))
            {
                field.tooltip = path + "\n\n" + SceneSystemUI.MainSceneTooltip;
            }
            field.RegisterValueChangedCallback(_change =>
            {
                if (_change.newValue == _change.previousValue)
                {
                    return;
                }
                SceneGroupMainScene.Set(_group, _change.newValue as SceneAsset);
                _requestRefresh();
            });
            return field;
        }

        private static VisualElement SetField(SceneGroup _group, SceneSystemSettings _settings, Action _requestRefresh)
        {
            if (_settings == null)
            {
                string setName = "—";
                if (_group.SceneSet != null)
                {
                    setName = _group.SceneSet.name;
                }
                Label none = new Label(setName);
                none.tooltip = "Changing the scene set needs the scene system to be set up.";
                Column(none, SetWidth, 1, SetMinWidth, SetMaxWidth);
                return none;
            }
            PopupField<SceneSet> field = SceneSystemUI.SceneSetPopup(null, _settings, _group.SceneSet, _picked =>
            {
                SceneSetSync.PickSet(_group, _picked);
                _requestRefresh();
            });
            Column(field, SetWidth, 1, SetMinWidth, SetMaxWidth);
            return field;
        }

        private static VisualElement StatusCell(SceneGroup _group, int _problemCount)
        {
            bool isBootGroup = SceneGroupCheck.IsBootGroup(_group);
            Color colour = SceneSystemUI.Green;
            string text = "No problems";
            if (_problemCount > 0)
            {
                colour = SceneSystemUI.Red;
                text = _problemCount + " problem(s)";
            }
            else if (isBootGroup)
            {
                colour = SceneSystemUI.Yellow;
                text = "Boot group, unused";
            }
            VisualElement cell = SceneSystemUI.StatusRow(colour, text, text);
            Column(cell, StatusWidth, StatusGrow, StatusMinWidth, StatusMaxWidth);
            Label label = cell.Q<Label>();
            if (label != null)
            {
                // One line in a narrow window; the whole text is in the tooltip.
                label.style.whiteSpace = WhiteSpace.NoWrap;
                label.style.overflow = Overflow.Hidden;
                label.style.textOverflow = TextOverflow.Ellipsis;
            }
            if (isBootGroup)
            {
                cell.tooltip = "A boot group from before scene sets: no main scene and no scene set. It is not a place the player can go.";
            }
            return cell;
        }

        private static VisualElement MigrationBox(SceneSystemSettings _settings, SceneGroupMigrationPlan _plan)
        {
            VisualElement box = new VisualElement();
            box.style.marginBottom = 8;
            string message = _plan.GroupsToMigrateCount + " scene groups have no scene set yet. Organize them into: " + string.Join(", ", _plan.SetNames) + ".";
            box.Add(new HelpBox(message, HelpBoxMessageType.Info));
            VisualElement buttons = SceneSystemUI.Row();
            buttons.Add(SceneSystemUI.Button("Preview migration", "Writes the migration report next to the settings asset without changing anything.", () => RunMigration(_settings, true)));
            buttons.Add(SceneSystemUI.Button("Migrate", "Makes the scene sets and points each scene group at its set. Every group keeps every scene. A report lists each group before and after.", () => RunMigration(_settings, false)));
            box.Add(buttons);
            return box;
        }

        private static void RunMigration(SceneSystemSettings _settings, bool _isPreview)
        {
            string reportPath = SceneGroupMigration.Run(_settings, _isPreview, null, null);
            SceneSystemSettingsAssets.RefreshLists(_settings);
            SceneSystemSettingsAssets.NotifyChanged();
            EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(reportPath));
        }
    }
}
