using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AbstractPixel.GameManagement.Debugging
{
    /// <summary>Everything the overlay writes, read from a machine at one moment.</summary>
    internal class DebuggerSnapshot
    {
        public const string Unset = "—";
        private const string NoName = "(no state)";
        private const int JournalRecords = 8;
        private const double SameMomentSeconds = 0.005;

        public readonly List<DebuggerStackEntry> Stack = new List<DebuggerStackEntry>();
        public readonly List<DebuggerJournalGroup> Journal = new List<DebuggerJournalGroup>();
        public string Time = Unset;
        public string Cursor = Unset;
        public string Hud = Unset;
        public string Input = Unset;

        /// <summary>One sentence, or null when nothing in the journal was refused.</summary>
        public string LastRefusal;

        /// <summary>A null machine gives an empty overlay.</summary>
        public static DebuggerSnapshot From(GameStateMachine _machine, GameStateCatalog _catalog)
        {
            DebuggerSnapshot snapshot = new DebuggerSnapshot();
            if (_machine == null)
            {
                return snapshot;
            }

            snapshot.ReadStack(_machine, _catalog);
            snapshot.ReadEffects(_machine.AppliedEffects);
            List<TransitionRecord> records = _machine.Journal.Records.ToList();
            snapshot.ReadLastRefusal(records);
            snapshot.ReadJournal(records, _catalog);
            return snapshot;
        }

        private void ReadStack(GameStateMachine _machine, GameStateCatalog _catalog)
        {
            for (int i = 0; i < _machine.Stack.Count; i++)
            {
                StateSO state = _machine.Stack[i];
                Stack.Add(new DebuggerStackEntry
                {
                    Name = NameOf(state),
                    Color = StateColors.IndexFor(state, _catalog),
                    IsTop = i == _machine.Stack.Count - 1,
                });
            }
        }

        private void ReadEffects(GameEffects _effects)
        {
            if (_effects.TimeScale.HasValue)
            {
                Time = _effects.TimeScale.Value.ToString("0.##", CultureInfo.InvariantCulture);
            }
            if (_effects.Cursor.HasValue)
            {
                Cursor = NameOf(_effects.Cursor.Value);
            }
            if (_effects.HudVisible.HasValue)
            {
                Hud = _effects.HudVisible.Value ? "Shown" : "Hidden";
            }
            if (_effects.InputMaps != null)
            {
                string names = string.Join(" + ", _effects.InputMaps.Where(_map => _map != null).Select(_map => _map.name));
                Input = names.Length == 0 ? "None" : names;
            }
        }

        private void ReadLastRefusal(List<TransitionRecord> _records)
        {
            TransitionRecord refusal = _records.LastOrDefault(_record => _record.Kind == TransitionKind.Refused);
            if (refusal == null)
            {
                return;
            }

            string requester = string.IsNullOrEmpty(refusal.Requester) ? string.Empty : $" ({refusal.Requester})";
            LastRefusal = $"{NameOf(refusal.State)} refused{requester}: {refusal.Reason.TrimEnd('.')}.";
        }

        private void ReadJournal(List<TransitionRecord> _records, GameStateCatalog _catalog)
        {
            DebuggerJournalGroup group = null;
            double groupTime = 0;
            int oldestShown = Math.Max(0, _records.Count - JournalRecords);
            for (int i = _records.Count - 1; i >= oldestShown; i--)
            {
                TransitionRecord record = _records[i];
                if (group == null || Math.Abs(groupTime - record.Time) >= SameMomentSeconds)
                {
                    groupTime = record.Time;
                    group = new DebuggerJournalGroup
                    {
                        Number = Journal.Count + 1,
                        Time = record.Time.ToString("0.000", CultureInfo.InvariantCulture),
                    };
                    Journal.Add(group);
                }

                DebuggerJournalLine line = group.Lines.Find(_line => _line.State == record.State);
                if (line == null)
                {
                    line = new DebuggerJournalLine
                    {
                        State = record.State,
                        Name = NameOf(record.State),
                        Color = StateColors.IndexFor(record.State, _catalog),
                    };
                    group.Lines.Add(line);
                }
                // Lines are found newest first, but each line's icons read in the order things happened.
                line.Events.Insert(0, record.Kind);
            }
        }

        private static string NameOf(GameCursorMode _cursor)
        {
            switch (_cursor)
            {
                case GameCursorMode.LockedHidden:
                    return "Locked, hidden";
                case GameCursorMode.LockedVisible:
                    return "Locked, visible";
                case GameCursorMode.FreeHidden:
                    return "Free, hidden";
                default:
                    return "Free";
            }
        }

        private static string NameOf(StateSO _state)
        {
            return _state == null ? NoName : _state.name;
        }
    }
}
