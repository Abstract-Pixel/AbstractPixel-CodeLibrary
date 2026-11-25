using AbstractPixel.Core;

namespace AbstractPixel.SceneManagement
{
    /// <summary>What a scene change request tells its caller when it ends: Done, Refused or Failed.</summary>
    /// <remarks>
    /// - Refused carries the reason in Refusal; Failed carries the scene that failed to load in FailedScene.
    /// - ToString gives "Done", "Refused (Busy)" or "Failed (scene name)", ready for a label or a log.
    /// </remarks>
    public sealed class SceneChangeResult
    {
        private static readonly SceneChangeResult done = new SceneChangeResult(SceneChangeOutcome.Done, SceneChangeRefusal.None, null);

        /// <summary>How the request ended: Done, Refused or Failed.</summary>
        public SceneChangeOutcome Outcome { get; }

        /// <summary>Why the change was refused. None unless the outcome is Refused.</summary>
        public SceneChangeRefusal Refusal { get; }

        /// <summary>The scene that failed to load. Null unless the outcome is Failed.</summary>
        public SceneReference FailedScene { get; }

        /// <summary>True when the outcome is Done.</summary>
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

        /// <summary>The result of a change that ran to its end. The scene system makes it; you only read it.</summary>
        public static SceneChangeResult Done()
        {
            return done;
        }

        /// <summary>The result of a refused request, with the reason. Made by the scene system.</summary>
        public static SceneChangeResult Refused(SceneChangeRefusal _reason)
        {
            return new SceneChangeResult(SceneChangeOutcome.Refused, _reason, null);
        }

        /// <summary>The result of a change in which a scene failed to load, with that scene. Made by the scene system.</summary>
        public static SceneChangeResult Failed(SceneReference _failedScene)
        {
            return new SceneChangeResult(SceneChangeOutcome.Failed, SceneChangeRefusal.None, _failedScene);
        }

        /// <summary>"Done", "Refused (reason)" or "Failed (scene name)"; "unknown scene" when the scene is missing.</summary>
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
