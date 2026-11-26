using AbstractPixel.EditorModules.Documentation;
using System.Collections.Generic;
using System.Text;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// The module's GLOSSARY.md, read directly: each bold term ("**Scene group**:") with its definition. Term links
    /// use the term's slug ("scene-group").
    /// </summary>
    #if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
    #endif
    public static class SceneGlossary
    {
        private static Dictionary<string, KeyValuePair<string, string>> terms;
        private static string loadedText;

        /// <summary>Slug to (term, definition).</summary>
        public static Dictionary<string, KeyValuePair<string, string>> Terms
        {
            get
            {
                string text = DocsFiles.Read(SceneDocsPaths.Glossary) ?? "";
                if (terms == null || text != loadedText)
                {
                    terms = Parse(text);
                    loadedText = text;
                }
                return terms;
            }
        }

        public static Dictionary<string, KeyValuePair<string, string>> Parse(string _text)
        {
            Dictionary<string, KeyValuePair<string, string>> found = new Dictionary<string, KeyValuePair<string, string>>();
            string[] lines = _text.Replace("\r\n", "\n").Split('\n');
            for (int index = 0; index < lines.Length; index++)
            {
                string line = lines[index].Trim();
                bool isTerm = line.StartsWith("**") && line.EndsWith("**:");
                if (!isTerm)
                {
                    continue;
                }
                string term = line.Substring(2, line.Length - 5);
                StringBuilder definition = new StringBuilder();
                int next = index + 1;
                while (next < lines.Length)
                {
                    string body = lines[next].Trim();
                    if (body.Length == 0 || body.StartsWith("_Avoid_") || body.StartsWith("**"))
                    {
                        break;
                    }
                    if (definition.Length > 0)
                    {
                        definition.Append(' ');
                    }
                    definition.Append(body);
                    next++;
                }
                found[DocsSlug.From(term)] = new KeyValuePair<string, string>(term, definition.ToString());
            }
            return found;
        }

        /// <summary>"Scene group: An asset that..." for a slug, or null.</summary>
        public static string TooltipFor(string _slug)
        {
            KeyValuePair<string, string> entry;
            if (!Terms.TryGetValue(_slug, out entry))
            {
                return null;
            }
            return entry.Key + ": " + DocsInline.ToPlainText(entry.Value);
        }
    }
}
