using System.Threading.Tasks;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Samples
{
    /// <summary>
    /// Sample 02: buttons that change the scene group from code with SceneActions.ChangeTo and show the
    /// SceneChangeResult each change returns (Done, or Refused with the reason).
    /// </summary>
    public class Sample02LevelChanger : MonoBehaviour
    {
        private const int ResultLine = 1;

        [SerializeField] private SceneGroup otherGroup;
        [SerializeField] private string otherGroupName = "Arena";
        [SerializeField] private SampleLabel label;

        // Static, so the result of a change that started in the old scene still shows in the new one.
        private static string lastResults = "Last result: none yet";
        private static int resultsVersion;
        private int shownVersion = -1;
        private string changeButtonText;

        // Domain reload may be off: start every Play with no result.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetResults()
        {
            lastResults = "Last result: none yet";
            resultsVersion = 0;
        }

        private void Awake()
        {
            changeButtonText = "Change to " + otherGroupName;
        }

        /// <summary>Changes to the other group and returns its result.</summary>
        public async Task<SceneChangeResult> ChangeToOther()
        {
            SceneChangeResult result = await SceneActions.ChangeTo(otherGroup);
            ShowResults("Change to " + otherGroupName + ": " + result);
            return result;
        }

        /// <summary>Asks for the same change twice in one frame: the first runs, the second is refused as Busy.</summary>
        public async Task<SceneChangeResult[]> ChangeTwiceFast()
        {
            Task<SceneChangeResult> first = SceneActions.ChangeTo(otherGroup);
            Task<SceneChangeResult> second = SceneActions.ChangeTo(otherGroup);
            SceneChangeResult[] results = await Task.WhenAll(first, second);
            ShowResults("Twice fast: first " + results[0] + ", second " + results[1]);
            return results;
        }

        /// <summary>Asks for the group that is already open: refused as AlreadyOpen.</summary>
        public async Task<SceneChangeResult> ChangeToTheOpenGroup()
        {
            SceneChangeResult result = await SceneActions.ChangeTo(SceneActions.ActiveSceneGroup);
            ShowResults("Change to the open group: " + result);
            return result;
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
            Rect area = new Rect(Screen.width - 290f, 10f, 280f, 200f);
            GUILayout.BeginArea(area);
            if (GUILayout.Button(changeButtonText))
            {
                RunAndForget(ChangeToOther());
            }
            if (GUILayout.Button("Change twice fast"))
            {
                RunAndForget(ChangeTwiceFast());
            }
            if (GUILayout.Button("Change to the open group"))
            {
                RunAndForget(ChangeToTheOpenGroup());
            }
            GUILayout.EndArea();
        }

        private static void ShowResults(string _text)
        {
            lastResults = "Last result: " + _text;
            resultsVersion++;
        }

        // Button clicks cannot await; an exception still reaches the Console.
        private static async void RunAndForget(Task _change)
        {
            await _change;
        }
    }
}
