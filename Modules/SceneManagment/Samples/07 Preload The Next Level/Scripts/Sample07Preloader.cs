using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Samples
{
    /// <summary>
    /// Sample 07: preload the next scene group in the background, then change to it. Shows the preloaded group, the two
    /// preload messages as they arrive, and how long the change took.
    /// </summary>
    public class Sample07Preloader : MonoBehaviour
    {
        private const int PreloadedLine = 1;
        private const int MessagesLine = 2;
        private const int TimeLine = 3;

        [SerializeField] private SceneGroup nextGroup;
        [SerializeField] private string nextName = "Next Level";
        [SerializeField] private SampleLabel label;

        // Static, so what happened in the old scene still shows in the new one.
        private static string messages = "Messages: none yet";
        private static string changeTime = string.Empty;
        private static int version;
        private int shownVersion = -1;
        private SceneGroup shownPreloaded;
        private string preloadButtonText;
        private string goButtonText;

        // Domain reload may be off: start every Play empty.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetShown()
        {
            messages = "Messages: none yet";
            changeTime = string.Empty;
            version = 0;
        }

        private void Awake()
        {
            preloadButtonText = "Preload " + nextName;
            goButtonText = "Go to " + nextName;
        }

        private void OnEnable()
        {
            SceneEventBus.OnSceneGroupPreloaded += OnPreloaded;
            SceneEventBus.OnPreloadedSceneGroupActivated += OnActivated;
        }

        private void OnDisable()
        {
            SceneEventBus.OnSceneGroupPreloaded -= OnPreloaded;
            SceneEventBus.OnPreloadedSceneGroupActivated -= OnActivated;
        }

        /// <summary>Opens the next group in the background; only its main scene waits, held back.</summary>
        public Task<SceneChangeResult> PreloadNext()
        {
            return SceneActions.Preload(nextGroup);
        }

        /// <summary>Changes to the next group and measures how long the change took.</summary>
        public async Task<SceneChangeResult> GoToNext()
        {
            Stopwatch watch = Stopwatch.StartNew();
            SceneChangeResult result = await SceneActions.ChangeTo(nextGroup);
            watch.Stop();
            changeTime = "ChangeTo took " + watch.ElapsedMilliseconds + " ms (" + result + ")";
            version++;
            return result;
        }

        private void OnPreloaded(SceneGroup _group)
        {
            AddMessage("OnSceneGroupPreloaded (" + _group.name + ")");
        }

        private void OnActivated(SceneGroup _group)
        {
            AddMessage("OnPreloadedSceneGroupActivated (" + _group.name + ")");
        }

        private static void AddMessage(string _message)
        {
            bool isFirst = version == 0 || messages == NoMessages;
            if (isFirst)
            {
                messages = "Messages:";
            }
            messages = messages + "\n" + _message;
            version++;
        }

        private const string NoMessages = "Messages: none yet";

        private void Update()
        {
            bool preloadedChanged = SceneActions.PreloadedSceneGroup != shownPreloaded;
            if (shownVersion == version && preloadedChanged == false)
            {
                return;
            }
            if (label == null)
            {
                return;
            }
            shownPreloaded = SceneActions.PreloadedSceneGroup;
            string preloadedName = "none";
            if (shownPreloaded != null)
            {
                preloadedName = shownPreloaded.name;
            }
            label.SetLine(PreloadedLine, "PreloadedSceneGroup: " + preloadedName);
            label.SetLine(MessagesLine, messages);
            label.SetLine(TimeLine, changeTime);
            shownVersion = version;
        }

        private void OnGUI()
        {
            Rect area = new Rect(Screen.width - 290f, 10f, 280f, 80f);
            GUILayout.BeginArea(area);
            if (GUILayout.Button(preloadButtonText))
            {
                RunAndForget(PreloadNext());
            }
            if (GUILayout.Button(goButtonText))
            {
                RunAndForget(GoToNext());
            }
            GUILayout.EndArea();
        }

        // Button clicks cannot await; an exception still reaches the Console.
        private static async void RunAndForget(Task _task)
        {
            await _task;
        }
    }
}
