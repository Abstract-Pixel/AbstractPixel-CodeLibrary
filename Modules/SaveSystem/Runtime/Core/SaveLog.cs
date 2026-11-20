using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Every message the save system writes to the Console goes through here.
    /// Tests that make a mistake on purpose call <see cref="Expect"/> first: the matching message is checked
    /// but not printed, so a test run leaves no errors in the Console. In a game nothing is ever expected,
    /// so every message prints.
    /// </summary>
    internal static class SaveLog
    {
        readonly struct Expectation
        {
            public readonly LogType Type;
            public readonly Regex Pattern;

            public Expectation(LogType type, Regex pattern)
            {
                Type = type;
                Pattern = pattern;
            }

            public override string ToString() => $"{Type}: {Pattern}";
        }

        static readonly List<Expectation> expected = new List<Expectation>();
        static readonly object gate = new object();

        [HideInCallstack]
        public static void Error(string message, Object context = null) => Write(LogType.Error, message, context);

        [HideInCallstack]
        public static void Warning(string message, Object context = null) => Write(LogType.Warning, message, context);

        [HideInCallstack]
        public static void Info(string message, Object context = null) => Write(LogType.Log, message, context);

        /// <summary>For tests: the next message of this type matching the pattern is taken instead of printed.</summary>
        public static void Expect(LogType type, Regex pattern)
        {
            lock (gate)
            {
                expected.Add(new Expectation(type, pattern));
            }
        }

        /// <summary>For tests: forgets every expectation and returns the ones no message matched.</summary>
        public static List<string> TakeUnmet()
        {
            lock (gate)
            {
                var unmet = expected.ConvertAll(expectation => expectation.ToString());
                expected.Clear();
                return unmet;
            }
        }

        [HideInCallstack]
        static void Write(LogType type, string message, Object context)
        {
            if (TryTakeExpected(type, message))
            {
                return;
            }
            Debug.unityLogger.Log(type, (object)message, context);
        }

        static bool TryTakeExpected(LogType type, string message)
        {
            lock (gate)
            {
                for (int i = 0; i < expected.Count; i++)
                {
                    if (expected[i].Type == type && expected[i].Pattern.IsMatch(message))
                    {
                        expected.RemoveAt(i);
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
