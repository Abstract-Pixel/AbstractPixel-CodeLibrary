using UnityEngine;
using UnityEngine.Playables;

namespace AbstractPixel.SceneManagement.SceneScreens
{
    /// <summary>
    /// Caps how far a launch scene's Timeline may advance in one frame, so a load hitch (the start scene group opening
    /// behind it) delays the picture instead of skipping it. The director keeps its own playback, so markers fire and
    /// audio clips play normally; on a long frame the clock only slows the root playable for that frame
    /// (speed = cap / frame time). Call Tick from Update, which runs before the director's own update in the same frame.
    /// </summary>
    public sealed class LaunchSceneTimelineClock
    {
        private readonly PlayableDirector timeline;
        private readonly float maximumStepSeconds;

        public LaunchSceneTimelineClock(PlayableDirector _timeline, float _maximumStepSeconds)
        {
            timeline = _timeline;
            maximumStepSeconds = _maximumStepSeconds;
        }

        /// <summary>Lets the Timeline advance this frame, at most the cap.</summary>
        public void Tick()
        {
            float frameSeconds = ReadFrameSeconds();
            double speed = 1d;
            if (frameSeconds > maximumStepSeconds)
            {
                speed = maximumStepSeconds / frameSeconds;
            }

            ApplySpeed(speed);
        }

        /// <summary>Keeps the playhead where it is this frame (for a game's own hold, such as waiting for saved volumes).</summary>
        public void Freeze()
        {
            ApplySpeed(0d);
        }

        // The same delta the director will use for this frame.
        private float ReadFrameSeconds()
        {
            if (timeline.timeUpdateMode == DirectorUpdateMode.UnscaledGameTime)
            {
                return Time.unscaledDeltaTime;
            }

            return Time.deltaTime;
        }

        private void ApplySpeed(double _speed)
        {
            bool hasGraph = timeline != null && timeline.playableGraph.IsValid();
            if (hasGraph == false)
            {
                return;
            }

            timeline.playableGraph.GetRootPlayable(0).SetSpeed(_speed);
        }
    }
}
