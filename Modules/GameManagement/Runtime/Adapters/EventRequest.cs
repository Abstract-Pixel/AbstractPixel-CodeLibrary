using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>Sends its request when <see cref="Raise"/> is called from a UnityEvent, an animation event or a script.</summary>
    [AddComponentMenu("Abstract Pixel/Game Management/State Request On Event")]
    public class EventRequest : StateRequestAdapter
    {
        public void Raise()
        {
            Fire();
        }
    }
}
