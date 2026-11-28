using AbstractPixel.Core.Editor.UI;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The Sample scene groups section of the Scene System page (ADR 0014): each sample scene group, numbered, as a
    /// read-only reference (it tints under the pointer and a click selects it), then whether any of their scenes is in
    /// the build, with "Remove sample scenes from the build". Groups or scenes that are missing (a sample deleted, or
    /// one that came from another project) are skipped quietly.
    /// </summary>
    internal static class SceneSystemSampleGroupsSection
    {
        public const string Title = "Sample scene groups";
        public const string Tooltip = "Scene groups marked as samples. Their scenes open in the Editor but never go into a build; a build that still contains one stops.";

        public static void Fill(VisualElement _card)
        {
            _card.Clear();
            SampleSceneGroups.Forget();
            List<SceneGroup> samples = SampleSceneGroups.All();
            if (samples.Count == 0)
            {
                _card.Add(EditorUI.Hint("None. Tick Sample scene group on a scene group to keep its scenes out of builds."));
                return;
            }

            int number = 1;
            foreach (SceneGroup sample in samples)
            {
                if (sample == null)
                {
                    continue;
                }
                _card.Add(SampleRow(number, sample));
                number++;
            }
            _card.Add(BuildState());
        }

        private static VisualElement SampleRow(int _number, SceneGroup _sample)
        {
            VisualElement row = EditorUI.Row();
            row.style.marginBottom = 2;
            Label index = new Label(_number + ".");
            index.style.width = 22;
            index.style.flexShrink = 0;
            row.Add(index);
            ObjectField reference = SceneSystemUI.ReadOnlyObjectField(_sample, typeof(SceneGroup), "Click to select it in the Project window.");
            reference.style.flexGrow = 1;
            AddHoverAndSelect(reference, _sample);
            row.Add(reference);
            return row;
        }

        // Read-only, so the field acts as a link: it tints under the pointer and one click selects the asset.
        private static void AddHoverAndSelect(VisualElement _field, Object _asset)
        {
            VisualElement input = _field.Q(className: ObjectField.inputUssClassName);
            VisualElement tinted = input != null ? input : _field;
            StyleColor normal = tinted.style.backgroundColor;
            Color hover = EditorGUIUtility.isProSkin ? new Color(0.27f, 0.35f, 0.45f) : new Color(0.79f, 0.86f, 0.95f);
            _field.RegisterCallback<PointerEnterEvent>(_event => tinted.style.backgroundColor = hover);
            _field.RegisterCallback<PointerLeaveEvent>(_event => tinted.style.backgroundColor = normal);
            _field.RegisterCallback<PointerDownEvent>(_event =>
            {
                EditorUI.PingAndSelect(_asset);
                _event.StopPropagation();
            }, TrickleDown.TrickleDown);
        }

        // Nothing to fix is a plain note, not a green light; sample scenes in the build are a red box with the fix.
        private static VisualElement BuildState()
        {
            string message = SampleSceneBuildCheck.FailureMessage(SampleSceneBuildCheck.ScenesInBuild(), SampleSceneGroups.ScenePaths(), IsAddressable);
            if (string.IsNullOrEmpty(message))
            {
                Label note = new Label("<b>Note:</b> none of their scenes are in the build.");
                note.enableRichText = true;
                note.style.marginTop = 6;
                note.style.color = EditorUI.Yellow;
                return note;
            }
            return EditorUI.HelpBoxWithButton("Sample scenes are in the build, so a build would stop.", HelpBoxMessageType.Error, "Remove sample scenes from the build", message, () =>
            {
                SampleSceneBuildCheck.RemoveSampleScenesFromBuild();
                SceneSystemSettingsProvider.Open();
            });
        }

        private static bool IsAddressable(string _scenePath)
        {
            AbstractPixel.Core.Editor.IAddressableSceneProvider provider = AbstractPixel.Core.Editor.AddressableScenes.Provider;
            if (provider == null || string.IsNullOrEmpty(_scenePath))
            {
                return false;
            }
            return provider.IsAddressable(AssetDatabase.AssetPathToGUID(_scenePath));
        }
    }
}
