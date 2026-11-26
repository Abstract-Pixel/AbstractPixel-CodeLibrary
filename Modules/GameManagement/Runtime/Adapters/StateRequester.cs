using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>For UnityEvent buttons: drag it in and pick Enter, Leave or Toggle with the state as the argument.</summary>
    [AddComponentMenu("Abstract Pixel/Game Management/State Requester")]
    public class StateRequester : MonoBehaviour
    {
        public void Enter(StateSO _state)
        {
            StateRequests.Perform(_state, StateRequestKind.Enter, name);
        }

        public void Leave(StateSO _state)
        {
            StateRequests.Perform(_state, StateRequestKind.Leave, name);
        }

        public void Toggle(StateSO _state)
        {
            StateRequests.Perform(_state, StateRequestKind.Toggle, name);
        }
    }
}
