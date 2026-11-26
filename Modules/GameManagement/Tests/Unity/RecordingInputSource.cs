using System.Collections.Generic;

namespace AbstractPixel.GameManagement.Tests
{
    /// <summary>A custom input source that remembers the maps it was asked to enable.</summary>
    public class RecordingInputSource : IInputSource
    {
        public readonly List<IReadOnlyList<string>> Calls = new List<IReadOnlyList<string>>();

        public void EnableOnly(IReadOnlyList<string> _mapIds)
        {
            Calls.Add(_mapIds);
        }
    }
}
