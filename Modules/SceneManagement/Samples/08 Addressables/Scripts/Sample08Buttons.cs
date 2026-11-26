using System.Threading.Tasks;
using UnityEngine;
#if ABSTRACTPIXEL_SAMPLES_ADDRESSABLES
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.SceneManagement;
#endif

namespace AbstractPixel.SceneManagement.Samples
{
    /// <summary>
    /// Sample 08: a button that changes to the scene group whose main scene is Addressable, and one back. Shows whether
    /// Addressables knows the scene. Without the Addressables package it only shows what to install.
    /// </summary>
    public class Sample08Buttons : MonoBehaviour
    {
        private const int StatusLine = 1;
        private const string NeedsAddressables = "Sample 08 needs the Addressables package (com.unity.addressables). Install it with the Package Manager.";

        [SerializeField] private SceneGroup otherGroup;
        [SerializeField] private string buttonText = "Go to the Addressable level";
        [Tooltip("The Addressable scene, as a scene reference: Addressables finds it by the reference's GUID, as the scene system's loader does.")]
        [SerializeField] private AbstractPixel.Core.SceneReference addressableScene = new AbstractPixel.Core.SceneReference(string.Empty);
        [SerializeField] private SampleLabel label;

#if ABSTRACTPIXEL_SAMPLES_ADDRESSABLES
        private float nextStatusTime;

        /// <summary>Changes to the other scene group.</summary>
        public Task<SceneChangeResult> Go()
        {
            return SceneActions.ChangeTo(otherGroup);
        }

        private void Update()
        {
            if (label == null || Time.unscaledTime < nextStatusTime)
            {
                return;
            }
            nextStatusTime = Time.unscaledTime + 0.5f;
            label.SetLine(StatusLine, DescribeAddressableScene());
        }

        // Whether Addressables has an entry for the scene (by its GUID), and whether the scene is open now.
        private string DescribeAddressableScene()
        {
            bool isAddressable = HasAddressablesEntry(addressableScene);
            bool isOpen = SceneManager.GetSceneByName(addressableScene.SceneName).isLoaded;
            string where = "not Addressable: it opens from its path";
            if (isAddressable)
            {
                where = "Addressable: it opens through Addressables";
            }
            string open = "closed";
            if (isOpen)
            {
                open = "open";
            }
            return addressableScene.SceneName + " is " + where + " (now " + open + ").";
        }

        // A scene known only by name cannot be Addressable: the entry is found by the scene reference's GUID.
        private static bool HasAddressablesEntry(AbstractPixel.Core.SceneReference _scene)
        {
            if (_scene == null || _scene.HasGuid == false || Addressables.ResourceLocators == null)
            {
                return false;
            }
            string _guid = _scene.SceneGuid;
            foreach (var locator in Addressables.ResourceLocators)
            {
                bool found = locator.Locate(_guid, typeof(UnityEngine.ResourceManagement.ResourceProviders.SceneInstance), out IList<IResourceLocation> locations);
                if (found && locations != null && locations.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        private void OnGUI()
        {
            Rect area = new Rect(Screen.width - 290f, 10f, 280f, 60f);
            GUILayout.BeginArea(area);
            if (GUILayout.Button(buttonText))
            {
                RunAndForget(Go());
            }
            GUILayout.EndArea();
        }

        // Button clicks cannot await; an exception still reaches the Console.
        private static async void RunAndForget(Task _change)
        {
            await _change;
        }
#else
        private void Start()
        {
            if (label != null)
            {
                label.SetLine(StatusLine, NeedsAddressables);
            }
        }
#endif
    }
}
