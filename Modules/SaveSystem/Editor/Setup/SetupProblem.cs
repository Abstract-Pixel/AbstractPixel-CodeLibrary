using System;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// One thing wrong with the save system's setup, found before play mode starts or a build runs. SetupCheck
    /// finds these; SetupProblemsWindow lists them.
    /// </summary>
    internal sealed class SetupProblem
    {
        /// <summary>The saved script's class, when the problem is about one. Null for a problem with no single script, such as a stale GameSaveFiles.cs.</summary>
        public readonly Type ScriptType;

        /// <summary>What a "Ping" button selects: a scene object, an asset, or null when there is nothing to ping.</summary>
        public readonly UnityEngine.Object Target;

        public readonly string WhatIsWrong;
        public readonly string HowToFix;

        public SetupProblem(Type scriptType, UnityEngine.Object target, string whatIsWrong, string howToFix)
        {
            ScriptType = scriptType;
            Target = target;
            WhatIsWrong = whatIsWrong;
            HowToFix = howToFix;
        }
    }
}
