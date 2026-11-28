using System.IO;
using System.Linq;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>Decides whether a player build holds the in-game debugger.</summary>
    internal static class DebuggerBuildRule
    {
        private const string AssemblyFile = "AbstractPixel.GameManagement.Debugger.dll";

        public static bool Includes(bool _developmentBuild, bool _optedIn)
        {
            return _developmentBuild || _optedIn;
        }

        /// <summary>The assemblies a build keeps: all of them, or all but the debugger's.</summary>
        public static string[] Filter(string[] _assemblies, bool _includesDebugger)
        {
            if (_includesDebugger)
            {
                return _assemblies;
            }
            return _assemblies.Where(_path => Path.GetFileName(_path) != AssemblyFile).ToArray();
        }
    }
}
