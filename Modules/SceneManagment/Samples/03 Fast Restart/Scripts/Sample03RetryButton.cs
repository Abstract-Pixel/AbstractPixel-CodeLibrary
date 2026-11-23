using System.Threading.Tasks;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Samples
{
    /// <summary>Sample 03: Retry the open scene group, once or twice in one frame, and show the results.</summary>
    public class Sample03RetryButton : MonoBehaviour
    {
        private const int ResultLine = 1;

        [SerializeField] private SampleLabel label;

        // Static, so the result of a Retry that started in the old scene still shows in the new one.
        private static string lastResults = "Last result: none yet";
        private static int resultsVersion;
        private int shownVersion = -1;

        // Domain reload may be off: start every Play with no result.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetResults()
        {
            lastResults = "Last result: none yet";
            resultsVersion = 0;
        }

        /// <summary>Retry: the open scene group closes and opens again.</summary>
        public async Task<SceneChangeResult> RetryOnce()
        {
            SceneChangeResult result = await SceneActions.Retry();
            ShowResults("Retry: " + result);
            return result;
        }

        /// <summary>Two Retries in one frame: the second is refused as Busy.</summary>
        public async Task<SceneChangeResult[]> RetryTwiceFast()
        {
            Task<SceneChangeResult> first = SceneActions.Retry();
            Task<SceneChangeResult> second = SceneActions.Retry();
            SceneChangeResult[] results = await Task.WhenAll(first, second);
            ShowResults("Retry twice fast: first " + results[0] + ", second " + results[1]);
            return results;
        }

        private void Update()
        {
            if (shownVersion == resultsVersion || label == null)
            {
                return;
            }
            label.SetLine(ResultLine, lastResults);
            shownVersion = resultsVersion;
        }

        private void OnGUI()
        {
            Rect area = new Rect(Screen.width - 290f, 10f, 280f, 120f);
            GUILayout.BeginArea(area);
            if (GUILayout.Button("Retry"))
            {
                RunAndForget(RetryOnce());
            }
            if (GUILayout.Button("Retry twice fast"))
            {
                RunAndForget(RetryTwiceFast());
            }
            GUILayout.EndArea();
        }

        private static void ShowResults(string _text)
        {
            lastResults = "Last result: " + _text;
            resultsVersion++;
        }

        // Button clicks cannot await; an exception still reaches the Console.
        private static async void RunAndForget(Task _retry)
        {
            await _retry;
        }
    }
}
