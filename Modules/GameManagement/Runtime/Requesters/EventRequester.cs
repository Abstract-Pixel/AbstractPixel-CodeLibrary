using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>Sends its request when <see cref="Raise"/> is called from a UnityEvent, an animation event or a script.</summary>
    [AddComponentMenu("Abstract Pixel/Game Management/Event Requester")]
    public class EventRequester : StateRequesterBase
    {
        public void Raise()
        {
            Fire();
        }
    }
}
