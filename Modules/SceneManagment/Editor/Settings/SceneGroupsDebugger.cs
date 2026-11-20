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
        public const string Tooltip = "Every scene group in the project in one place: change its main scene or scene set, see its problems and fix them. Changes have Undo.";

        private const float NameWidth = 150;
        private const float MainSceneMinWidth = 120;
        private const float SetWidth = 140;
        private const float StatusWidth = 150;
        private const float OpenWidth = 84;
        private const string ObjectFieldLabelClass = "unity-object-field-display__label";

        /// <summary>Fills the section's card and the summary next to its title ("16 groups, 0 problems").</summary>
        public static void Fill(VisualElement _card, VisualElement _summary, SceneSystemSettings _settings, Action _requestRefresh)
        {
            _card.Clear();
            List<SceneGroup> groups = SceneGroupAssets.FindAll();
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
            foreach (SceneGroup group in groups)
            {
                problemCount += AddGroupRow(_card, group, _settings, _requestRefresh);
            }
            FillSummary(_summary, groups.Count, problemCount);
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
            row.Add(HeaderCell("Scene group", NameWidth, SceneSystemUI.SceneGroupTooltip));
            Label mainScene = HeaderCell("Main scene", MainSceneMinWidth, SceneSystemUI.MainSceneTooltip);
            mainScene.style.flexGrow = 1;
            mainScene.style.flexShrink = 1;
            row.Add(mainScene);
            row.Add(HeaderCell("Scene set", SetWidth, SceneSystemUI.SceneSetTooltip));
            row.Add(HeaderCell("Status", StatusWidth, "Whether the scene group check finds problems in this group. Each problem is listed under the row with its fix."));
            VisualElement openColumn = new VisualElement();
            FixWidth(openColumn, OpenWidth);
            openColumn.style.marginRight = 0;
            row.Add(openColumn);
            return row;
        }

        private static Label HeaderCell(string _text, float _width, string _tooltip)
        {
            Label cell = new Label(_text);
            cell.tooltip = _tooltip;
            cell.style.unityFontStyleAndWeight = FontStyle.Bold;
            FixWidth(cell, _width);
            return cell;
        }

        // Every column but the main scene keeps its width, so the rows line up under the header.
        private static void FixWidth(VisualElement _cell, float _width)
        {
            _cell.style.width = _width;
            _cell.style.flexShrink = 0;
            _cell.style.marginRight = 6;
        }

        // Adds the group's row and, under it, its problems with their fixes. Returns the number of problems.
        private static int AddGroupRow(VisualElement _card, SceneGroup _group, SceneSystemSettings _settings, Action _requestRefresh)
        {
            VisualElement block = new VisualElement();
            block.style.marginTop = 1;
            block.style.marginBottom = 1;

            SceneGroupProblemsView problemsView = new SceneGroupProblemsView(false);
            problemsView.style.marginLeft = NameWidth;
            problemsView.style.marginBottom = 2;
            int problemCount = problemsView.Refresh(_group, _requestRefresh);

            VisualElement row = SceneSystemUI.Row();
            Label name = SceneSystemUI.AssetLink(_group, _group.name);
            FixWidth(name, NameWidth);
            row.Add(name);
            row.Add(MainSceneField(_group, _requestRefresh));
            row.Add(SetField(_group, _settings, _requestRefresh));
            row.Add(StatusCell(_group, problemCount));
            Button open = SceneSystemUI.Button("Open group", SceneSystemUI.OpenGroupTooltip, () => SceneGroupOpener.Open(_group));
            FixWidth(open, OpenWidth);
            open.style.marginLeft = 0;
            open.style.marginRight = 0;
            row.Add(open);

            block.Add(row);
            block.Add(problemsView);
            _card.Add(block);
            return problemCount;
        }

        // A small object field that shows the main scene's path and takes a new scene by drag or picker.
        private static VisualElement MainSceneField(SceneGroup _group, Action _requestRefresh)
        {
            SceneAsset current = null;
            string path = string.Empty;
            if (_group.MainScene != null)
            {
                current = _group.MainScene.SceneFieldReference.SceneAssetInEditor;
                path = _group.MainScene.ScenePath;
            }

            ObjectField field = new ObjectField();
            field.objectType = typeof(SceneAsset);
            field.allowSceneObjects = false;
            field.SetValueWithoutNotify(current);
            field.style.flexGrow = 1;
            field.style.flexShrink = 1;
            field.style.minWidth = MainSceneMinWidth;
            field.style.marginLeft = 0;
            field.style.marginRight = 6;
            field.tooltip = SceneSystemUI.MainSceneTooltip;
            if (!string.IsNullOrEmpty(path))
            {
                field.tooltip = path + "\n\n" + SceneSystemUI.MainSceneTooltip;
            }
            ShowPathInField(field, path);
            field.RegisterCallback<AttachToPanelEvent>(_event => ShowPathInField(field, path));
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

        // The object field shows "Level 7 (Scene Asset)"; the path says which "Level 7" it is. The end stays visible.
        private static void ShowPathInField(ObjectField _field, string _path)
        {
            if (string.IsNullOrEmpty(_path))
            {
                return;
            }
            Label label = _field.Q<Label>(className: ObjectFieldLabelClass);
            if (label == null)
            {
                return;
            }
            label.text = _path;
            label.style.overflow = Overflow.Hidden;
            label.style.whiteSpace = WhiteSpace.NoWrap;
            label.style.textOverflow = TextOverflow.Ellipsis;
            label.style.unityTextOverflowPosition = TextOverflowPosition.Start;
        }

        private static VisualElement SetField(SceneGroup _group, SceneSystemSettings _settings, Action _requestRefresh)
        {
            if (_settings == null)
            {
                Label none = new Label("—");
                none.tooltip = "Scene sets need the scene system to be set up.";
                FixWidth(none, SetWidth);
                return none;
            }
            PopupField<SceneSet> field = SceneSystemUI.SceneSetPopup(null, _settings, _group.SceneSet, _picked =>
            {
                SceneSetSync.PickSet(_group, _picked);
                _requestRefresh();
            });
            FixWidth(field, SetWidth);
            field.style.marginLeft = 0;
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
            VisualElement cell = SceneSystemUI.StatusRow(colour, text, string.Empty);
            FixWidth(cell, StatusWidth);
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
