using System.Threading.Tasks;
using UnityEngine;

namespace AbstractPixel.SceneManagement.SceneScreens
{
    /// <summary>
    /// The ready-made loading scene's plain progress bar: the fill's right edge follows the smoothed progress. It uses
    /// the RectTransform's anchors, so it needs no sprite and no art.
    /// </summary>
    [DisallowMultipleComponent]
    public class SimpleLoadingProgressBar : MonoBehaviour, ILoadingProgressDisplay
    {
        [Tooltip("The fill, stretched inside the bar's background; its right anchor follows the progress.")]
        [SerializeField] private RectTransform fill;

        public void Show()
        {
            SetProgress(0f);
        }

        public void SetProgress(float _progress)
        {
            if (fill == null)
            {
                return;
            }
            Vector2 anchorMax = fill.anchorMax;
            anchorMax.x = Mathf.Clamp01(_progress);
            fill.anchorMax = anchorMax;
        }

        public Task PlayFull()
        {
            SetProgress(1f);
            return Task.CompletedTask;
        }

        public void Hide()
        {
        }
    }
}
