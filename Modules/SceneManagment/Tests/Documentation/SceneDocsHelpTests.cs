using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using AbstractPixel.SceneManagement.Editor;
using NUnit.Framework;

namespace AbstractPixel.SceneManagement.Documentation.Tests
{
    /// <summary>Every help button opens a topic that exists, and every setup step, page section and problem has one.</summary>
    public class SceneDocsHelpTests
    {
        private static readonly Regex OpenCall = new Regex(@"SceneSystemDocs\.Open\(([^)]*)\)");
        private static readonly Regex HelpCall = new Regex(@"SceneSystemHelp\.(?:Button|LearnMore|HelpRow|TabBarButton|TitleBarButton)\(([^;]*)\)");

        [Test]
        public void EveryOpenCallUsesATopicConstant()
        {
            List<string> loose = new List<string>();
            string root = SceneDocsPaths.Physical(SceneDocsPaths.Module);
            foreach (string path in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories))
            {
                string normalised = path.Replace('\\', '/');
                bool isExempt = normalised.Contains("/Tests/") || normalised.EndsWith("SceneSystemDocs.cs") || normalised.EndsWith("SceneSystemHelp.cs");
                if (isExempt)
                {
                    continue;
                }
                string text = File.ReadAllText(path);
                foreach (Match call in OpenCall.Matches(text))
                {
                    if (!call.Groups[1].Value.Contains("SceneSystemDocTopics."))
                    {
                        loose.Add(Path.GetFileName(path) + ": " + call.Value);
                    }
                }
                foreach (Match call in HelpCall.Matches(text))
                {
                    bool usesConstant = call.Groups[1].Value.Contains("SceneSystemDocTopics.") || call.Groups[1].Value.Contains("TopicId") || call.Groups[1].Value.Contains("topic");
                    if (!usesConstant)
                    {
                        loose.Add(Path.GetFileName(path) + ": " + call.Value);
                    }
                }
            }
            CollectionAssert.IsEmpty(loose, "Open the docs with a SceneSystemDocTopics constant");
        }

        [Test]
        public void EveryTopicConstantResolves()
        {
            List<string> broken = new List<string>();
            foreach (FieldInfo field in typeof(SceneSystemDocTopics).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                string topic = (string)field.GetValue(null);
                if (!SceneDocsReferenceTests.Resolves(topic))
                {
                    broken.Add(field.Name + " = " + topic);
                }
            }
            CollectionAssert.IsEmpty(broken);
        }

        [Test]
        public void EveryProblemKindHasAFindAProblemEntry()
        {
            foreach (SceneGroupProblemKind kind in System.Enum.GetValues(typeof(SceneGroupProblemKind)))
            {
                string topic = SceneSystemDocTopics.ForProblem(kind);
                StringAssert.StartsWith("reference/find-a-problem#", topic, kind.ToString());
                Assert.IsTrue(SceneDocsReferenceTests.Resolves(topic), kind + " -> " + topic);
            }
        }

        [Test]
        public void EverySettingsSectionHasAHelpButton()
        {
            string provider = SceneDocsPaths.Read(SceneDocsPaths.Module + "Editor/Settings/SceneSystemSettingsProvider.cs");
            int sectionsWithHelp = Regex.Matches(provider, @"AddSection\(page,.*SceneSystemDocTopics\.").Count;
            bool debuggerHasHelp = provider.Contains("SceneSystemDocTopics.DebuggerSection");
            Assert.AreEqual(7, sectionsWithHelp, "seven sections made with AddSection carry a topic");
            Assert.IsTrue(debuggerHasHelp, "the Scene Groups Debugger header has a help button");
            Assert.AreEqual(8, SceneDocsReferenceTests.SettingsSections().Count);
        }

        [Test]
        public void EverySetupStepHasATopic()
        {
            string[] steps = { "SetupWelcome", "SetupFolder", "SetupNewScenes", "SetupLaunchScene", "SetupLoadingScene", "SetupSceneSets", "SetupOpeningScenes", "SetupReady" };
            foreach (string step in steps)
            {
                FieldInfo field = typeof(SceneSystemDocTopics).GetField(step);
                Assert.IsNotNull(field, step);
                Assert.IsTrue(SceneDocsReferenceTests.Resolves((string)field.GetValue(null)), step);
            }
        }
    }
}
