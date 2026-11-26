using System;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The two scene opening preferences, per user and per project, both on by default (session 5A2):
    /// <list type="bullet">
    /// <item>"Open a main scene with its scene group": a group's main scene opens its manager and shared scenes; a manager
    /// or shared scene opened alone opens the start scene group; a sample scene opens its sample scene group.</item>
    /// <item>"Open a scene in no group with the default scene set": a scene in no group opens the default scene set's scenes.</item>
    /// </list>
    /// The first acts in edit mode only. The second also decides Play: off, a scene in no group plays with no manager or shared scenes.
    /// Shown in Preferences &gt; Abstract Pixel &gt; Scenes, the Tools menu, setup and the scene picker's footer.
    /// </summary>
    public static class SceneOpeningPreferences
    {
        public const string MainSceneWithGroupLabel = "Open a main scene with its scene group";
        public const string NoGroupSceneWithDefaultSetLabel = "Open a scene in no group with the default scene set";

        public const string MainSceneWithGroupTooltip = "When you open a scene group's main scene on its own, also open its manager and shared scenes, so the Hierarchy shows what Play loads. A manager or shared scene opened alone opens the start scene group; a sample scene opens its sample scene group. Play always loads them. For you only, in this project.";
        public const string NoGroupSceneWithDefaultSetTooltip = "When you open a scene in no group on its own, also open the default scene set's manager and shared scenes, and load them in Play. Off: the scene opens and plays on its own. For you only, in this project.";

        /// <summary>Raised after either preference changes, so every place that shows them can refresh.</summary>
        public static event Action Changed;

        public static bool MainSceneWithGroup
        {
            get { return Store.MainSceneWithGroup; }
            set
            {
                Store.MainSceneWithGroup = value;
                RaiseChanged();
            }
        }

        public static bool NoGroupSceneWithDefaultSet
        {
            get { return Store.NoGroupSceneWithDefaultSet; }
            set
            {
                Store.NoGroupSceneWithDefaultSet = value;
                RaiseChanged();
            }
        }

        public static SceneOpeningChoices Choices
        {
            get { return Store.Choices; }
        }

        private static SceneOpeningPreferenceStore Store
        {
            get { return new SceneOpeningPreferenceStore(EditorPrefsBoolStore.Instance, PlayerSettings.productGUID.ToString()); }
        }

        private static void RaiseChanged()
        {
            Action handlers = Changed;
            if (handlers != null)
            {
                handlers();
            }
        }
    }
}
