#if ABSTRACTPIXEL_ADDRESSABLES
using System;
using AbstractPixel.Core.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Tells the scene field whether a scene is Addressable, through the Addressables editor API. Compiled only when
    /// the Addressables package is installed (ABSTRACTPIXEL_ADDRESSABLES, set by this assembly's versionDefines).
    /// </summary>
    [InitializeOnLoad]
    internal sealed class AddressablesSceneProvider : IAddressableSceneProvider
    {
        public event Action Changed;

        static AddressablesSceneProvider()
        {
            AddressablesSceneProvider provider = new AddressablesSceneProvider();
            AddressableAssetSettings.OnModificationGlobal += provider.OnAddressablesModified;
            AddressableScenes.Register(provider);
            // The loader asks the Editor settings instead of starting Addressables just to look a scene up.
            AddressableSceneCheck.EditorAnswer = provider.IsAddressable;
        }

        public bool IsAddressable(string _sceneGuid)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null || string.IsNullOrEmpty(_sceneGuid))
            {
                return false;
            }
            // A scene inside an Addressable folder has no entry of its own but still loads through Addressables.
            AddressableAssetEntry entry = settings.FindAssetEntry(_sceneGuid, true);
            return entry != null;
        }

        public bool TryMakeAddressable(string _sceneGuid, out string _error)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                _error = "this project has no Addressables settings yet. Create them in Window > Asset Management > Addressables > Groups.";
                return false;
            }
            if (settings.DefaultGroup == null)
            {
                _error = "Addressables has no default group. Set one in Window > Asset Management > Addressables > Groups.";
                return false;
            }

            AddressableAssetEntry entry = settings.CreateOrMoveEntry(_sceneGuid, settings.DefaultGroup);
            if (entry == null)
            {
                _error = "Addressables refused the scene's entry.";
                return false;
            }
            _error = string.Empty;
            return true;
        }

        public void RemoveFromAddressables(string _sceneGuid)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                return;
            }
            settings.RemoveAssetEntry(_sceneGuid);
        }

        private void OnAddressablesModified(AddressableAssetSettings _settings, AddressableAssetSettings.ModificationEvent _event, object _data)
        {
            if (Changed == null)
            {
                return;
            }
            Changed();
        }
    }
}
#endif
