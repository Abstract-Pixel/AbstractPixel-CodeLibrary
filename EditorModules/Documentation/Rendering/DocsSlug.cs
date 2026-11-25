using System.Text;

namespace AbstractPixel.EditorModules.Documentation
{
    /// <summary>
    /// GitHub-style heading ids: lowercase, letters, digits, spaces, hyphens and underscores kept, every other character
    /// removed, then spaces turned into hyphens. "Start-up and Play from any scene" becomes "start-up-and-play-from-any-scene".
    /// </summary>
    public static class DocsSlug
    {
        public static string From(string _text)
        {
            if (string.IsNullOrEmpty(_text))
            {
                return "";
            }
            string lower = _text.Trim().ToLowerInvariant();
            StringBuilder slug = new StringBuilder(lower.Length);
            foreach (char character in lower)
            {
                bool isKept = char.IsLetterOrDigit(character) || character == '-' || character == '_';
                if (isKept)
                {
                    slug.Append(character);
                    continue;
                }
                if (character == ' ')
                {
                    slug.Append('-');
                }
            }
            return slug.ToString();
        }
    }
}
