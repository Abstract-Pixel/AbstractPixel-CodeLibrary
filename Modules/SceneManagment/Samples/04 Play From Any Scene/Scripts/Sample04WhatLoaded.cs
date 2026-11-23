using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement.Samples
{
    /// <summary>
    /// Sample 04: lives in the shared scene and lists what Play opened: the open scenes and the active scene group.
    /// Whichever scene of the group Play starts from, the list is the same.
    /// </summary>
    public class Sample04WhatLoaded : MonoBehaviour
    {
        private const int ListLine = 1;

        private SampleLabel label;
        private int shownSceneCount = -1;
        private SceneGroup shownGroup;

        /// <summary>The open scenes and the active scene group, as the label shows them.</summary>
        public string Listing { get; private set; } = string.Empty;

        private void Update()
        {
            bool nothingChanged = SceneManager.sceneCount == shownSceneCount && SceneActions.ActiveSceneGroup == shownGroup;
            if (nothingChanged)
            {
                return;
            }
            Listing = DescribeWhatIsOpen();
            shownSceneCount = SceneManager.sceneCount;
            shownGroup = SceneActions.ActiveSceneGroup;
            ShowListing();
        }

        private static string DescribeWhatIsOpen()
        {
            StringBuilder text = new StringBuilder("Open scenes: ");
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                if (index > 0)
                {
                    text.Append(", ");
                }
                text.Append(SceneManager.GetSceneAt(index).name);
            }
            SceneGroup group = SceneActions.ActiveSceneGroup;
            string groupName = "none yet";
            if (group != null)
            {
                groupName = group.name;
            }
            text.Append("\nActive scene group: ").Append(groupName);
            return text.ToString();
        }

        // The label is in the main scene; this shared scene finds it once it is open.
        private void ShowListing()
        {
            if (label == null)
            {
                label = FindAnyObjectByType<SampleLabel>();
            }
            if (label == null)
            {
                return;
            }
            label.SetLine(ListLine, Listing);
        }
    }
}
