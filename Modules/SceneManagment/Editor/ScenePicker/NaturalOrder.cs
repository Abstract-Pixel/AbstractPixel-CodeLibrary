namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>Compares names the way people read them, ignoring case: "Level 2" comes before "Level 10".</summary>
    public static class NaturalOrder
    {
        public static int Compare(string _a, string _b)
        {
            string a = _a ?? string.Empty;
            string b = _b ?? string.Empty;
            int indexA = 0;
            int indexB = 0;
            while (indexA < a.Length && indexB < b.Length)
            {
                bool bothDigits = char.IsDigit(a[indexA]) && char.IsDigit(b[indexB]);
                if (bothDigits)
                {
                    int byNumber = CompareNumbers(a, ref indexA, b, ref indexB);
                    if (byNumber != 0)
                    {
                        return byNumber;
                    }
                    continue;
                }
                int byLetter = char.ToLowerInvariant(a[indexA]).CompareTo(char.ToLowerInvariant(b[indexB]));
                if (byLetter != 0)
                {
                    return byLetter;
                }
                indexA++;
                indexB++;
            }
            int leftInA = a.Length - indexA;
            int leftInB = b.Length - indexB;
            return leftInA.CompareTo(leftInB);
        }

        // Compares the digit runs at each index by length first (leading zeros dropped), so no number overflows.
        private static int CompareNumbers(string _a, ref int _indexA, string _b, ref int _indexB)
        {
            string numberA = ReadDigits(_a, ref _indexA).TrimStart('0');
            string numberB = ReadDigits(_b, ref _indexB).TrimStart('0');
            if (numberA.Length != numberB.Length)
            {
                return numberA.Length.CompareTo(numberB.Length);
            }
            return string.CompareOrdinal(numberA, numberB);
        }

        private static string ReadDigits(string _text, ref int _index)
        {
            int start = _index;
            while (_index < _text.Length && char.IsDigit(_text[_index]))
            {
                _index++;
            }
            return _text.Substring(start, _index - start);
        }
    }
}
