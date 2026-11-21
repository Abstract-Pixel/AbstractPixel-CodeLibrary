using System;
using System.Threading.Tasks;
using UnityEngine;

namespace AbstractPixel.SceneTransitions
{
    /// <summary>One screen effect, such as a fade. The <see cref="TransitionManager"/> creates it from a transition profile.</summary>
    public interface ITransitionController
    {
        GameObject gameObject { get; }

        public void Initialize(TransitionProfile _transitionProfile);

        /// <summary>Covers the screen. Finishes once the screen counts as covered; the effect may keep finishing on top.</summary>
        public Task PlayHideScreen();

        /// <summary>
        /// Uncovers the screen. Finishes once the effect has ended. Calls &lt;paramref name="_screenCountsAsVisible"/&gt;
        /// once, at the profile's "Screen counts as visible at" share (it may be null).
        /// </summary>
        public Task PlayShowScreen(Action _screenCountsAsVisible);
    }
}
