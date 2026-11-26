using AbstractPixel.EditorModules.Documentation;
using AbstractPixel.SceneManagement.Editor;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>Sets the handler that SceneSystemDocs.Open calls, so Scene System Editor code can open this window.</summary>
    [InitializeOnLoad]
    public static class SceneDocumentationRegistration
    {
        static SceneDocumentationRegistration()
        {
            SceneSystemDocs.Handler = OpenTopic;
        }

        private static void OpenTopic(string _topicId)
        {
            SceneDocumentationWindow.OpenAt(_topicId);
        }
    }
}
