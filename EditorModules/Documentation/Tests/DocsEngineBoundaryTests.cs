using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Compilation;
using Assembly = System.Reflection.Assembly;

namespace AbstractPixel.EditorModules.Documentation.Tests
{
    public class DocsEngineBoundaryTests
    {
        private const string EngineAssembly = "AbstractPixel.EditorModules.Documentation";

        [Test]
        public void TheEngineAssemblyReferencesNoModuleAssembly()
        {
            List<string> offenders = new List<string>();
            foreach (AssemblyName reference in typeof(DocsRenderer).Assembly.GetReferencedAssemblies())
            {
                if (IsModuleAssembly(reference.Name))
                {
                    offenders.Add(reference.Name);
                }
            }
            string asmdef = File.ReadAllText(FileUtil.GetPhysicalPath(DocsEngineFiles.Root + EngineAssembly + ".asmdef"));
            foreach (Match quoted in Regex.Matches(asmdef, "\"(AbstractPixel\\.[^\"]+)\""))
            {
                if (IsModuleAssembly(quoted.Groups[1].Value))
                {
                    offenders.Add("asmdef: " + quoted.Groups[1].Value);
                }
            }
            CollectionAssert.IsEmpty(offenders);
        }

        [Test]
        public void NoEngineCodeNamesASceneType()
        {
            HashSet<string> sceneTypes = SceneTypeNames();
            Assert.Greater(sceneTypes.Count, 50, "the scene assemblies were not found, so this test proves nothing");
            List<string> offenders = new List<string>();
            string root = FileUtil.GetPhysicalPath(DocsEngineFiles.Root.TrimEnd('/'));
            foreach (string file in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories))
            {
                if (file.Replace('\\', '/').Contains("/Tests/"))
                {
                    continue;
                }
                foreach (Match word in Regex.Matches(File.ReadAllText(file), "[A-Za-z_][A-Za-z0-9_]*"))
                {
                    if (sceneTypes.Contains(word.Value))
                    {
                        offenders.Add(Path.GetFileName(file) + ": " + word.Value);
                    }
                }
            }
            CollectionAssert.IsEmpty(offenders);
        }

        private static bool IsModuleAssembly(string _name)
        {
            string path = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(_name);
            return path != null && path.Contains("com.abstractpixel.library/Modules/");
        }

        // Type names declared by the scene module's runtime and editor assemblies.
        private static HashSet<string> SceneTypeNames()
        {
            HashSet<string> names = new HashSet<string>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string name = assembly.GetName().Name;
                bool isScene = name.StartsWith("AbstractPixel.SceneManag") && !name.Contains("Tests") && !name.Contains("Samples");
                if (!isScene)
                {
                    continue;
                }
                foreach (Type type in assembly.GetTypes())
                {
                    // A nested type is only reached through its owner, and its short name is often an everyday word.
                    if (!type.IsNested)
                    {
                        names.Add(type.Name.Split('`')[0]);
                    }
                }
            }
            return names;
        }
    }
}
