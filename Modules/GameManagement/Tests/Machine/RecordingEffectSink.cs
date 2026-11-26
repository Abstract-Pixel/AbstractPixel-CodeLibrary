using System.Collections.Generic;

namespace AbstractPixel.GameManagement.Tests
{
    /// <summary>Remembers every set of effects the machine applied.</summary>
    public class RecordingEffectSink : IGameEffectSink
    {
        public readonly List<GameEffects> Applied = new List<GameEffects>();

        public GameEffects Last
        {
            get { return Applied[Applied.Count - 1]; }
        }

        public void Apply(GameEffects _effects)
        {
            Applied.Add(_effects);
        }
    }
}
