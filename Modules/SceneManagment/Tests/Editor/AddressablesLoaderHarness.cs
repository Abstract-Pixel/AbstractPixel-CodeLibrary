using System.Collections.Generic;
using AbstractPixel.Core;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>The Addressables loader on a fake Addressables API. Scene names stand in for GUIDs.</summary>
    internal sealed class AddressablesLoaderHarness : ILoaderContractHarness
    {
        public FakeAddressablesApi Api { get; } = new FakeAddressablesApi();
        public ISceneLoader Loader { get; }

        public AddressablesLoaderHarness()
        {
            Loader = new AddressablesSceneLoader(Api);
        }

        public SceneReference ReferenceTo(string _sceneName)
        {
            return new SceneReference(_sceneName, "guid-" + _sceneName, "Assets/" + _sceneName + ".unity");
        }

        public string HeldSceneName => ToName(Api.HeldKey);
        public string ActiveSceneName => ToName(Api.ActiveKey);
        public List<string> ActivatedScenes => Api.ActivatedKeys.ConvertAll(ToName);
        public List<string> ClosedScenes => Api.ClosedKeys.ConvertAll(ToName);

        private static string ToName(string _key)
        {
            if (_key == null)
            {
                return null;
            }

            return _key.Substring("guid-".Length);
        }
    }
}
