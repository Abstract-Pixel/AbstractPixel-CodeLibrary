using System.Threading.Tasks;
using AbstractPixel.SceneManagement.SceneScreens;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Samples
{
    /// <summary>
    /// Sample 06: a loading bar made of two cubes. The LoadingProgressView on the same object drives it: Show, then
    /// SetProgress every frame (the inner cube grows from the left), PlayFull once every scene is open, then Hide.
    /// </summary>
    public class Sample06BarDisplay : MonoBehaviour, ILoadingProgressDisplay
    {
        private const float FlashSeconds = 0.25f;

        [SerializeField] private Transform bar;
        [SerializeField] private Renderer barRenderer;
        [SerializeField] private float fullWidth = 6f;
        [SerializeField] private Color flashColor = new Color(1f, 1f, 0.85f);

        private MaterialPropertyBlock flashBlock;
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        public float ShownProgress { get; private set; }

        public void Show()
        {
            bar.gameObject.SetActive(true);
            SetProgress(0f);
        }

        public void SetProgress(float _progress)
        {
            ShownProgress = _progress;
            float width = Mathf.Max(0.01f, fullWidth * _progress);
            Vector3 scale = bar.localScale;
            scale.x = width;
            bar.localScale = scale;
            // Grows from the left edge of the frame.
            Vector3 position = bar.localPosition;
            position.x = (width - fullWidth) * 0.5f;
            bar.localPosition = position;
        }

        /// <summary>The "full" beat: the bar flashes once.</summary>
        public async Task PlayFull()
        {
            if (flashBlock == null)
            {
                flashBlock = new MaterialPropertyBlock();
            }
            flashBlock.SetColor(ColorId, flashColor);
            barRenderer.SetPropertyBlock(flashBlock);
            await WaitUnscaled(FlashSeconds);
            if (barRenderer != null)
            {
                barRenderer.SetPropertyBlock(null);
            }
        }

        public void Hide()
        {
            if (bar != null)
            {
                bar.gameObject.SetActive(false);
            }
        }

        private static async Task WaitUnscaled(float _seconds)
        {
            double endTime = Time.realtimeSinceStartupAsDouble + _seconds;
            while (Time.realtimeSinceStartupAsDouble < endTime)
            {
                await Task.Yield();
            }
        }
    }
}
