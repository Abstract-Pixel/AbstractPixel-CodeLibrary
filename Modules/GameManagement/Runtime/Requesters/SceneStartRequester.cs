using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>Requests its state when the scene starts, for example Play in a level or MainMenu in the menu scene.</summary>
    [AddComponentMenu("Abstract Pixel/Game Management/Scene Start Requester")]
    public class SceneStartRequester : StateRequesterBase
    {
        private void Start()
        {
            Fire();
        }
    }
}
