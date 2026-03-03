using System.Collections;

namespace AbstractPixel.Core
{
    public interface IAnimationCommand
    {
        public string AnimationName { get; set; }
        public IEnumerator ExecuteAnimation();
    }
}