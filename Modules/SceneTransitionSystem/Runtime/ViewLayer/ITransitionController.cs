using System.Threading.Tasks;
using UnityEngine;

namespace AbstractPixel.SceneTransitions
{
    public interface ITransitionController
    {
        GameObject gameObject { get; }

        public void Initialize(TransitionProfile _transitionProfile);
        public Task PlayTransitionIn();
        public Task PlayTransitionOut();
    }
}