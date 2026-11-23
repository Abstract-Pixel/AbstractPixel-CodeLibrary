using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using AbstractPixel.SceneManagement.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Documentation.Tests
{
    /// <summary>
    /// The scene documentation stays true to the code: a page per public type, pages that parse, links, anchors and
    /// terms that resolve, code blocks still in their scripts, images and icons that exist, and actions that point at
    /// real things.
    /// </summary>
    #if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
    #endif
    public class SceneDocsReferenceTests
    {
        private static readonly Regex LinkPattern = new Regex(@"(?<!!)\[[^\]]+\]\(([^)\s]+)\)");
        private static readonly Regex ImagePattern = new Regex(@"!\[[^\]]*\]\(([^)\s]+)\)");
        private static readonly string[] TransitionTypes =
        {
            "AbstractPixel.SceneTransitions.TransitionActions", "AbstractPixel.SceneTransitions.TransitionProfile",
            "AbstractPixel.SceneTransitions.ImageFadeProfile", "AbstractPixel.SceneTransitions.ScreenState",
            "AbstractPixel.SceneTransitions.TransitionEventBus"
        };

        /// <summary>Every documentation page as a topic: Pages/, the sample READMEs and the design decisions.</summary>
        public static List<string> AllTopics()
        {
            List<string> topics = new List<string>();
            foreach (string path in SceneDocsTopics.AllPagePaths())
            {
                topics.Add(SceneDocsTopics.TopicFor(path));
            }
            topics.Add(SceneDocsTopics.SamplesTopic);
            foreach (string number in SceneDocsTopics.SampleNumbers())
            {
                topics.Add(SceneDocsTopics.SamplePrefix + number);
            }
            foreach (string path in SceneDocsTopics.AdrPaths())
            {
                topics.Add(SceneDocsTopics.TopicFor(path));
            }
            return topics;
        }

        public static List<string> TypesInScope()
        {
            List<string> names = new List<string>();
            foreach (UnityEditor.Compilation.Assembly compiled in UnityEditor.Compilation.CompilationPipeline.GetAssemblies())
            {
                string name = compiled.name;
                bool isRuntimeScene = name.StartsWith("AbstractPixel.SceneManag") && !name.Contains("Test") && !name.Contains("Editor") && !name.Contains("Samples");
                if (!isRuntimeScene)
                {
                    continue;
                }
                Assembly assembly = Assembly.Load(name);
                foreach (System.Type type in assembly.GetExportedTypes())
                {
                    bool inScope = !type.IsNested && (type.Namespace == "AbstractPixel.SceneManagement" || type.Namespace == "AbstractPixel.SceneManagement.SceneScreens");
                    if (inScope)
                    {
                        names.Add(type.Name);
                    }
                }
            }
            foreach (string fullName in TransitionTypes)
            {
                names.Add(fullName.Substring(fullName.LastIndexOf('.') + 1));
            }
            names.Sort();
            return names;
        }

        [Test]
        public void EveryTypeInScopeHasATypePage()
        {
            List<string> missing = new List<string>();
            foreach (string name in TypesInScope())
            {
                if (SceneDocsTopics.FileFor("types/" + name.ToLowerInvariant()) == null)
                {
                    missing.Add(name);
                }
            }
            CollectionAssert.IsEmpty(missing, "Add Pages/types/<Name>.md for these types");
        }

        [Test]
        public void EveryTransitionTypeInScopeIsLoaded()
        {
            foreach (string fullName in TransitionTypes)
            {
                Assert.IsNotNull(SceneDocsMembers.FindType(fullName), fullName);
            }
        }

        [Test]
        public void EveryPageParsesWithNoProblem()
        {
            List<string> problems = new List<string>();
            foreach (string topic in AllTopics())
            {
                DocsPage page = SceneDocsPageCache.Get(topic);
                Assert.IsNotNull(page, topic);
                problems.AddRange(page.Problems);
            }
            CollectionAssert.IsEmpty(problems);
        }

        [Test]
        public void EveryContentsEntryOpensAPage()
        {
            foreach (SceneDocsContentsEntry entry in SceneDocsContents.Flatten())
            {
                Assert.IsTrue(Resolves(entry.Topic), entry.Title + " -> " + entry.Topic);
            }
        }

        [Test]
        public void EveryLinkAnchorAndTermResolves()
        {
            SceneDocsHost host = new SceneDocsHost(_topic => { });
            List<string> broken = new List<string>();
            foreach (string topic in AllTopics())
            {
                string text = WithoutCode(SceneDocsPaths.Read(SceneDocsTopics.FileFor(topic)));
                foreach (Match link in LinkPattern.Matches(text))
                {
                    string target = link.Groups[1].Value;
                    if (target.StartsWith("http"))
                    {
                        continue;
                    }
                    string resolved = host.TopicOfTarget(target, topic + ".md");
                    if (resolved == null || !Resolves(resolved))
                    {
                        broken.Add(topic + ": " + target);
                    }
                }
            }
            CollectionAssert.IsEmpty(broken);
        }

        [Test]
        public void EveryTermLinkNamesAGlossaryTerm()
        {
            List<string> unknown = new List<string>();
            foreach (KeyValuePair<string, string> term in SceneDocsContents.TermTopics())
            {
                if (!SceneGlossary.Terms.ContainsKey(term.Key))
                {
                    unknown.Add(term.Key);
                }
            }
            CollectionAssert.IsEmpty(unknown, "Contents.md Terms names a slug the glossary does not have");
        }

        [Test]
        public void EveryTopicConstantResolves()
        {
            List<string> broken = new List<string>();
            foreach (FieldInfo field in typeof(SceneSystemDocTopics).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                string topic = (string)field.GetValue(null);
                if (!Resolves(topic))
                {
                    broken.Add(field.Name + " = " + topic);
                }
            }
            CollectionAssert.IsEmpty(broken);
        }

        [Test]
        public void EveryCodeBlockStillAppearsInItsScript()
        {
            SceneDocsHost host = new SceneDocsHost(_topic => { });
            List<string> drifted = new List<string>();
            foreach (string topic in AllTopics())
            {
                foreach (DocsBlock block in AllBlocks(SceneDocsPageCache.Get(topic).Blocks))
                {
                    string source = block.Kind == DocsBlockKind.Code ? DocsCodeBlock.SourceOf(block.Info) : null;
                    if (source == null)
                    {
                        continue;
                    }
                    string path = host.ResolveSourcePath(source, topic + ".md");
                    string file = SceneDocsPaths.Read(path);
                    if (file == null)
                    {
                        drifted.Add(topic + ": missing " + source);
                        continue;
                    }
                    string normalisedFile = Regex.Replace(file, @"\s+", " ");
                    foreach (string line in block.Text.Split('\n'))
                    {
                        string trimmed = line.Trim();
                        bool isComment = trimmed.StartsWith("//") || trimmed.Length == 0;
                        if (!isComment && !normalisedFile.Contains(Regex.Replace(trimmed, @"\s+", " ")))
                        {
                            drifted.Add(topic + ": " + trimmed);
                        }
                    }
                }
            }
            CollectionAssert.IsEmpty(drifted);
        }

        [Test]
        public void EveryImageExists()
        {
            List<string> missing = new List<string>();
            foreach (string topic in AllTopics())
            {
                string fileAsset = SceneDocsTopics.FileFor(topic);
                foreach (Match image in ImagePattern.Matches(WithoutCode(SceneDocsPaths.Read(fileAsset))))
                {
                    string path = SceneDocsPaths.Combine(SceneDocsPaths.FolderOf(fileAsset), image.Groups[1].Value);
                    if (AssetDatabase.LoadAssetAtPath<Texture2D>(path) == null)
                    {
                        missing.Add(topic + ": " + image.Groups[1].Value);
                    }
                }
            }
            CollectionAssert.IsEmpty(missing);
        }

        [Test]
        public void EveryIconLoads()
        {
            foreach (string name in DocsIcons.All)
            {
                Assert.IsNotNull(DocsIcons.Load(name), name);
            }
        }

        [Test]
        public void EveryActionAndStepPointsAtSomethingReal()
        {
            List<string> broken = new List<string>();
            foreach (string topic in AllTopics())
            {
                foreach (DocsDirective directive in AllDirectives(SceneDocsPageCache.Get(topic).Blocks))
                {
                    string problem = ProblemOf(directive);
                    if (problem != null)
                    {
                        broken.Add(topic + ":" + directive.Line + " " + problem);
                    }
                }
                foreach (DocsListItem item in AllItems(SceneDocsPageCache.Get(topic).Blocks))
                {
                    bool unknownMessage = !string.IsNullOrEmpty(item.DoneWhen) && !SceneDocsDirectives.Messages.Contains(item.DoneWhen);
                    if (unknownMessage)
                    {
                        broken.Add(topic + ": done-when " + item.DoneWhen);
                    }
                }
            }
            CollectionAssert.IsEmpty(broken);
        }

        private static string ProblemOf(DocsDirective _directive)
        {
            if (_directive.Name != "action")
            {
                return null;
            }
            string kind = _directive.Argument(0);
            if (!SceneDocsDirectives.ActionKinds.Contains(kind))
            {
                return "unknown action " + kind;
            }
            bool needsAsset = kind == "select" || kind == "ping";
            if (needsAsset && AssetDatabase.LoadMainAssetAtPath(_directive.Argument(1)) == null)
            {
                return "no asset " + _directive.Argument(1);
            }
            if (kind == "open-sample" && SceneDocsActions.FirstSampleGroup(_directive.Argument(1)) == null)
            {
                return "no sample " + _directive.Argument(1);
            }
            if (kind == "select-in-scene" && AssetDatabase.LoadMainAssetAtPath(_directive.Argument(1)) == null)
            {
                return "no scene " + _directive.Argument(1);
            }
            if (kind == "open-settings" && !SettingsSections().Contains(_directive.Argument(1)))
            {
                return "no Scene System section " + _directive.Argument(1);
            }
            return null;
        }

        public static List<string> SettingsSections()
        {
            return new List<string>
            {
                SceneSystemSettingsProvider.SetupSection, SceneSystemSettingsProvider.StartUpSection, SceneSystemSettingsProvider.SceneSetsSection,
                SceneSystemLoadingScenesSection.Title, SceneSystemSettingsProvider.NewScenesSection, SceneSystemSettingsProvider.DebuggerSection,
                SceneSystemSampleGroupsSection.Title, SceneSystemRelocationSection.Title
            };
        }

        /// <summary>True when the page exists and, if the topic names a heading, the page has it.</summary>
        public static bool Resolves(string _topic)
        {
            string page;
            string anchor;
            SceneDocsTopics.Split(_topic, out page, out anchor);
            DocsPage parsed = SceneDocsPageCache.Get(page);
            if (parsed == null)
            {
                return false;
            }
            return anchor.Length == 0 || parsed.HasHeading(anchor);
        }

        private static string WithoutCode(string _text)
        {
            return Regex.Replace(_text ?? "", @"```[\s\S]*?```|`[^`\n]*`", "");
        }

        public static IEnumerable<DocsBlock> AllBlocks(List<DocsBlock> _blocks)
        {
            foreach (DocsBlock block in _blocks)
            {
                yield return block;
                foreach (DocsBlock child in AllBlocks(block.Children))
                {
                    yield return child;
                }
            }
        }

        private static IEnumerable<DocsDirective> AllDirectives(List<DocsBlock> _blocks)
        {
            foreach (DocsBlock block in AllBlocks(_blocks))
            {
                if (block.Directive != null)
                {
                    yield return block.Directive;
                }
            }
            foreach (DocsListItem item in AllItems(_blocks))
            {
                foreach (DocsDirective directive in item.Directives)
                {
                    yield return directive;
                }
            }
        }

        private static IEnumerable<DocsListItem> AllItems(List<DocsBlock> _blocks)
        {
            foreach (DocsBlock block in AllBlocks(_blocks))
            {
                foreach (DocsListItem item in ItemsOf(block))
                {
                    yield return item;
                }
            }
        }

        private static IEnumerable<DocsListItem> ItemsOf(DocsBlock _list)
        {
            foreach (DocsListItem item in _list.Items)
            {
                yield return item;
                if (item.NestedList != null)
                {
                    foreach (DocsListItem nested in ItemsOf(item.NestedList))
                    {
                        yield return nested;
                    }
                }
            }
        }
    }
}
