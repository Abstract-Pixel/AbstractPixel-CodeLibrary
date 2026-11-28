using AbstractPixel.Core.Editor.UI;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Asks what happens before a scene set is removed, when scene groups use it or it is the default: move the groups
    /// to another scene set, or convert them to override (their own scenes, scene set None); and which set becomes the
    /// default. Remove scene set runs SceneSetRemoval; Cancel changes nothing.
    /// </summary>
    public class SceneSetRemovalWindow : EditorWindow
    {
        private SceneSystemSettings settings;
        private SceneSet set;
        private Action onRemoved;
        private SceneSetRemovalMode mode = SceneSetRemovalMode.MoveToAnotherSet;
        private SceneSet replacement;
        private SceneSet newDefault;
        private bool defaultPickedByHand;
        private VisualElement choices;
        private VisualElement problemArea;
        private PopupField<SceneSet> defaultField;

        // The "?" in the tab bar. Unity calls this by reflection.
        private void ShowButton(Rect _position)
        {
            SceneSystemHelp.TabBarButton(_position, SceneSystemDocTopics.SceneSetRemoval);
        }

        public static SceneSetRemovalWindow Open(SceneSystemSettings _settings, SceneSet _set, Action _onRemoved)
        {
            SceneSetRemovalWindow window = CreateInstance<SceneSetRemovalWindow>();
            window.titleContent = new GUIContent("Remove scene set");
            window.settings = _settings;
            window.set = _set;
            window.onRemoved = _onRemoved;
            window.PickStartingChoices();
            window.minSize = new Vector2(480, 340);
            window.maxSize = new Vector2(720, 640);
            window.ShowUtility();
            window.BuildGUI();
            return window;
        }

        private List<SceneSet> OtherSets()
        {
            List<SceneSet> others = new List<SceneSet>();
            if (settings == null)
            {
                return others;
            }
            foreach (SceneSet candidate in settings.SceneSets)
            {
                if (candidate != null && candidate != set)
                {
                    others.Add(candidate);
                }
            }
            return others;
        }

        // Move when there is somewhere to move to (the default set first), otherwise convert to override.
        private void PickStartingChoices()
        {
            List<SceneSet> others = OtherSets();
            replacement = null;
            if (others.Count > 0)
            {
                replacement = others[0];
                if (settings != null && settings.DefaultSceneSet != null && others.Contains(settings.DefaultSceneSet))
                {
                    replacement = settings.DefaultSceneSet;
                }
            }
            mode = replacement == null ? SceneSetRemovalMode.ConvertToOverride : SceneSetRemovalMode.MoveToAnotherSet;
            newDefault = replacement;
        }

        private bool IsDefault
        {
            get { return settings != null && set != null && settings.DefaultSceneSet == set; }
        }

        private void BuildGUI()
        {
            VisualElement root = rootVisualElement;
            root.Clear();
            root.style.paddingLeft = 14;
            root.style.paddingRight = 14;
            root.style.paddingTop = 12;
            root.style.paddingBottom = 12;
            if (set == null)
            {
                root.Add(new HelpBox("This scene set no longer exists.", HelpBoxMessageType.Info));
                return;
            }

            List<SceneGroup> users = SceneSetSync.GroupsUsing(set);
            Label title = new Label("Remove scene set '" + set.name + "'");
            title.style.fontSize = 15;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 8;
            title.style.whiteSpace = WhiteSpace.Normal;
            root.Add(title);

            if (users.Count > 0)
            {
                string groupWord = users.Count == 1 ? "scene group" : "scene groups";
                root.Add(new HelpBox("This scene set is used by " + users.Count + " " + groupWord + ". Choose what happens to them, then remove the set.", HelpBoxMessageType.Warning));
                root.Add(UsersFoldout(users));
                choices = new VisualElement();
                choices.style.marginTop = 8;
                root.Add(choices);
                FillChoices();
            }
            else
            {
                root.Add(EditorUI.Hint("No scene group uses this scene set."));
            }

            if (IsDefault)
            {
                root.Add(DefaultRow());
            }

            problemArea = new VisualElement();
            problemArea.style.marginTop = 6;
            root.Add(problemArea);
            root.Add(EditorUI.Spacer());

            Label warning = EditorUI.Hint("This can't be undone: the set moves to your system's trash.");
            warning.style.marginTop = 8;
            root.Add(warning);
            VisualElement buttons = EditorUI.Row();
            buttons.style.marginTop = 8;
            buttons.style.justifyContent = Justify.FlexEnd;
            Button cancel = EditorUI.Button("Cancel", "Closes this window and changes nothing.", Close);
            cancel.style.width = 90;
            buttons.Add(cancel);
            Button remove = EditorUI.PrimaryButton("Remove scene set", "Applies the choice above to the scene groups, then moves the set to the trash.", RemoveSet);
            remove.style.marginLeft = 6;
            buttons.Add(remove);
            root.Add(buttons);
        }

        private static Foldout UsersFoldout(List<SceneGroup> _users)
        {
            Foldout foldout = new Foldout();
            foldout.text = "Scene groups using it (" + _users.Count + ")";
            foldout.value = false;
            foldout.style.marginTop = 4;
            foreach (SceneGroup group in _users)
            {
                Label link = SceneSystemUI.AssetLink(group, group.name);
                link.style.alignSelf = Align.FlexStart;
                foldout.Add(link);
            }
            return foldout;
        }

        private void FillChoices()
        {
            choices.Clear();
            List<SceneSet> others = OtherSets();
            bool canMove = others.Count > 0;
            string moveLine = canMove ? "Each group takes the scenes of the set you pick. Parts a group overrides stay as they are." : "There is no other scene set to move them to.";
            choices.Add(EditorUI.ChoiceCard("Move them to another scene set", moveLine, mode == SceneSetRemovalMode.MoveToAnotherSet, canMove,
                "The scene groups point at the scene set picked below instead.", () => PickMode(SceneSetRemovalMode.MoveToAnotherSet)));
            if (canMove && mode == SceneSetRemovalMode.MoveToAnotherSet)
            {
                PopupField<SceneSet> target = new PopupField<SceneSet>("Move them to", others, others.IndexOf(replacement), SetName, SetName);
                target.tooltip = "The scene set the scene groups use after the removal.";
                target.style.marginLeft = 28;
                target.style.marginBottom = 8;
                target.RegisterValueChangedCallback(_change => PickReplacement(_change.newValue));
                choices.Add(target);
            }
            choices.Add(EditorUI.ChoiceCard("Convert to override", "Each group keeps the scenes it gets now as its own, and its scene set becomes None.", mode == SceneSetRemovalMode.ConvertToOverride, true,
                "Nothing in the scene groups' scenes changes; they just stop following a scene set.", () => PickMode(SceneSetRemovalMode.ConvertToOverride)));
        }

        private VisualElement DefaultRow()
        {
            VisualElement box = new VisualElement();
            box.style.marginTop = 8;
            box.Add(new HelpBox("'" + set.name + "' is the default scene set: new scene groups start with it. Pick the new default.", HelpBoxMessageType.Info));
            List<SceneSet> options = new List<SceneSet> { null };
            options.AddRange(OtherSets());
            defaultField = new PopupField<SceneSet>("New default", options, options.IndexOf(newDefault), DefaultName, DefaultName);
            defaultField.tooltip = "The scene set new scene groups start with after the removal.";
            defaultField.style.marginTop = 4;
            defaultField.RegisterValueChangedCallback(_change =>
            {
                newDefault = _change.newValue;
                defaultPickedByHand = true;
            });
            box.Add(defaultField);
            return box;
        }

        private void PickMode(SceneSetRemovalMode _mode)
        {
            mode = _mode;
            FillChoices();
        }

        // The new default follows the set the groups move to, until it is picked by hand.
        private void PickReplacement(SceneSet _replacement)
        {
            replacement = _replacement;
            if (defaultPickedByHand || defaultField == null)
            {
                return;
            }
            newDefault = _replacement;
            defaultField.SetValueWithoutNotify(_replacement);
        }

        private void RemoveSet()
        {
            string error = SceneSetRemoval.Remove(settings, set, mode, replacement, newDefault);
            if (!string.IsNullOrEmpty(error))
            {
                problemArea.Clear();
                problemArea.Add(new HelpBox(error, HelpBoxMessageType.Error));
                return;
            }
            Close();
            if (onRemoved != null)
            {
                onRemoved();
            }
        }

        private static string SetName(SceneSet _set)
        {
            return _set == null ? "None" : _set.name;
        }

        private static string DefaultName(SceneSet _set)
        {
            return _set == null ? "None (new groups start without a set)" : _set.name;
        }
    }
}
