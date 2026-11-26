using System;
using System.Collections.Generic;
using System.Linq;

namespace AbstractPixel.GameManagement
{
    /// <summary>Decides which states are active and what effects apply. Plain C#: build it with a catalog, a clock and an effect sink.</summary>
    /// <remarks>
    /// - The stack holds at most one main state at the bottom and its open sub-states above it. The last one is the top state.
    /// - Requests and leaves made while a step runs (for example from a Changed handler) wait in a queue and run in order.
    /// - Effects are worked out from the stack each time, never saved and restored.
    /// </remarks>
    public class GameStateMachine
    {
        private enum StepKind { Request, Leave, LeaveOnSceneChange, SkipDelay }

        private struct Step
        {
            public StepKind Kind;
            public StateSO State;
            public string Requester;
        }

        private readonly GameManagementSettings settings;
        private readonly IGameClock clock;
        private readonly IGameEffectSink sink;
        private readonly List<StateSO> stack = new List<StateSO>();
        private readonly Queue<Step> queue = new Queue<Step>();
        private bool isRunningSteps;
        private double delayEndsAt = -1;
        private GameEffects lastApplied;

        /// <summary>Every step: entered, left, applied or refused (with the reason). Repeats and leaves of inactive states raise nothing.</summary>
        public event Action<TransitionRecord> Changed;

        /// <summary>The top state's effects were applied: its delay ended, or a sub-state above it closed.</summary>
        public event Action<StateSO> Applied;

        public TransitionJournal Journal { get; }

        public GameStateMachine(GameManagementSettings _settings, IGameClock _clock, IGameEffectSink _sink)
        {
            settings = _settings;
            clock = _clock;
            sink = _sink;
            Journal = new TransitionJournal(Math.Max(8, _settings.JournalSize));
        }

        /// <summary>The state in control, or null when none is active.</summary>
        public StateSO Top
        {
            get { return stack.Count == 0 ? null : stack[stack.Count - 1]; }
        }

        /// <summary>The active states, bottom (main state) to top.</summary>
        public IReadOnlyList<StateSO> Stack
        {
            get { return stack; }
        }

        /// <summary>True while the top state waits for its execution delay before applying effects.</summary>
        public bool IsInDelay
        {
            get { return delayEndsAt >= 0; }
        }

        /// <summary>True when this machine was built from the given settings asset.</summary>
        public bool UsesSettings(GameManagementSettings _settings)
        {
            return settings == _settings;
        }

        public bool IsActive(StateSO _state)
        {
            return stack.Contains(_state);
        }

        public void Request(StateSO _state, string _requester = "")
        {
            Enqueue(new Step { Kind = StepKind.Request, State = _state, Requester = _requester });
        }

        public void Leave(StateSO _state, string _requester = "")
        {
            Enqueue(new Step { Kind = StepKind.Leave, State = _state, Requester = _requester });
        }

        /// <summary>Called on a scene change: states marked to leave on scene change leave, and a pending delay is cancelled with them.</summary>
        public void LeaveOnSceneChange()
        {
            Enqueue(new Step { Kind = StepKind.LeaveOnSceneChange, Requester = "Scene change" });
        }

        /// <summary>Ends the top state's execution delay now.</summary>
        public void SkipDelay()
        {
            Enqueue(new Step { Kind = StepKind.SkipDelay, Requester = "Skip delay" });
        }

        /// <summary>Call every frame: applies the top state's effects once its delay is over.</summary>
        public void Tick()
        {
            if (IsInDelay && clock.Now >= delayEndsAt)
            {
                SkipDelay();
            }
        }

        private void Enqueue(Step _step)
        {
            queue.Enqueue(_step);
            if (isRunningSteps)
            {
                return;
            }

            isRunningSteps = true;
            try
            {
                while (queue.Count > 0)
                {
                    Run(queue.Dequeue());
                }
            }
            finally
            {
                isRunningSteps = false;
            }
        }

        private void Run(Step _step)
        {
            switch (_step.Kind)
            {
                case StepKind.Request:
                    RunRequest(_step.State, _step.Requester);
                    break;
                case StepKind.Leave:
                    RunLeave(_step.State, _step.Requester);
                    break;
                case StepKind.LeaveOnSceneChange:
                    RunLeaveOnSceneChange(_step.Requester);
                    break;
                case StepKind.SkipDelay:
                    if (IsInDelay)
                    {
                        delayEndsAt = -1;
                        ApplyEffects(_step.Requester);
                    }
                    break;
            }
        }

        private void RunRequest(StateSO _state, string _requester)
        {
            if (_state == null)
            {
                Refuse(null, _requester, "no state was given");
                return;
            }
            if (IsActive(_state))
            {
                return;
            }
            if (settings.Catalog == null || !settings.Catalog.Contains(_state))
            {
                Refuse(_state, _requester, $"{_state.name} is not in the state catalog");
                return;
            }
            if (IsInDelay)
            {
                Refuse(_state, _requester, $"{Top.name} is in its execution delay");
                return;
            }

            bool isSubState = _state.Parents.Count > 0;
            if (isSubState)
            {
                if (Top == null || !_state.Parents.Contains(Top))
                {
                    string parents = string.Join(" or ", _state.Parents.Where(_parent => _parent != null).Select(_parent => _parent.name));
                    Refuse(_state, _requester, $"{_state.name} needs {parents} on top");
                    return;
                }
            }
            else if (stack.Count > 0)
            {
                StateSO main = stack[0];
                if (settings.Catalog.Outranks(main, _state))
                {
                    Refuse(_state, _requester, $"{main.name} is higher");
                    return;
                }
                CloseFrom(0, _requester);
            }

            Enter(_state, _requester);
        }

        private void RunLeave(StateSO _state, string _requester)
        {
            int index = stack.IndexOf(_state);
            if (index < 0)
            {
                return;
            }
            CloseFrom(index, _requester);
            ApplyEffects(_requester);
        }

        private void RunLeaveOnSceneChange(string _requester)
        {
            int firstToLeave = stack.FindIndex(_state => _state.DisableStateOnSceneChange);
            if (firstToLeave < 0)
            {
                return;
            }
            CloseFrom(firstToLeave, _requester);
            ApplyEffects(_requester);
        }

        /// <summary>Closes the stack from the given index up, top down, one journaled step each.</summary>
        private void CloseFrom(int _index, string _requester)
        {
            while (stack.Count > _index)
            {
                StateSO leaving = Top;
                StateSO[] before = stack.ToArray();
                stack.RemoveAt(stack.Count - 1);
                delayEndsAt = -1;
                Record(TransitionKind.Left, leaving, _requester, string.Empty, before, lastApplied, lastApplied);
            }
        }

        private void Enter(StateSO _state, string _requester)
        {
            StateSO[] before = stack.ToArray();
            stack.Add(_state);
            Record(TransitionKind.Entered, _state, _requester, string.Empty, before, lastApplied, lastApplied);

            if (_state.StateExecutionDelay > 0f)
            {
                delayEndsAt = clock.Now + _state.StateExecutionDelay;
                return;
            }
            ApplyEffects(_requester);
        }

        private void ApplyEffects(string _requester)
        {
            if (IsInDelay)
            {
                return;
            }

            // "Keep" fields take the last applied value, so a state that keeps everything sends nothing to the sink,
            // but it is still applied: menus that wait for Applied must open.
            GameEffects wanted = EffectResolver.Resolve(stack, settings.NoStateTimeScale);
            GameEffects result = WithKeptValues(wanted, lastApplied);
            if (result.Equals(lastApplied))
            {
                if (Top != null)
                {
                    Record(TransitionKind.Applied, Top, _requester, "no effect changed", stack.ToArray(), lastApplied, lastApplied);
                    Applied?.Invoke(Top);
                }
                return;
            }

            GameEffects before = lastApplied;
            sink.Apply(wanted);
            lastApplied = result;
            Record(TransitionKind.Applied, Top, _requester, string.Empty, stack.ToArray(), before, result);
            if (Top != null)
            {
                Applied?.Invoke(Top);
            }
        }

        private static GameEffects WithKeptValues(GameEffects _wanted, GameEffects _last)
        {
            GameEffects result = _wanted;
            if (!result.TimeScale.HasValue)
            {
                result.TimeScale = _last.TimeScale;
                result.TimeScaleEaseSeconds = _last.TimeScaleEaseSeconds;
                result.TimeScaleEaseCurve = _last.TimeScaleEaseCurve;
            }
            result.Cursor = result.Cursor ?? _last.Cursor;
            result.HudVisible = result.HudVisible ?? _last.HudVisible;
            result.InputMaps = result.InputMaps ?? _last.InputMaps;
            return result;
        }

        private void Refuse(StateSO _state, string _requester, string _reason)
        {
            Record(TransitionKind.Refused, _state, _requester, _reason, stack.ToArray(), lastApplied, lastApplied);
        }

        private void Record(TransitionKind _kind, StateSO _state, string _requester, string _reason, StateSO[] _before, GameEffects _effectsBefore, GameEffects _effectsAfter)
        {
            TransitionRecord record = new TransitionRecord
            {
                Kind = _kind,
                State = _state,
                Requester = _requester,
                Reason = _reason,
                Time = clock.Now,
                StackBefore = _before,
                StackAfter = stack.ToArray(),
                EffectsBefore = _effectsBefore,
                EffectsAfter = _effectsAfter,
            };
            Journal.Add(record);
            Changed?.Invoke(record);
        }
    }
}
