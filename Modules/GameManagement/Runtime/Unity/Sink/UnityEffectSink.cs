namespace AbstractPixel.GameManagement
{
    /// <summary>Applies the machine's effects to the game: time scale (eased in realtime), cursor, HUD and input maps.</summary>
    /// <remarks>
    /// - Time eases from its value at apply time, so a script that set 0.6 just before Game Over eases from 0.6.
    /// - The cursor goes through <see cref="GameCursorController"/>, which still hides it while a controller is used.
    /// - With no input source the input maps are left alone.
    /// - <see cref="GameStateDriver"/> calls <see cref="Step"/> each frame with unscaled time.
    /// </remarks>
    public class UnityEffectSink : IGameEffectSink
    {
        private readonly ITimeScaleTarget timeScale;
        private readonly TimeScaleEase ease = new TimeScaleEase();

        public UnityEffectSink(ITimeScaleTarget _timeScale, IInputSource _inputSource)
        {
            timeScale = _timeScale;
            InputSource = _inputSource;
        }

        /// <summary>The input source in use, or null to leave input alone.</summary>
        public IInputSource InputSource { get; set; }

        public void Apply(GameEffects _effects)
        {
            if (_effects.TimeScale.HasValue)
            {
                ease.Start(timeScale.TimeScale, _effects.TimeScale.Value, _effects.TimeScaleEaseSeconds, _effects.TimeScaleEaseCurve);
                if (_effects.TimeScaleEaseSeconds <= 0f)
                {
                    timeScale.TimeScale = ease.Step(0f);
                }
            }
            if (_effects.Cursor.HasValue)
            {
                GameCursorController.UseMachineCursor(_effects.Cursor.Value);
            }
            if (_effects.HudVisible.HasValue)
            {
                GameHud.SetVisible(_effects.HudVisible.Value);
            }
            if (_effects.InputMapIds != null && InputSource != null)
            {
                InputSource.EnableOnly(_effects.InputMapIds);
            }
        }

        /// <summary>Moves a running ease on by unscaled seconds.</summary>
        public void Step(float _unscaledSeconds)
        {
            if (ease.IsRunning)
            {
                timeScale.TimeScale = ease.Step(_unscaledSeconds);
            }
        }
    }
}
