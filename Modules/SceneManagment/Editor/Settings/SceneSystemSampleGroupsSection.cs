using System.Collections.Generic;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The Sample scene groups section of the Scene System page (ADR 0014): each sample scene group with its main scene,
    /// whether any of their scenes is in the build, and "Remove sample scenes from the build".
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
                _card.Add(SceneSystemUI.Hint("None. Tick Sample scene group on a scene group to keep its scenes out of builds."));
                return;
            }

            foreach (SceneGroup sample in samples)
            {
                VisualElement row = SceneSystemUI.Row();
                row.style.marginBottom = 2;
                row.Add(SceneSystemUI.AssetLink(sample, sample.name));
                string mainScene = sample.MainScene != null ? sample.MainScene.SceneName : string.Empty;
                VisualElement spacer = SceneSystemUI.Spacer();
                row.Add(spacer);
                row.Add(SceneSystemUI.Hint(mainScene));
                _card.Add(row);
            }

            string message = SampleSceneBuildCheck.FailureMessage(SampleSceneBuildCheck.ScenesInBuild(), SampleSceneGroups.ScenePaths(), IsAddressable);
            if (string.IsNullOrEmpty(message))
            {
                _card.Add(SceneSystemUI.StatusRow(SceneSystemUI.Green, "None of their scenes are in the build.", string.Empty));
                return;
            }
            HelpBox box = SceneSystemUI.HelpBoxWithButton("Sample scenes are in the build, so a build would stop.", HelpBoxMessageType.Error, "Remove sample scenes from the build", message, () =>
            {
                SampleSceneBuildCheck.RemoveSampleScenesFromBuild();
                Fill(_card);
            });
            _card.Add(box);
        }

        private static bool IsAddressable(string _scenePath)
        {
            AbstractPixel.Core.Editor.IAddressableSceneProvider provider = AbstractPixel.Core.Editor.AddressableScenes.Provider;
            if (provider == null)
            {
                return false;
            }
            return provider.IsAddressable(UnityEditor.AssetDatabase.AssetPathToGUID(_scenePath));
        }
    }
}
