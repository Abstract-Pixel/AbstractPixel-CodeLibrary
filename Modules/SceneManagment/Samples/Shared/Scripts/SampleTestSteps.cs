using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Samples
{
    /// <summary>
    /// Steps the sample PlayMode tests share: wait until start-up has opened its first scene group, then open the
    /// sample's own scene group with SceneActions.ChangeTo, with a timeout. Used from coroutines ([UnityTest]).
    /// </summary>
    public static class SampleTestSteps
    {
        public const float DefaultTimeoutSeconds = 60f;

        /// <summary>Waits until SceneActions.HasGameStarted, or until the timeout; fills the result's TimedOut.</summary>
        public static IEnumerator WaitUntilGameStarted(SampleStepResult _result)
        {
            float startTime = Time.realtimeSinceStartup;
            while (SceneActions.HasGameStarted == false)
            {
                bool timedOut = Time.realtimeSinceStartup - startTime > DefaultTimeoutSeconds;
                if (timedOut)
                {
                    _result.TimedOut = true;
                    _result.Problem = "start-up did not finish within " + DefaultTimeoutSeconds + " s";
                    yield break;
                }
                yield return null;
            }
        }

        /// <summary>Waits for start-up, then changes to the group and waits for the result.</summary>
        public static IEnumerator OpenGroup(SceneGroup _group, SampleStepResult _result)
        {
            yield return WaitUntilGameStarted(_result);
            if (_result.TimedOut)
            {
                yield break;
            }
            if (SceneActions.ActiveSceneGroup == _group)
            {
                _result.Result = SceneChangeResult.Done();
                yield break;
            }
            Task<SceneChangeResult> change = SceneActions.ChangeTo(_group);
            yield return WaitFor(change, _result);
        }

        /// <summary>Waits for a scene change task, or until the timeout.</summary>
        public static IEnumerator WaitFor(Task<SceneChangeResult> _change, SampleStepResult _result)
        {
            float startTime = Time.realtimeSinceStartup;
            while (_change.IsCompleted == false)
            {
                bool timedOut = Time.realtimeSinceStartup - startTime > DefaultTimeoutSeconds;
                if (timedOut)
                {
                    _result.TimedOut = true;
                    _result.Problem = "the scene change did not end within " + DefaultTimeoutSeconds + " s";
                    yield break;
                }
                yield return null;
            }
            if (_change.IsFaulted)
            {
                _result.Problem = "the scene change threw: " + _change.Exception;
                yield break;
            }
            _result.Result = _change.Result;
        }
    }
}
