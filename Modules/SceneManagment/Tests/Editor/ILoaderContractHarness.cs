using System.Collections.Generic;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>
    /// A loader plus what the shared contract tests need to see of Unity's side. Each loader's fake provides one:
    /// the recording loader for Build Settings scenes, and the Addressables loader on a fake Addressables API.
    /// Scene names here are the names the tests passed in.
    /// </summary>
    internal interface ILoaderContractHarness
    {
        ISceneLoader Loader { get; }

        /// <summary>A reference the loader accepts for a scene with this name (the Addressables loader needs a GUID).</summary>
        AbstractPixel.Core.SceneReference ReferenceTo(string _sceneName);

        /// <summary>The scene that waits for activation, or null.</summary>
        string HeldSceneName { get; }

        /// <summary>The scene the loader made the active scene, or null.</summary>
        string ActiveSceneName { get; }

        /// <summary>Every scene that was let through activation, in order, whether to open or to close.</summary>
        List<string> ActivatedScenes { get; }

        /// <summary>Every scene that was closed, in order.</summary>
        List<string> ClosedScenes { get; }
    }
}
