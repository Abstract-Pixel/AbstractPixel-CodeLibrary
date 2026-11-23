using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// Reads a Markdown page into blocks: headings (#, ##, ###), paragraphs, bullet and numbered lists (two levels),
    /// tables, quotes and callouts (&gt; [!NOTE], [!WARNING], [!TIP]), fenced code, images with marks,
    /// &lt;details&gt;&lt;summary&gt; foldouts and directives (HTML comments). A directive whose name is not known is
    /// reported in <see cref="DocsPage.Problems"/> with the page and line.
    /// </summary>
    public static class MarkdownParser
    {
        private static readonly Regex DirectivePattern = new Regex(@"<!--\s*(.*?)\s*-->");
        private static readonly Regex HeadingPattern = new Regex(@"^(#{1,3})\s+(.*)$");
        private static readonly Regex BulletPattern = new Regex(@"^(\s*)[-*]\s+(.*)$");
        private static readonly Regex NumberedPattern = new Regex(@"^(\s*)\d+\.\s+(.*)$");
        private static readonly Regex ImagePattern = new Regex(@"^!\[(.*?)\]\((.*?)\)\s*$");
        private static readonly Regex CalloutPattern = new Regex(@"^\[!(NOTE|WARNING|TIP)\]\s*$");
        private static readonly Regex SummaryPattern = new Regex(@"^<details>\s*<summary>(.*?)</summary>\s*$");
        private static readonly Regex MarkPattern = new Regex(@"^(\d+)@([-0-9.]+),([-0-9.]+)\s+""(.*)""$");

        public static DocsPage Parse(string _text, string _path, ICollection<string> _knownDirectives)
        {
            DocsPage page = new DocsPage(_path);
            string normalised = (_text ?? "").Replace("\r\n", "\n").Replace('\r', '\n');
            string[] lines = normalised.Split('\n');
            ParseContext context = new ParseContext(page, lines, _knownDirectives);
            int index = 0;
            ParseBlocks(context, ref index, page.Blocks, false);
            return page;
        }

        // The state one parse shares between its helpers, so no helper needs more than three parameters.
        private class ParseContext
        {
            public readonly DocsPage Page;
            public readonly string[] Lines;
            public readonly ICollection<string> Known;
            public readonly Dictionary<string, int> UsedIds = new Dictionary<string, int>();

            public ParseContext(DocsPage _page, string[] _lines, ICollection<string> _known)
            {
                Page = _page;
                Lines = _lines;
                Known = _known;
            }
        }

        // Reads blocks until the end, or until "</details>" when inside a foldout.
        private static void ParseBlocks(ParseContext _context, ref int _index, List<DocsBlock> _into, bool _insideDetails)
        {
            string[] lines = _context.Lines;
            while (_index < lines.Length)
            {
                string line = lines[_index];
                string trimmed = line.Trim();
                if (trimmed.Length == 0)
                {
                    _index++;
                    continue;
                }
                if (_insideDetails && trimmed == "</details>")
                {
                    _index++;
                    return;
                }
                DocsBlock block = ParseOne(_context, ref _index);
                if (block != null)
                {
                    _into.Add(block);
                }
            }
        }

        private static DocsBlock ParseOne(ParseContext _context, ref int _index)
        {
            string line = _context.Lines[_index];
            string trimmed = line.Trim();
            int lineNumber = _index + 1;
            if (trimmed.StartsWith("```"))
            {
                return ParseCode(_context, ref _index);
            }
            Match summary = SummaryPattern.Match(trimmed);
            if (summary.Success)
            {
                DocsBlock details = new DocsBlock(DocsBlockKind.Details, lineNumber);
                details.Title = summary.Groups[1].Value.Trim();
                _index++;
                ParseBlocks(_context, ref _index, details.Children, true);
                return details;
            }
            Match heading = HeadingPattern.Match(trimmed);
            if (heading.Success)
            {
                _index++;
                return ParseHeading(_context, heading, lineNumber);
            }
            if (trimmed.StartsWith("<!--") && trimmed.EndsWith("-->"))
            {
                _index++;
                return ParseDirectiveLine(_context, trimmed, lineNumber);
            }
            Match image = ImagePattern.Match(trimmed);
            if (image.Success)
            {
                _index++;
                return ParseImage(_context, image, ref _index);
            }
            if (trimmed.StartsWith(">"))
            {
                return ParseQuote(_context, ref _index);
            }
            if (trimmed.StartsWith("|"))
            {
                return ParseTable(_context, ref _index);
            }
            if (BulletPattern.IsMatch(line) || NumberedPattern.IsMatch(line))
            {
                return ParseList(_context, ref _index, IndentOf(line));
            }
            return ParseParagraph(_context, ref _index);
        }

        private static DocsBlock ParseHeading(ParseContext _context, Match _match, int _lineNumber)
        {
            int level = _match.Groups[1].Value.Length;
            string text = _match.Groups[2].Value;
            int pill = TakePill(_context, ref text, _lineNumber);
            if (level == 1 && _context.Page.Title == null)
            {
                _context.Page.Title = text;
                _context.Page.TitlePill = pill;
                return null;
            }
            DocsBlock block = new DocsBlock(DocsBlockKind.Heading, _lineNumber);
            block.Level = level;
            block.Text = text;
            block.Pill = pill;
            block.Id = UniqueId(_context, DocsSlug.From(text));
            _context.Page.Headings.Add(new DocsHeading(block.Id, text, level, pill));
            return block;
        }

        // A trailing "<!-- level N -->" on a heading; any other trailing directive is checked and dropped.
        private static int TakePill(ParseContext _context, ref string _text, int _lineNumber)
        {
            int pill = -1;
            foreach (DocsDirective directive in TakeTrailingDirectives(_context, ref _text, _lineNumber))
            {
                bool isLevel = directive.Name == "level";
                int value;
                bool parsed = int.TryParse(directive.Argument(0), out value);
                if (isLevel && parsed && value >= 0 && value <= 3)
                {
                    pill = value;
                    continue;
                }
                if (isLevel)
                {
                    Report(_context, _lineNumber, "level must be 0, 1, 2 or 3");
                }
            }
            return pill;
        }

        // Removes every "<!-- ... -->" from the end of a line and returns them as directives.
        private static List<DocsDirective> TakeTrailingDirectives(ParseContext _context, ref string _text, int _lineNumber)
        {
            List<DocsDirective> found = new List<DocsDirective>();
            MatchCollection matches = DirectivePattern.Matches(_text);
            foreach (Match match in matches)
            {
                DocsDirective directive = ReadDirective(_context, match.Groups[1].Value, _lineNumber);
                if (directive != null)
                {
                    found.Add(directive);
                }
            }
            _text = DirectivePattern.Replace(_text, "").Trim();
            return found;
        }

        private static string UniqueId(ParseContext _context, string _slug)
        {
            int count;
            if (!_context.UsedIds.TryGetValue(_slug, out count))
            {
                _context.UsedIds[_slug] = 1;
                return _slug;
            }
            _context.UsedIds[_slug] = count + 1;
            return _slug + "-" + count;
        }

        private static DocsBlock ParseDirectiveLine(ParseContext _context, string _trimmed, int _lineNumber)
        {
            Match match = DirectivePattern.Match(_trimmed);
            DocsDirective directive = ReadDirective(_context, match.Groups[1].Value, _lineNumber);
            if (directive == null)
            {
                return null;
            }
            DocsBlock block = new DocsBlock(DocsBlockKind.Directive, _lineNumber);
            block.Directive = directive;
            return block;
        }

        // Splits "name arg "quoted arg"" into a directive. Unknown names are reported and dropped.
        private static DocsDirective ReadDirective(ParseContext _context, string _body, int _lineNumber)
        {
            List<string> words = SplitArguments(_body);
            if (words.Count == 0)
            {
                Report(_context, _lineNumber, "empty directive");
                return null;
            }
            string name = words[0];
            words.RemoveAt(0);
            if (_context.Known != null && !_context.Known.Contains(name))
            {
                Report(_context, _lineNumber, "unknown directive '" + name + "'");
                return null;
            }
            return new DocsDirective(name, words, _lineNumber, _body);
        }

        /// <summary>Splits on spaces, keeping text in double quotes together (without the quotes).</summary>
        public static List<string> SplitArguments(string _body)
        {
            List<string> words = new List<string>();
            StringBuilder current = new StringBuilder();
            bool inQuotes = false;
            bool hasWord = false;
            foreach (char character in _body)
            {
                if (character == '"')
                {
                    inQuotes = !inQuotes;
                    hasWord = true;
                    continue;
                }
                bool endsWord = character == ' ' && !inQuotes;
                if (endsWord)
                {
                    FlushWord(words, current, ref hasWord);
                    continue;
                }
                current.Append(character);
                hasWord = true;
            }
            FlushWord(words, current, ref hasWord);
            return words;
        }

        private static void FlushWord(List<string> _words, StringBuilder _current, ref bool _hasWord)
        {
            if (_hasWord)
            {
                _words.Add(_current.ToString());
            }
            _current.Length = 0;
            _hasWord = false;
        }

        private static DocsBlock ParseImage(ParseContext _context, Match _match, ref int _index)
        {
            int lineNumber = _index;
            DocsBlock block = new DocsBlock(DocsBlockKind.Image, lineNumber);
            block.Text = _match.Groups[1].Value;
            block.ImagePath = _match.Groups[2].Value;
            bool hasNextLine = _index < _context.Lines.Length;
            if (!hasNextLine)
            {
                return block;
            }
            string next = _context.Lines[_index].Trim();
            Match directive = DirectivePattern.Match(next);
            bool isMarks = directive.Success && directive.Groups[1].Value.StartsWith("marks ");
            if (!isMarks)
            {
                return block;
            }
            _index++;
            string body = directive.Groups[1].Value.Substring("marks ".Length);
            ReadMarks(_context, body, block);
            return block;
        }

        // "1@0.12,0.30 "Scenes tab"; 2@0.55,0.30 "Search"". A mark outside 0 to 1 is reported and dropped.
        private static void ReadMarks(ParseContext _context, string _body, DocsBlock _image)
        {
            foreach (string part in _body.Split(';'))
            {
                string trimmed = part.Trim();
                if (trimmed.Length == 0)
                {
                    continue;
                }
                Match match = MarkPattern.Match(trimmed);
                if (!match.Success)
                {
                    Report(_context, _image.Line + 1, "a mark must read N@x,y \"label\": " + trimmed);
                    continue;
                }
                float x = float.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
                float y = float.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
                bool inside = x >= 0f && x <= 1f && y >= 0f && y <= 1f;
                if (!inside)
                {
                    Report(_context, _image.Line + 1, "mark " + match.Groups[1].Value + " is outside the image (x and y go from 0 to 1)");
                    continue;
                }
                int number = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                _image.Marks.Add(new DocsMark(number, x, y, match.Groups[4].Value));
            }
        }

        private static DocsBlock ParseCode(ParseContext _context, ref int _index)
        {
            DocsBlock block = new DocsBlock(DocsBlockKind.Code, _index + 1);
            block.Info = _context.Lines[_index].Trim().Substring(3).Trim();
            _index++;
            List<string> body = new List<string>();
            while (_index < _context.Lines.Length)
            {
                string line = _context.Lines[_index];
                _index++;
                if (line.Trim().StartsWith("```"))
                {
                    break;
                }
                body.Add(line);
            }
            block.Text = string.Join("\n", body);
            return block;
        }

        // A quote is a callout when its first line is [!NOTE], [!WARNING] or [!TIP]. Its inside is parsed as blocks.
        private static DocsBlock ParseQuote(ParseContext _context, ref int _index)
        {
            int lineNumber = _index + 1;
            List<string> inner = new List<string>();
            while (_index < _context.Lines.Length)
            {
                string trimmed = _context.Lines[_index].Trim();
                if (!trimmed.StartsWith(">"))
                {
                    break;
                }
                inner.Add(trimmed.Substring(1).TrimStart());
                _index++;
            }
            DocsBlock block = new DocsBlock(DocsBlockKind.Quote, lineNumber);
            Match callout = inner.Count > 0 ? CalloutPattern.Match(inner[0]) : Match.Empty;
            if (callout.Success)
            {
                block = new DocsBlock(DocsBlockKind.Callout, lineNumber);
                block.Info = callout.Groups[1].Value.ToLowerInvariant();
                inner.RemoveAt(0);
            }
            ParseContext innerContext = new ParseContext(_context.Page, inner.ToArray(), _context.Known);
            int innerIndex = 0;
            ParseBlocks(innerContext, ref innerIndex, block.Children, false);
            return block;
        }

        private static DocsBlock ParseTable(ParseContext _context, ref int _index)
        {
            DocsBlock block = new DocsBlock(DocsBlockKind.Table, _index + 1);
            while (_index < _context.Lines.Length)
            {
                string trimmed = _context.Lines[_index].Trim();
                if (!trimmed.StartsWith("|"))
                {
                    break;
                }
                _index++;
                bool isDivider = trimmed.Replace("|", "").Replace("-", "").Replace(":", "").Trim().Length == 0;
                if (isDivider)
                {
                    continue;
                }
                block.Rows.Add(SplitRow(trimmed));
            }
            return block;
        }

        // Splits "| a | b |" into cells. A "|" inside backticks stays in its cell.
        private static List<string> SplitRow(string _row)
        {
            string inner = _row.Trim();
            if (inner.StartsWith("|"))
            {
                inner = inner.Substring(1);
            }
            if (inner.EndsWith("|"))
            {
                inner = inner.Substring(0, inner.Length - 1);
            }
            List<string> cells = new List<string>();
            StringBuilder cell = new StringBuilder();
            bool inCode = false;
            foreach (char character in inner)
            {
                if (character == '`')
                {
                    inCode = !inCode;
                }
                if (character == '|' && !inCode)
                {
                    cells.Add(cell.ToString().Trim());
                    cell.Length = 0;
                    continue;
                }
                cell.Append(character);
            }
            cells.Add(cell.ToString().Trim());
            return cells;
        }

        private static DocsBlock ParseList(ParseContext _context, ref int _index, int _indent)
        {
            Match first = NumberedPattern.Match(_context.Lines[_index]);
            DocsBlock block = new DocsBlock(DocsBlockKind.List, _index + 1);
            block.Ordered = first.Success;
            while (_index < _context.Lines.Length)
            {
                string line = _context.Lines[_index];
                if (line.Trim().Length == 0)
                {
                    bool continues = NextIsListItem(_context, _index + 1, _indent, block.Ordered);
                    if (!continues)
                    {
                        break;
                    }
                    _index++;
                    continue;
                }
                Match bullet = BulletPattern.Match(line);
                Match numbered = NumberedPattern.Match(line);
                Match item = numbered.Success ? numbered : bullet;
                int indent = IndentOf(line);
                if (!item.Success)
                {
                    bool isContinuation = indent > _indent && block.Items.Count > 0;
                    if (!isContinuation)
                    {
                        break;
                    }
                    DocsListItem last = block.Items[block.Items.Count - 1];
                    last.Text = last.Text + " " + line.Trim();
                    _index++;
                    continue;
                }
                if (indent < _indent)
                {
                    break;
                }
                bool otherKind = indent == _indent && numbered.Success != block.Ordered;
                if (otherKind)
                {
                    break;
                }
                if (indent > _indent && block.Items.Count > 0)
                {
                    DocsListItem parent = block.Items[block.Items.Count - 1];
                    parent.NestedList = ParseList(_context, ref _index, indent);
                    continue;
                }
                block.Items.Add(ReadItem(_context, item.Groups[2].Value, _index + 1));
                _index++;
            }
            return block;
        }

        // After a blank line the list goes on only with an item of the same kind (a numbered list after a bullet list
        // is a new list), or a deeper item.
        private static bool NextIsListItem(ParseContext _context, int _index, int _indent, bool _ordered)
        {
            if (_index >= _context.Lines.Length)
            {
                return false;
            }
            string line = _context.Lines[_index];
            int indent = IndentOf(line);
            if (indent > _indent)
            {
                return BulletPattern.IsMatch(line) || NumberedPattern.IsMatch(line);
            }
            bool sameKind = _ordered ? NumberedPattern.IsMatch(line) : BulletPattern.IsMatch(line);
            return sameKind && indent == _indent;
        }

        private static DocsListItem ReadItem(ParseContext _context, string _text, int _lineNumber)
        {
            string text = _text;
            List<DocsDirective> directives = TakeTrailingDirectives(_context, ref text, _lineNumber);
            DocsListItem item = new DocsListItem(text);
            foreach (DocsDirective directive in directives)
            {
                if (directive.Name == "done-when")
                {
                    item.DoneWhen = directive.Argument(0);
                    continue;
                }
                item.Directives.Add(directive);
            }
            return item;
        }

        private static DocsBlock ParseParagraph(ParseContext _context, ref int _index)
        {
            DocsBlock block = new DocsBlock(DocsBlockKind.Paragraph, _index + 1);
            List<string> parts = new List<string>();
            while (_index < _context.Lines.Length)
            {
                string line = _context.Lines[_index];
                string trimmed = line.Trim();
                bool endsParagraph = trimmed.Length == 0 || StartsOtherBlock(line);
                if (endsParagraph && parts.Count > 0)
                {
                    break;
                }
                parts.Add(trimmed);
                _index++;
            }
            block.Text = string.Join(" ", parts);
            return block;
        }

        private static bool StartsOtherBlock(string _line)
        {
            string trimmed = _line.Trim();
            bool isFence = trimmed.StartsWith("```");
            bool isHeading = HeadingPattern.IsMatch(trimmed);
            bool isQuoteOrTable = trimmed.StartsWith(">") || trimmed.StartsWith("|");
            bool isItem = BulletPattern.IsMatch(_line) || NumberedPattern.IsMatch(_line);
            bool isMarkup = trimmed.StartsWith("<!--") || trimmed.StartsWith("<details>") || trimmed == "</details>";
            bool isImage = ImagePattern.IsMatch(trimmed);
            return isFence || isHeading || isQuoteOrTable || isItem || isMarkup || isImage;
        }

        private static int IndentOf(string _line)
        {
            int count = 0;
            foreach (char character in _line)
            {
                if (character == ' ')
                {
                    count++;
                    continue;
                }
                if (character == '\t')
                {
                    count += 4;
                    continue;
                }
                break;
            }
            return count;
        }

        private static void Report(ParseContext _context, int _lineNumber, string _what)
        {
            _context.Page.Problems.Add(_context.Page.Path + ":" + _lineNumber + ": " + _what);
        }
    }
}
