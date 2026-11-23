using UnityEditor;
#if ABSTRACTPIXEL_ADDRESSABLES
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Whether Addressables can take a scene now, and the way to its Groups window. A scene group cannot fix either
    /// problem here (no settings, no default group), so the scene picker sends the user to the Groups window. The
    /// Addressables calls compile only when the package is installed (ABSTRACTPIXEL_ADDRESSABLES).
    /// </summary>
    public static class AddressablesSetup
    {
        /// <summary>The Addressables package's own menu item; the package has no public call that opens the window.</summary>
        public const string GroupsMenuPath = "Window/Asset Management/Addressables/Groups";

        public const string NoSettingsText = "Addressables has no settings yet. Create them there.";
        public const string NoDefaultGroupText = "Addressables has no default group. Set one there.";

        /// <summary>Why Addressables cannot take a scene now; empty when it can, or when the package is not installed.</summary>
        public static string Problem()
        {
#if ABSTRACTPIXEL_ADDRESSABLES
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                return NoSettingsText;
            }
            if (settings.DefaultGroup == null)
            {
                return NoDefaultGroupText;
            }
#endif
            return string.Empty;
        }

        /// <summary>Opens the Addressables Groups window. Without the package it says where the window would be.</summary>
        public static void OpenGroupsWindow()
        {
            bool opened = EditorApplication.ExecuteMenuItem(GroupsMenuPath);
            if (!opened)
            {
                UnityEngine.Debug.Log("[Scene system] The Addressables Groups window is at Window > Asset Management > Addressables > Groups, once the Addressables package is installed.");
            }
        }
    }
}
