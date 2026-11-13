using System;
using System.Collections.Generic;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Tells the save system about scenes: which one is active, which are loaded, and when a newly loaded scene
    /// is ready, so its Scene save files can raise their load events.
    /// </summary>
    internal interface ISceneInfo
    {
        string ActiveSceneName { get; }

        IReadOnlyList<string> LoadedSceneNames { get; }

        /// <summary>Raised once per loaded scene, one frame after its scripts start, so its Savable Bridges have restored.</summary>
        event Action<string> SceneReady;

        event Action<string> SceneUnloaded;
    }
}
