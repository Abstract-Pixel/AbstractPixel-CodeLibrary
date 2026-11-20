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

        private const float NameWidth = 150;
        private const float MainSceneMinWidth = 90;
        private const float MainSceneMaxWidth = 300;
        private const float ColumnGap = 14;
        private const float MinColumnWidth = 70;
        private const float SetWidth = 140;
        private const float StatusWidth = 150;
        private const float OpenWidth = 84;

        /// <summary>Fills the section's card and the summary next to its title ("16 groups, 0 problems").</summary>
        public static void Fill(VisualElement _card, VisualElement _summary, SceneSystemSettings _settings, Action _requestRefresh)
        {
            _card.Clear();
            if (_settings != null)
            {
                _card.Add(BeforeBuildToggle(_settings));
            }
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

        private static Toggle BeforeBuildToggle(SceneSystemSettings _settings)
        {
            Toggle beforeBuild = new Toggle("Check scene groups before a build");
            beforeBuild.value = _settings.CheckSceneGroupsBeforeBuild;
            beforeBuild.tooltip = "A build stops when any scene group below has a problem, a scene listed twice included, with a message naming each group and scene. Saved in the settings asset, so it is shared through version control.";
            beforeBuild.style.marginLeft = 0;
            beforeBuild.style.marginBottom = 8;
            beforeBuild.RegisterValueChangedCallback(_change =>
            {
                _settings.CheckSceneGroupsBeforeBuild = _change.newValue;
                SceneSystemSettingsAssets.Save(_settings);
            });
            return beforeBuild;
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
            mainScene.style.maxWidth = MainSceneMaxWidth;
            mainScene.style.minWidth = MainSceneMinWidth;
            row.Add(mainScene);
            row.Add(HeaderCell("Scene set", SetWidth, SceneSystemUI.SceneSetTooltip));
            row.Add(HeaderCell("Status", StatusWidth, "Whether the scene group check finds problems in this group. Each problem is listed under the row with its fix."));
            VisualElement openColumn = new VisualElement();
            FixWidth(openColumn, OpenWidth);
            openColumn.style.flexShrink = 0;
            openColumn.style.marginRight = 0;
            row.Add(openColumn);
            return row;
        }

        private static Label HeaderCell(string _text, float _width, string _tooltip)
        {
            Label cell = new Label(_text);
            cell.tooltip = _tooltip;
            cell.style.unityFontStyleAndWeight = FontStyle.Bold;
            cell.style.whiteSpace = WhiteSpace.NoWrap;
            cell.style.textOverflow = TextOverflow.Ellipsis;
            FixWidth(cell, _width);
            return cell;
        }

        // Every column but the main scene starts at its width and gives way only in a narrow window, down to a minimum,
        // the same in the header and the rows, so they stay lined up.
        private static void FixWidth(VisualElement _cell, float _width)
        {
            _cell.style.width = _width;
            _cell.style.flexShrink = 1;
            _cell.style.minWidth = MinColumnWidth;
            _cell.style.overflow = Overflow.Hidden;
            _cell.style.marginRight = ColumnGap;
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
            open.style.flexShrink = 0;
            open.style.marginLeft = 0;
            open.style.marginRight = 0;
            row.Add(open);

            block.Add(row);
            block.Add(problemsView);
            _card.Add(block);
            return problemCount;
        }

        // A small object field with the main scene (its path in the tooltip) that takes a new scene by drag or picker.
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
            field.style.maxWidth = MainSceneMaxWidth;
            field.style.marginLeft = 0;
            field.style.marginRight = ColumnGap;
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
            VisualElement cell = SceneSystemUI.StatusRow(colour, text, text);
            FixWidth(cell, StatusWidth);
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
