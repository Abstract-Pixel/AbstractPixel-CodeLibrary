using System;

namespace AbstractPixel.Core.Editor
{
    /// <summary>
    /// Answers whether a scene is Addressable and changes that, for the scene field. Core never references the
    /// Addressables package (ADR 0005); the scene module registers an implementation when the package is installed.
    /// </summary>
    public interface IAddressableSceneProvider
    {
        /// <summary>Raised when Addressables entries change, so open scene fields can show the new status.</summary>
        event Action Changed;

        bool IsAddressable(string _sceneGuid);

        /// <summary>Gives the scene an Addressables entry. Returns false with the reason when it can't.</summary>
        bool TryMakeAddressable(string _sceneGuid, out string _error);

        void RemoveFromAddressables(string _sceneGuid);
    }
}
