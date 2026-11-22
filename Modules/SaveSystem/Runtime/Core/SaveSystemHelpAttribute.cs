using System;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// The "?" button in the header of a save system inspector opens the guide at the section about it, the way
    /// Unity's own components open their manual page. The guide ships in the package, so the Editor finds the link
    /// (SaveSystemGuide); in a build there is none.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class SaveSystemHelpAttribute : HelpURLAttribute
    {
        /// <summary>Set by the Editor: turns a guide section id, such as "config", into a link.</summary>
        internal static Func<string, string> FindLink;

        readonly string section;

        public SaveSystemHelpAttribute(string section) : base("")
        {
            this.section = section;
        }

        public override string URL => FindLink?.Invoke(section);
    }
}
