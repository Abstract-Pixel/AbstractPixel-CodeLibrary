using System;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// A hidden object in a newly loaded scene. One frame after the scene's scripts start (so every Savable Bridge in
    /// the scene has registered and restored), it reports the scene as ready and removes itself. Restoring never waits
    /// for this; only the load event does.
    /// </summary>
    [AddComponentMenu("")]
    internal sealed class SceneReadySignal : MonoBehaviour
    {
        public string SceneName;
        public Action<string> Ready;

        int startFrame;

        void Start()
        {
            startFrame = Time.frameCount;
        }

        // Unity calls each script's Start just before that script's first update, so in the scene's first frame
        // some scripts may not have started yet. By the next frame's Update every queued Start has run.
        void Update()
        {
            bool isStillTheFirstFrame = Time.frameCount <= startFrame;
            if (isStillTheFirstFrame)
            {
                return;
            }
            Ready?.Invoke(SceneName);
            Ready = null;
            Destroy(gameObject);
        }
    }
}
