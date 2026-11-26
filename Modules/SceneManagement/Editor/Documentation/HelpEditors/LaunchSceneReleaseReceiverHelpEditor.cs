using AbstractPixel.EditorModules.Documentation;
using AbstractPixel.SceneManagement.Editor;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>The LaunchSceneReleaseReceiver inspector: Unity's default fields under a help row.</summary>
    [CustomEditor(typeof(AbstractPixel.SceneManagement.SceneScreens.LaunchSceneReleaseReceiver))]
    public class LaunchSceneReleaseReceiverHelpEditor : SceneDocsHelpEditor
    {
        protected override string Summary
        {
            get { return "Releases the launch scene hold when the Timeline reaches a release marker."; }
        }

        protected override string TopicId
        {
            get { return SceneSystemDocTopics.LaunchSceneReleaseReceiver; }
        }
    }
}
