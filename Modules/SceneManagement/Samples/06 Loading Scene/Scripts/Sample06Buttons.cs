using System.Threading.Tasks;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Samples
{
    /// <summary>Sample 06: one button that changes to the other scene group.</summary>
    public class Sample06Buttons : MonoBehaviour
    {
        [SerializeField] private SceneGroup otherGroup;
        [SerializeField] private string buttonText = "Go to Destination";

        /// <summary>Changes to the other scene group.</summary>
        public Task<SceneChangeResult> Go()
        {
            return SceneActions.ChangeTo(otherGroup);
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
    }
}
