using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// A code block: a header (file name, Copy, Open in IDE), a line-number gutter and selectable, highlighted code in
    /// the Editor's RobotoMono font, with its own horizontal scroll. Lines never wrap.
    /// </summary>
    #if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
    #endif
    public static class DocsCodeBlock
    {
        public const string MonoFontPath = "Fonts/RobotoMono/RobotoMono-Regular.ttf";
        private static Font monoFont;
        private static bool monoFontLooked;

        /// <summary>The Console's monospace font, or null when this Editor has none at that path.</summary>
        public static Font MonoFont
        {
            get
            {
                if (!monoFontLooked)
                {
                    monoFontLooked = true;
                    monoFont = EditorGUIUtility.Load(MonoFontPath) as Font;
                }
                return monoFont;
            }
        }

        public static VisualElement Create(DocsBlock _block, DocsRenderContext _context)
        {
            string source = SourceOf(_block.Info);
            string assetPath = string.IsNullOrEmpty(source) ? null : _context.Host.ResolveSourcePath(source, _context.Page.Path);
            VisualElement box = new VisualElement();
            box.AddToClassList("docs-code");
            box.Add(Header(_block, source, assetPath));
            box.Add(Body(_block.Text));
            return box;
        }

        /// <summary>The path after "source=" in a fence's info string, or null.</summary>
        public static string SourceOf(string _info)
        {
            if (string.IsNullOrEmpty(_info))
            {
                return null;
            }
            // The path is everything after "source=", so it may hold spaces (sample folders do).
            int start = _info.IndexOf("source=", System.StringComparison.Ordinal);
            if (start < 0)
            {
                return null;
            }
            return _info.Substring(start + "source=".Length).Trim();
        }

        private static VisualElement Header(DocsBlock _block, string _source, string _assetPath)
        {
            VisualElement header = new VisualElement();
            header.AddToClassList("docs-code-header");
            string title = string.IsNullOrEmpty(_source) ? "Example" : Path.GetFileName(_source.Replace("lib:", ""));
            // A small C# badge in the language's purple, before the language or file name.
            Label language = new Label("C#");
            language.AddToClassList("docs-code-language");
            header.Add(language);
            Label titleLabel = new Label(title);
            titleLabel.AddToClassList("docs-code-title");
            header.Add(titleLabel);
            string code = _block.Text;
            bool leavesCodeOut = code.Contains("// ...");
            if (!leavesCodeOut)
            {
                Button copy = DocsCodeIcons.Action(DocsCodeIcons.Copy(), "Copy", "Copies this code to the clipboard", () => EditorGUIUtility.systemCopyBuffer = code);
                header.Add(copy);
            }
            if (!string.IsNullOrEmpty(_assetPath))
            {
                int line = FirstLineInFile(_assetPath, code);
                Button open = DocsCodeIcons.Action(DocsCodeIcons.OpenInIde(), "Open in IDE", "Opens " + title + " at line " + line + " in your code editor", () => OpenAt(_assetPath, line));
                header.Add(open);
            }
            return header;
        }

        private static VisualElement Body(string _code)
        {
            string[] lines = _code.Split('\n');
            ScrollView scroll = new ScrollView(ScrollViewMode.Horizontal);
            scroll.AddToClassList("docs-code-scroll");
            VisualElement row = new VisualElement();
            row.AddToClassList("docs-code-row");
            Label gutter = new Label(LineNumbers(lines.Length));
            gutter.AddToClassList("docs-code-gutter");
            Label code = new Label(Highlight(lines));
            code.enableRichText = true;
            code.selection.isSelectable = true;
            code.focusable = true;
            code.AddToClassList("docs-code-text");
            ApplyMonoFont(gutter);
            ApplyMonoFont(code);
            row.Add(gutter);
            row.Add(code);
            scroll.Add(row);
            return scroll;
        }

        // The font cannot be named in USS (it lives in the Editor's own resources), so it is set here, as the guide says.
        public static void ApplyMonoFont(VisualElement _element)
        {
            Font font = MonoFont;
            if (font == null)
            {
                return;
            }
            _element.style.unityFontDefinition = FontDefinition.FromFont(font);
        }

        private static string Highlight(string[] _lines)
        {
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            for (int index = 0; index < _lines.Length; index++)
            {
                if (index > 0)
                {
                    result.Append('\n');
                }
                string highlighted = KeepIndent(DocsCodeHighlighter.Highlight(_lines[index]));
                bool needsMspace = MonoFont == null;
                result.Append(needsMspace ? "<mspace=0.6em>" + highlighted + "</mspace>" : highlighted);
            }
            return result.ToString();
        }

        // UI Toolkit drops leading spaces, so the indent becomes no-break spaces.
        private static string KeepIndent(string _line)
        {
            int spaces = 0;
            while (spaces < _line.Length && _line[spaces] == ' ')
            {
                spaces++;
            }
            if (spaces == 0)
            {
                return _line;
            }
            return new string(NoBreakSpace, spaces) + _line.Substring(spaces);
        }

        private const char NoBreakSpace = (char)0xA0;

        private static string LineNumbers(int _count)
        {
            System.Text.StringBuilder numbers = new System.Text.StringBuilder();
            for (int number = 1; number <= _count; number++)
            {
                if (number > 1)
                {
                    numbers.Append('\n');
                }
                numbers.Append(number);
            }
            return numbers.ToString();
        }

        /// <summary>The 1-based line in the file where the block's first non-empty line is, or 1.</summary>
        public static int FirstLineInFile(string _assetPath, string _code)
        {
            string firstLine = null;
            foreach (string line in _code.Split('\n'))
            {
                if (line.Trim().Length > 0)
                {
                    firstLine = line.Trim();
                    break;
                }
            }
            if (firstLine == null || !File.Exists(_assetPath))
            {
                return 1;
            }
            string[] fileLines = File.ReadAllLines(_assetPath);
            for (int index = 0; index < fileLines.Length; index++)
            {
                if (fileLines[index].Trim() == firstLine)
                {
                    return index + 1;
                }
            }
            return 1;
        }

        private static void OpenAt(string _assetPath, int _line)
        {
            Object script = AssetDatabase.LoadMainAssetAtPath(_assetPath);
            if (script == null)
            {
                Debug.LogWarning("[Scene documentation] Cannot open " + _assetPath + ": the file is missing.");
                return;
            }
            AssetDatabase.OpenAsset(script, _line);
        }
    }
}
