using AbstractPixel.Core;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// Loading progress, from 0 to 1, for a loading scene to show: every scene the change opens has an equal share, each
    /// scene's own load progress fills its share, the value never goes back, and it is 1 only once every scene is open.
    /// It is worked out when read, with no allocation, so a view can read it every frame. Not smoothed.
    /// </summary>
    internal sealed class SceneLoadingProgress
    {
        // Short of 1 while any scene is still loading, so 1 means "every scene is open".
        private const float HighestValueWhileLoading = 0.99f;

        private readonly SceneOpening opening;
        private readonly List<SceneReference> scenesToOpen = new List<SceneReference>();
        private float lastValue = 0f;

        public SceneLoadingProgress(SceneOpening _opening)
        {
            opening = _opening;
        }

        public float Value
        {
            get
            {
                int sceneCount = scenesToOpen.Count;
                if (sceneCount == 0)
                {
                    return lastValue;
                }

                float filled = 0f;
                bool everySceneOpen = true;
                for (int index = 0; index < sceneCount; index++)
                {
                    SceneReference scene = scenesToOpen[index];
                    if (opening.OpenedScenes.Contains(scene))
                    {
                        filled += 1f;
                        continue;
                    }
                    everySceneOpen = false;
                    filled += Mathf.Clamp01(opening.Loader.GetLoadProgress(scene));
                }

                float value = filled / sceneCount;
                if (!everySceneOpen)
                {
                    value = Mathf.Min(value, HighestValueWhileLoading);
                }
                lastValue = Mathf.Max(lastValue, value);
                return lastValue;
            }
        }

        /// <summary>A new scene change starts from 0.</summary>
        public void Reset()
        {
            scenesToOpen.Clear();
            lastValue = 0f;
        }

        /// <summary>The scenes this plan opens. A fallback replaces them, and the value still never goes back.</summary>
        public void Track(SceneChangePlan _plan)
        {
            scenesToOpen.Clear();
            scenesToOpen.AddRange(_plan.ManagerialToOpen);
            scenesToOpen.AddRange(_plan.ContextualToOpen);
            if (_plan.MainSceneToOpen != null)
            {
                scenesToOpen.Add(_plan.MainSceneToOpen);
            }
        }

        /// <summary>Every scene is open.</summary>
        public void Complete()
        {
            scenesToOpen.Clear();
            lastValue = 1f;
        }
    }
}
