using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using AbstractPixel.SaveSystem.Editor;
using NUnit.Framework;

namespace AbstractPixel.SaveSystem.Tests
{
    /// <summary>
    /// Keeps the save system's HTML docs honest: every link and #anchor points somewhere real, every sample has a
    /// README some docs page links to, a sample README's "lines that matter" still appear in its scripts, every
    /// [SaveSystemHelp] target exists, the term tooltip data matches GLOSSARY.md, and every public type has an api/
    /// page that lists every one of its public members.
    /// </summary>
    public class SaveSystemDocsTests
    {
        static readonly Regex HrefOrSrc = new Regex("(?:href|src)=\"([^\"]+)\"");
        static readonly Regex Id = new Regex("id=\"([^\"]+)\"");
        static readonly Regex SourcedCode = new Regex("<pre data-source=\"([^\"]+)\"><code>(.*?)</code></pre>", RegexOptions.Singleline);

        static string PackageRoot
        {
            get
            {
                UnityEditor.PackageManager.PackageInfo package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(SaveActions).Assembly);
                Assert.IsNotNull(package, "The save system should be inside the Abstract Pixel Library package.");
                return package.resolvedPath;
            }
        }

        static string ModuleRoot => Path.Combine(PackageRoot, "Modules", "SaveSystem");
        static string DocsRoot => Path.Combine(ModuleRoot, "Documentation");
        static string SamplesFolder => Path.Combine(ModuleRoot, "Samples");
        static string GlossaryPath => Path.Combine(ModuleRoot, "GLOSSARY.md");

        static IEnumerable<string> DocsPages()
        {
            return Directory.GetFiles(DocsRoot, "*.html", SearchOption.AllDirectories);
        }

        static IEnumerable<string> HtmlPages()
        {
            return DocsPages().Concat(Directory.GetFiles(SamplesFolder, "*.html", SearchOption.AllDirectories));
        }

        static IEnumerable<string> SampleFolders()
        {
            return Directory.GetDirectories(SamplesFolder).Where(folder => char.IsDigit(Path.GetFileName(folder)[0]));
        }

        [Test]
        public void EveryLinkInTheDocs_PointsAtAFileAndAnchorThatExist()
        {
            var broken = new List<string>();
            foreach (string page in HtmlPages())
            {
                string html = File.ReadAllText(page);
                foreach (Match match in HrefOrSrc.Matches(html))
                {
                    string link = WebUtility.HtmlDecode(match.Groups[1].Value);
                    if (link.StartsWith("http:") || link.StartsWith("https:") || link.StartsWith("mailto:"))
                    {
                        continue;
                    }
                    string[] parts = link.Split('#');
                    string target = parts[0].Length == 0 ? page : Path.GetFullPath(Path.Combine(Path.GetDirectoryName(page), Uri.UnescapeDataString(parts[0])));
                    if (!File.Exists(target))
                    {
                        broken.Add($"{Name(page)}: {link} (no such file)");
                        continue;
                    }
                    bool hasAnchor = parts.Length > 1 && parts[1].Length > 0;
                    if (hasAnchor && target.EndsWith(".html") && !Id.Matches(File.ReadAllText(target)).Cast<Match>().Any(id => id.Groups[1].Value == parts[1]))
                    {
                        broken.Add($"{Name(page)}: {link} (no id \"{parts[1]}\")");
                    }
                }
            }
            Assert.IsEmpty(broken, "Broken links in the docs:\n" + string.Join("\n", broken));
        }

        [Test]
        public void EverySample_HasAReadme_AndSomeDocsPageLinksToIt()
        {
            string allDocsText = string.Join("\n", DocsPages().Select(p => WebUtility.HtmlDecode(File.ReadAllText(p))));
            foreach (string folder in SampleFolders())
            {
                string name = Path.GetFileName(folder);
                Assert.IsTrue(File.Exists(Path.Combine(folder, "README.html")), $"Sample '{name}' has no README.html.");
                StringAssert.Contains($"Samples/{name}/README.html", allDocsText, $"No docs page links to sample '{name}'.");
            }
        }

        [Test]
        public void TheLinesThatMatter_InEachSampleReadme_StillAppearInItsScripts()
        {
            var drifted = new List<string>();
            foreach (string folder in SampleFolders())
            {
                string readme = File.ReadAllText(Path.Combine(folder, "README.html"));
                foreach (Match block in SourcedCode.Matches(readme))
                {
                    string[] sources = block.Groups[1].Value.Split(' ');
                    string allSource = string.Join("\n", sources.Select(source => Collapse(File.ReadAllText(Path.Combine(folder, source)))));
                    foreach (string line in WebUtility.HtmlDecode(block.Groups[2].Value).Split('\n'))
                    {
                        string code = Collapse(StripComment(line));
                        if (code.Length > 0 && !allSource.Contains(code))
                        {
                            drifted.Add($"{Path.GetFileName(folder)}: {code}");
                        }
                    }
                }
            }
            Assert.IsEmpty(drifted, "README code lines that no longer match the sample's scripts:\n" + string.Join("\n", drifted));
        }

        [Test]
        public void EverySaveSystemHelpTarget_Exists()
        {
            var broken = new List<string>();
            IEnumerable<Type> candidates = typeof(SaveActions).Assembly.GetTypes().Concat(typeof(SaveSystemGuide).Assembly.GetTypes());
            foreach (Type type in candidates)
            {
                var attr = type.GetCustomAttributes(typeof(SaveSystemHelpAttribute), false).Cast<SaveSystemHelpAttribute>().FirstOrDefault();
                if (attr == null)
                {
                    continue;
                }
                string page = (string)typeof(SaveSystemHelpAttribute).GetField("section", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(attr);
                CheckGuideTarget(page, type.Name, broken);
            }

            FieldInfo savedObjectsTarget = typeof(SavedObjectsWindow).GetField("GuideSection", BindingFlags.NonPublic | BindingFlags.Static);
            if (savedObjectsTarget != null)
            {
                CheckGuideTarget((string)savedObjectsTarget.GetRawConstantValue(), "SavedObjectsWindow", broken);
            }

            Assert.IsEmpty(broken, "Missing [SaveSystemHelp] targets:\n" + string.Join("\n", broken));
        }

        static void CheckGuideTarget(string page, string owner, List<string> broken)
        {
            string[] parts = page.Split('#');
            string file = Path.Combine(DocsRoot, parts[0].Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(file))
            {
                broken.Add($"{owner}: [SaveSystemHelp(\"{page}\")] (no such file)");
                return;
            }
            if (parts.Length > 1 && !Id.Matches(File.ReadAllText(file)).Cast<Match>().Any(id => id.Groups[1].Value == parts[1]))
            {
                broken.Add($"{owner}: [SaveSystemHelp(\"{page}\")] (no id \"{parts[1]}\")");
            }
        }

        [Test]
        public void GlossaryData_MatchesGlossaryMd()
        {
            SaveSystemDocsWriter.GlossaryDoc doc = SaveSystemDocsWriter.Parse(File.ReadAllText(GlossaryPath));
            string expectedJs = SaveSystemDocsWriter.BuildDataJs(doc);
            string actualJs = File.ReadAllText(Path.Combine(DocsRoot, "assets", "glossary-data.js"));
            Assert.AreEqual(expectedJs, actualJs, "assets/glossary-data.js is stale. Run Tools > External Project Tools > Save System > Update Glossary Data.");
        }

        [Test]
        public void NoDocsPage_LinksToTheRemovedGlossaryPage()
        {
            var offenders = new List<string>();
            foreach (string page in HtmlPages())
            {
                if (File.ReadAllText(page).Contains("glossary.html"))
                {
                    offenders.Add(Name(page));
                }
            }
            Assert.IsEmpty(offenders, "Pages still linking to the removed glossary.html:\n" + string.Join("\n", offenders));
        }

        static IEnumerable<Type> PublicSaveSystemTypes()
        {
            return typeof(SaveActions).Assembly.GetTypes()
                .Where(type => type.IsPublic && !type.IsNested && type.Namespace == "AbstractPixel.SaveSystem");
        }

        static string PageNameFor(Type type)
        {
            return type.Name.Split('`')[0];
        }

        [Test]
        public void EveryPublicType_HasAnApiPage()
        {
            var missing = PublicSaveSystemTypes()
                .Select(PageNameFor)
                .Distinct()
                .Where(name => !File.Exists(Path.Combine(DocsRoot, "api", name + ".html")))
                .ToList();
            Assert.IsEmpty(missing, "Public types with no api/ page:\n" + string.Join("\n", missing));
        }

        [Test]
        public void EachTypePage_ListsEveryPublicMember()
        {
            var missing = new List<string>();
            foreach (Type type in PublicSaveSystemTypes())
            {
                string path = Path.Combine(DocsRoot, "api", PageNameFor(type) + ".html");
                if (!File.Exists(path))
                {
                    continue; // Reported by EveryPublicType_HasAnApiPage.
                }
                string html = File.ReadAllText(path);
                foreach (string member in PublicMemberNames(type))
                {
                    if (!html.Contains(member))
                    {
                        missing.Add($"{PageNameFor(type)}: {member}");
                    }
                }
            }
            Assert.IsEmpty(missing, "Type pages missing a public member:\n" + string.Join("\n", missing));
        }

        // Trivial object overrides aren't meaningful API; a type page doesn't need to list them.
        static readonly HashSet<string> TrivialObjectOverrides = new HashSet<string> { "ToString", "Equals", "GetHashCode" };

        static IEnumerable<string> PublicMemberNames(Type type)
        {
            if (type.IsEnum)
            {
                return Enum.GetNames(type);
            }
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            var names = new List<string>();
            names.AddRange(type.GetMethods(flags).Where(m => !m.IsSpecialName && !TrivialObjectOverrides.Contains(m.Name)).Select(m => m.Name));
            names.AddRange(type.GetProperties(flags).Select(p => p.Name));
            names.AddRange(type.GetEvents(flags).Select(e => e.Name));
            names.AddRange(type.GetFields(flags).Select(f => f.Name));
            return names.Distinct();
        }

        static string StripComment(string line)
        {
            int comment = line.IndexOf("//", StringComparison.Ordinal);
            return comment < 0 ? line : line.Substring(0, comment);
        }

        static string Collapse(string text)
        {
            return Regex.Replace(text, @"\s+", " ").Trim();
        }

        static string Name(string page)
        {
            return page.Substring(PackageRoot.Length).TrimStart('/', '\\');
        }
    }
}
