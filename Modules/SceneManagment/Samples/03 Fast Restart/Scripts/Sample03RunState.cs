using UnityEngine;

namespace AbstractPixel.SceneManagement.Samples
{
    /// <summary>
    /// Sample 03: per-run state in a shared scene. The score rises by 1 each second (unscaled time). The scene group
    /// has Reset shared scenes on, so Retry closes and opens this scene again and the score starts over at 0.
    /// </summary>
    public class Sample03RunState : MonoBehaviour
    {
        private const int ScoreLine = 0;

        private float secondsThisRun;
        private int score;
        private int shownScore = -1;
        private SampleLabel label;

        // Counts Run State objects this Play: a Retry makes a new one, so the number goes up.
        private static int runsThisPlay;
        private int runNumber;

        // Domain reload may be off: count from zero every Play.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRuns()
        {
            runsThisPlay = 0;
        }

        private void Awake()
        {
            runsThisPlay++;
            runNumber = runsThisPlay;
        }

        public int Score
        {
            get { return score; }
        }

        private void Update()
        {
            secondsThisRun += Time.unscaledDeltaTime;
            score = Mathf.FloorToInt(secondsThisRun);
            ShowScore();
        }

        // The label is in the main scene; this shared scene finds it once it is open.
        private void ShowScore()
        {
            if (score == shownScore)
            {
                return;
            }
            if (label == null)
            {
                label = FindAnyObjectByType<SampleLabel>();
            }
            if (label == null)
            {
                return;
            }
            label.SetLine(ScoreLine, "Score this run: " + score + " (Run State scene, run " + runNumber + ")");
            shownScore = score;
        }
    }
}
