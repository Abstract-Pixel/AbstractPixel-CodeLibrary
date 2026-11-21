using System.Threading.Tasks;

namespace AbstractPixel.SceneManagement.SceneScreens
{
    /// <summary>
    /// A look for the loading progress view written in code, such as an animation command sequence. The view finds one
    /// on its own object or uses the one assigned, and drives it: Show when Show screen starts, SetProgress with the
    /// smoothed value every frame, PlayFull once every scene is open (the view waits for its task), Hide when the
    /// loading scene closes.
    /// </summary>
    public interface ILoadingProgressDisplay
    {
        void Show();

        /// <summary>The smoothed loading progress, from 0 to 1.</summary>
        void SetProgress(float _progress);

        /// <summary>The "full" beat. Hide screen waits until this task ends.</summary>
        Task PlayFull();

        void Hide();
    }
}
