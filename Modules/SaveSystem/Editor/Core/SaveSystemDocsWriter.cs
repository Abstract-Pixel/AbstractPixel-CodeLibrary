using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>Writes assets/glossary-data.js (term tooltips) from GLOSSARY.md. Run after editing the glossary.
    /// The terms themselves are defined on their Types pages under Documentation/api/, not on a glossary page.</summary>
    internal static class SaveSystemDocsWriter
    {
        static readonly Regex TermLine = new Regex(@"^\*\*(.+?)\*\*:$");

        [MenuItem("Tools/External Project Tools/Save System/Update Glossary Data")]
        public static void UpdateGlossaryData()
        {
            string packageRoot = PackageRoot();
            string glossaryPath = Path.Combine(packageRoot, "Modules", "SaveSystem", "GLOSSARY.md");
            string docsRoot = Path.Combine(packageRoot, "Modules", "SaveSystem", "Documentation");
            GlossaryDoc doc = Parse(File.ReadAllText(glossaryPath));

            File.WriteAllText(Path.Combine(docsRoot, "assets", "glossary-data.js"), BuildDataJs(doc));
            AssetDatabase.Refresh();
            SaveLog.Info("[SaveSystem] Wrote assets/glossary-data.js from GLOSSARY.md.");
        }

        internal static string PackageRoot()
        {
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(SaveActions).Assembly);
            return package.resolvedPath;
        }

        internal sealed class Term
        {
            public string Name;
            public string Slug;
            public string Definition;
            public string Avoid;
        }

        internal sealed class Group
        {
            public string Name;
            public readonly List<Term> Terms = new List<Term>();
        }

        internal sealed class GlossaryDoc
        {
            public string Title;
            public string Intro;
            public readonly List<Group> Groups = new List<Group>();
        }

        internal static GlossaryDoc Parse(string markdown)
        {
            string[] lines = markdown.Replace("\r\n", "\n").Split('\n');
            var doc = new GlossaryDoc();
            int i = 0;
            while (i < lines.Length && !lines[i].StartsWith("# "))
            {
                i++;
            }
            doc.Title = lines[i].Substring(2).Trim();
            i++;

            var intro = new List<string>();
            while (i < lines.Length && !lines[i].StartsWith("## "))
            {
                if (lines[i].Trim().Length > 0)
                {
                    intro.Add(lines[i].Trim());
                }
                i++;
            }
            doc.Intro = string.Join(" ", intro);

            Group currentGroup = null;
            Term currentTerm = null;
            var defLines = new List<string>();

            void FlushTerm()
            {
                if (currentTerm == null)
                {
                    return;
                }
                currentTerm.Definition = string.Join(" ", defLines).Trim();
                currentGroup.Terms.Add(currentTerm);
                currentTerm = null;
                defLines.Clear();
            }

            for (; i < lines.Length; i++)
            {
                string trimmed = lines[i].Trim();
                if (trimmed.StartsWith("## "))
                {
                    FlushTerm();
                    currentGroup = new Group { Name = trimmed.Substring(3).Trim() };
                    doc.Groups.Add(currentGroup);
                    continue;
                }
                if (currentGroup == null)
                {
                    continue;
                }
                if (currentGroup.Name == "Code names")
                {
                    // The code-names table isn't part of the term tooltips; GLOSSARY.md keeps it for humans only.
                    continue;
                }
                Match termMatch = TermLine.Match(trimmed);
                if (termMatch.Success)
                {
                    FlushTerm();
                    string name = termMatch.Groups[1].Value.Trim();
                    currentTerm = new Term { Name = name, Slug = Slugify(name) };
                    continue;
                }
                if (trimmed.StartsWith("_Avoid_:"))
                {
                    if (currentTerm != null)
                    {
                        currentTerm.Avoid = trimmed.Substring("_Avoid_:".Length).Trim();
                    }
                    continue;
                }
                if (trimmed.Length == 0)
                {
                    continue;
                }
                defLines.Add(trimmed);
            }
            FlushTerm();
            return doc;
        }

        internal static string Slugify(string name)
        {
            string lowered = name.ToLowerInvariant();
            return Regex.Replace(lowered, "[^a-z0-9]+", "-").Trim('-');
        }

        internal static string BuildDataJs(GlossaryDoc doc)
        {
            var sb = new StringBuilder();
            sb.Append("// Generated by SaveSystemDocsWriter from GLOSSARY.md. Do not edit by hand.\n");
            sb.Append("window.DOCS_GLOSSARY = {\n");
            bool first = true;
            foreach (Group group in doc.Groups)
            {
                if (group.Name == "Code names")
                {
                    continue;
                }
                foreach (Term term in group.Terms)
                {
                    if (!first)
                    {
                        sb.Append(",\n");
                    }
                    first = false;
                    sb.Append("  ").Append(JsonString(term.Slug)).Append(": { term: ").Append(JsonString(term.Name))
                        .Append(", definition: ").Append(JsonString(term.Definition)).Append(" }");
                }
            }
            sb.Append("\n};\n");
            return sb.ToString();
        }

        static string JsonString(string text)
        {
            return "\"" + (text ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n") + "\"";
        }
    }
}
