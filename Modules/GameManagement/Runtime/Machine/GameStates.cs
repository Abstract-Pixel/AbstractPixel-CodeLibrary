using System;
using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>The game's state machine, reachable from anywhere. Daily calls: Request, Leave, IsActive, Top, Changed.</summary>
    /// <remarks>
    /// - Builds its machine from the project's settings on first use, or uses the one given to <see cref="Use"/>.
    /// - Until Session 3 adds the Unity effect sink, the built machine applies no effects (it only decides states).
    /// - Resets at the start of every Play session, so Enter Play Mode without a domain reload starts clean.
    /// </remarks>
    public static class GameStates
    {
        private static GameStateMachine machine;

        /// <summary>Every step of the machine, refusals included. Handlers move to a new machine on <see cref="Use"/>.</summary>
        public static event Action<TransitionRecord> Changed;

        public static bool HasMachine
        {
            get { return machine != null; }
        }

        public static GameStateMachine Machine
        {
            get
            {
                if (machine == null)
                {
                    GameManagementSettings settings = GameManagementSettingsSource.Find();
                    if (settings == null)
                    {
                        Debug.LogWarning("[Game management] No GameManagementSettings asset found; state requests are ignored.");
                        return null;
                    }
                    Use(new GameStateMachine(settings, new RealtimeGameClock(), new NoEffectSink()));
                }
                return machine;
            }
        }

        public static StateSO Top
        {
            get { return Machine?.Top; }
        }

        public static void Use(GameStateMachine _machine)
        {
            if (machine != null)
            {
                machine.Changed -= RaiseChanged;
            }
            machine = _machine;
            if (machine != null)
            {
                machine.Changed += RaiseChanged;
            }
        }

        public static void Request(StateSO _state, string _requester = "")
        {
            Machine?.Request(_state, _requester);
        }

        public static void Leave(StateSO _state, string _requester = "")
        {
            Machine?.Leave(_state, _requester);
        }

        public static bool IsActive(StateSO _state)
        {
            return Machine != null && machine.IsActive(_state);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetForNewPlaySession()
        {
            Use(null);
            Changed = null;
        }

        private static void RaiseChanged(TransitionRecord _record)
        {
            Changed?.Invoke(_record);
        }

        private class NoEffectSink : IGameEffectSink
        {
            public void Apply(GameEffects _effects)
            {
            }
        }
    }
}
