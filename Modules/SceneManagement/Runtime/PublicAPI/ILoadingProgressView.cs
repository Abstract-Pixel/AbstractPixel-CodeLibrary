using System.Threading.Tasks;

namespace AbstractPixel.SceneManagement
{
    /// <summary>The part of a loading scene that shows loading progress, as the scene system sees it.</summary>
    /// <remarks>
    /// - The loading scene registers one with <see cref="SceneActions.RegisterLoadingProgressView"/>.
    /// - The scene system tells it when Show screen starts (or at the cut), so it starts its appear animation then.
    /// - The scene system waits for its "full" animation before Hide screen, so the effect and the loading scene never animate over each other.
    /// </remarks>
    public interface ILoadingProgressView
    {
        /// <summary>Show screen is starting on the loading scene (or the instant cut happened): play the appear animation.</summary>
        void OnShowStarted();

        /// <summary>Every scene is open: play the "full" animation and any hold; the task ends when Hide screen may start.</summary>
        Task PlayFullAnimation();
    }
}
