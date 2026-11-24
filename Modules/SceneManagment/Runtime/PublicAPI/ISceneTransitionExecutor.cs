using System;
using System.Threading.Tasks;

namespace AbstractPixel.SceneManagement
{
    /// <summary>The one thing the scene system asks to hide and show the screen around a scene change (ADR 0001).</summary>
    /// <remarks>
    /// - The transitions module provides it and registers it with <see cref="SceneActions.RegisterTransitionExecutor"/>.
    /// - With none registered, scenes change with no effect.
    /// - The scene system calls Hide screen only after it accepted a change, so a refused change never covers the screen.
    /// </remarks>
    public interface ISceneTransitionExecutor
    {
        /// <summary>Hide screen: finishes once the screen counts as covered.</summary>
        Task TransitionToHideScreen();

        /// <summary>Show screen: finishes once the screen is uncovered.</summary>
        /// <remarks>
        /// - Calls <paramref name="_screenCountsAsVisible"/> once, at the transition profile's "Screen counts as visible at" share, so the scene system can release relocatables then.
        /// - The argument may be null.
        /// - An executor that never calls it is released at the end of Show screen.
        /// </remarks>
        Task TransitionToShowScreen(Action _screenCountsAsVisible);
    }
}
