using System.Threading.Tasks;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// The one thing the scene system asks to hide and show the screen around a scene change (ADR 0001). The
    /// transitions module provides it and registers it with <see cref="SceneActions.RegisterTransitionExecutor"/>;
    /// with none registered, scenes change with no effect. The scene system calls Hide screen only after it accepted
    /// a change, so a refused change never covers the screen.
    /// </summary>
    public interface ISceneTransitionExecutor
    {
        /// <summary>Hide screen: finishes once the screen counts as covered.</summary>
        Task TransitionToHideScreen();

        /// <summary>Show screen: finishes once the screen is uncovered.</summary>
        Task TransitionToShowScreen();
    }
}
