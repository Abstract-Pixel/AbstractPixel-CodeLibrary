using System.Collections.Generic;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>What a setup run made, for the Ready and All set pages and for tests.</summary>
    public class OnboardingResult
    {
        public GameManagementSettings Settings;
        /// <summary>Every asset and folder this run made, in the order it made them. A failed run deletes them again.</summary>
        public readonly List<string> CreatedPaths = new List<string>();
        /// <summary>One sentence for each name that was taken: what was wanted and what was made instead.</summary>
        public readonly List<string> Renamed = new List<string>();
        /// <summary>Things that were skipped, with the reason and what to do.</summary>
        public readonly List<string> Skipped = new List<string>();
        public readonly List<OnboardingToDo> ToDo = new List<OnboardingToDo>();
    }
}
