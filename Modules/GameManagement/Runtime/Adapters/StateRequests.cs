namespace AbstractPixel.GameManagement
{
    /// <summary>The rules every request adapter shares: what a kind of request does, and when a gate is open.</summary>
    public static class StateRequests
    {
        /// <summary>Sends the request to <see cref="GameStates"/>. Toggle leaves an active state and enters an inactive one.</summary>
        public static void Perform(StateSO _state, StateRequestKind _kind, string _requester)
        {
            if (_state == null)
            {
                UnityEngine.Debug.LogWarning($"[Game management] '{_requester}' has no state assigned, so its request was ignored. Assign a state in its inspector.");
                return;
            }
            bool shouldLeave = _kind == StateRequestKind.Leave
                || (_kind == StateRequestKind.Toggle && GameStates.IsActive(_state));
            if (shouldLeave)
            {
                GameStates.Leave(_state, _requester);
                return;
            }
            GameStates.Request(_state, _requester);
        }

        /// <summary>No gate means always open; otherwise the gate state must be active.</summary>
        public static bool IsGateOpen(StateSO _gate)
        {
            return _gate == null || GameStates.IsActive(_gate);
        }

        /// <summary>Back and cancel only close the state while it is the one on top, so a sub-state above it closes first.</summary>
        public static bool IsOnTop(StateSO _state)
        {
            return _state != null && GameStates.Top == _state;
        }
    }
}
