using System.Collections.Generic;
using AbstractPixel.Core;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>The fake loader the coordinator tests use (the Build Settings loader's stand-in).</summary>
    internal sealed class RecordingLoaderHarness : ILoaderContractHarness
    {
        public RecordingSceneLoader Recording { get; } = new RecordingSceneLoader();
        public ISceneLoader Loader => Recording;

        public SceneReference ReferenceTo(string _sceneName)
        {
            return new SceneReference(_sceneName);
        }

        public string HeldSceneName => Recording.HeldScene;
        public string ActiveSceneName => Recording.ActiveSceneName;
        public List<string> ActivatedScenes => Recording.ActivatedScenes;
        public List<string> ClosedScenes => Recording.ClosedScenes;
    }
}
