using System.Collections;
using UnityEngine;

public interface IAnimationCommand
{
    public string AnimationName {  get; set; }
    public IEnumerator ExecuteAnimation();
}
