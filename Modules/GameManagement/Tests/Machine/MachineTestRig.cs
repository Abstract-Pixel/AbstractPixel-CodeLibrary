using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.GameManagement.Tests
{
    /// <summary>Builds Rally's example catalog (DESIGN.md) with a fake clock and a recording sink, and cleans it up.</summary>
    public class MachineTestRig
    {
        public readonly FakeGameClock Clock = new FakeGameClock();
        public readonly RecordingEffectSink Sink = new RecordingEffectSink();
        public readonly GameManagementSettings Settings;
        public readonly GameStateCatalog Catalog;
        public readonly List<TransitionRecord> Records = new List<TransitionRecord>();

        // Catalog order, top to bottom: GameOver, GameWin, Settings, Pause, MainMenu, Play.
        public readonly StateSO GameOver;
        public readonly StateSO GameWin;
        public readonly StateSO Settings_;
        public readonly StateSO Pause;
        public readonly StateSO MainMenu;
        public readonly StateSO Play;

        private readonly List<Object> created = new List<Object>();

        public GameStateMachine Machine { get; private set; }

        public MachineTestRig()
        {
            GameOver = MakeState("GameOver", 0f);
            GameWin = MakeState("GameWin", 0f);
            MainMenu = MakeState("MainMenu", 0f);
            Play = MakeState("Play", 1f);
            Pause = MakeState("Pause", 0f);
            Settings_ = MakeState("Settings", 0f);
            Pause.Parents.Add(Play);
            Settings_.Parents.Add(MainMenu);
            Settings_.Parents.Add(Pause);
            Settings_.Effects.TimeScaleSource = EffectSource.SameAsParent;

            Catalog = Track(ScriptableObject.CreateInstance<GameStateCatalog>());
            Catalog.States.AddRange(new[] { GameOver, GameWin, Settings_, Pause, MainMenu, Play });

            Settings = Track(ScriptableObject.CreateInstance<GameManagementSettings>());
            Settings.Catalog = Catalog;
            Rebuild();
        }

        /// <summary>Makes a new machine from the current settings (after a test changed them).</summary>
        public void Rebuild()
        {
            Machine = new GameStateMachine(Settings, Clock, Sink);
            Records.Clear();
            Machine.Changed += Records.Add;
        }

        public StateSO MakeState(string _name, float _timeScale)
        {
            StateSO state = Track(ScriptableObject.CreateInstance<StateSO>());
            state.name = _name;
            state.Effects.TimeScaleSource = EffectSource.Own;
            state.Effects.TimeScale = _timeScale;
            return state;
        }

        public void Dispose()
        {
            foreach (Object item in created)
            {
                Object.DestroyImmediate(item);
            }
        }

        private T Track<T>(T _item) where T : Object
        {
            created.Add(_item);
            return _item;
        }
    }
}
