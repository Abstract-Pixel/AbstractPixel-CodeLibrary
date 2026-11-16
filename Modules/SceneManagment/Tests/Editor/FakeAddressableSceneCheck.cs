using System.Collections.Generic;
using System.Threading.Tasks;
using AbstractPixel.Core;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>Says a scene is Addressable when its GUID was added. Counts the questions it was asked.</summary>
    internal sealed class FakeAddressableSceneCheck : IAddressableSceneCheck
    {
        private readonly HashSet<string> addressableGuids = new HashSet<string>();

        public int QuestionsAsked { get; private set; }

        public void MarkAddressable(string _guid)
        {
            addressableGuids.Add(_guid);
        }

        public Task<bool> IsAddressable(SceneReference _scene)
        {
            QuestionsAsked++;
            bool isAddressable = _scene.HasGuid && addressableGuids.Contains(_scene.SceneGuid);
            return Task.FromResult(isAddressable);
        }
    }
}
