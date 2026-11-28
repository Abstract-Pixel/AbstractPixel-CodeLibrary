using AbstractPixel.Core.Editor.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>Loads a page's UXML from the library and fills in the logo and the "Game Management" pill.</summary>
    internal static class OnboardingPageLoader
    {
        private const string PageFolder = "Packages/com.abstractpixel.library/Modules/GameManagement/Editor/Onboarding/Window/Pages/";

        internal static void Load(VisualElement _root, string _template, bool _largeHeader)
        {
            VisualTreeAsset tree = LoadTemplate(_template);
            if (tree == null)
            {
                _root.Add(new Label("The setup page '" + _template + "' is missing from the library. Reimport the library."));
                return;
            }
            tree.CloneTree(_root);
            Image logo = _root.Q<Image>("logo");
            if (logo != null)
            {
                logo.image = Resources.Load<Texture2D>("AbstractPixelLogo");
            }
            VisualElement pillHost = _root.Q<VisualElement>("pillHost");
            if (pillHost != null)
            {
                pillHost.Add(SystemPill.Create("Game Management", _largeHeader));
            }
        }

        /// <summary>A small piece of a page (a menu card) to clone as many times as needed.</summary>
        internal static VisualTreeAsset LoadTemplate(string _template)
        {
            return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PageFolder + _template + ".uxml");
        }
    }
}
