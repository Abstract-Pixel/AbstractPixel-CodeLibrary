using System.Threading.Tasks;
using UnityEngine;
public interface ITransitionController
{
    GameObject gameObject { get; }
    public Task PlayTransitionIn();
    public Task PlayTransitionOut();
}