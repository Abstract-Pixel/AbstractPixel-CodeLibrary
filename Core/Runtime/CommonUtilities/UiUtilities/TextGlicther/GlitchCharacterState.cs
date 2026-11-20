namespace AbstractPixel.Core
{
    public partial class TMPGlitchTextEffect
    {
        // A helper class to track the exact state of every single character
        private class GlitchCharacterState
        {
            public char TargetCharacter;
            public char CurrentDisplayCharacter;

            public bool IsRevealed;
            public bool IsSettledToTargetChar;

            public float TimeUntilSettled;
            public float TimeUntilNextRandomChange;
        }
    }
}
