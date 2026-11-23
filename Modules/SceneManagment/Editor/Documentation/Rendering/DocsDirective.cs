using System.Collections.Generic;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// One directive, written as an HTML comment so GitHub hides it: a name and its arguments, such as
    /// <c>&lt;!-- action ping "Assets/A.asset" "Ping it" --&gt;</c>. Quoted arguments keep their spaces.
    /// </summary>
    public class DocsDirective
    {
        public string Name { get; private set; }
        public List<string> Arguments { get; private set; }
        public int Line { get; private set; }
        public string RawText { get; private set; }

        public DocsDirective(string _name, List<string> _arguments, int _line, string _rawText)
        {
            Name = _name;
            Arguments = _arguments;
            Line = _line;
            RawText = _rawText;
        }

        /// <summary>The argument at this index, or an empty string when there are fewer.</summary>
        public string Argument(int _index)
        {
            if (_index < 0 || _index >= Arguments.Count)
            {
                return "";
            }
            return Arguments[_index];
        }
    }
}
