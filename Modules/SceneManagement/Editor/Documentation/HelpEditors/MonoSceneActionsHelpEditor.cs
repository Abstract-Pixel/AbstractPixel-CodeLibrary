using AbstractPixel.EditorModules.Documentation;
using AbstractPixel.SceneManagement.Editor;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>The MonoSceneActions inspector: Unity's default fields under a help row.</summary>
    [CustomEditor(typeof(AbstractPixel.SceneManagement.MonoSceneActions))]
    public class MonoSceneActionsHelpEditor : SceneDocsHelpEditor
    {
        protected override string Summary
        {
            get { return "Change scene from a UnityEvent: point a button's On Click at ChangeTo, Retry or Preload."; }
        }

        protected override string TopicId
        {
            get { return SceneSystemDocTopics.MonoSceneActions; }
        }
    }
}
