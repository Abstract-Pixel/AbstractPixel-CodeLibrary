using AbstractPixel.Core;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// What a scene change request tells its caller when it ends: Done, Refused (with the reason) or Failed (with the
    /// scene that failed to load).
    /// </summary>
    public sealed class SceneChangeResult
    {
        private static readonly SceneChangeResult done = new SceneChangeResult(SceneChangeOutcome.Done, SceneChangeRefusal.None, null);

        public SceneChangeOutcome Outcome { get; }

        /// <summary>Why the change was refused. None unless the outcome is Refused.</summary>
        public SceneChangeRefusal Refusal { get; }

        /// <summary>The scene that failed to load. Null unless the outcome is Failed.</summary>
        public SceneReference FailedScene { get; }

        public bool IsDone
        {
            get { return Outcome == SceneChangeOutcome.Done; }
        }

        private SceneChangeResult(SceneChangeOutcome _outcome, SceneChangeRefusal _refusal, SceneReference _failedScene)
        {
            Outcome = _outcome;
            Refusal = _refusal;
            FailedScene = _failedScene;
        }

        public static SceneChangeResult Done()
        {
            return done;
        }

        public static SceneChangeResult Refused(SceneChangeRefusal _reason)
        {
            return new SceneChangeResult(SceneChangeOutcome.Refused, _reason, null);
        }

        public static SceneChangeResult Failed(SceneReference _failedScene)
        {
            return new SceneChangeResult(SceneChangeOutcome.Failed, SceneChangeRefusal.None, _failedScene);
        }

        public override string ToString()
        {
            if (Outcome == SceneChangeOutcome.Refused)
            {
                return "Refused (" + Refusal + ")";
            }
            if (Outcome == SceneChangeOutcome.Failed)
            {
                string sceneName = FailedScene == null ? "unknown scene" : FailedScene.SceneName;
                return "Failed (" + sceneName + ")";
            }
            return "Done";
        }
    }
}
