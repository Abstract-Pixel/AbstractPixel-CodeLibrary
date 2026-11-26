using System;

namespace AbstractPixel.EditorModules.Documentation
{
    /// <summary>Finds a type a page names, wherever the project declares it.</summary>
    public static class DocsTypeFinder
    {
        /// <summary>The type with this full name in any loaded assembly, or null.</summary>
        public static Type FindType(string _fullName)
        {
            // The project's own assemblies, as Unity lists them (AppDomain can list assemblies already unloaded).
            foreach (UnityEditor.Compilation.Assembly assembly in UnityEditor.Compilation.CompilationPipeline.GetAssemblies())
            {
                Type type = Type.GetType(_fullName + ", " + assembly.name, false);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }
    }
}
