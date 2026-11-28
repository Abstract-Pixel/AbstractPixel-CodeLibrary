using System;
using System.Linq;

namespace AbstractPixel.GameManagement.Debugging
{
    /// <summary>Shares the stack line's room between state names. Flex alone would shorten "Play" to make room for a long name.</summary>
    internal static class StackNameFit
    {
        /// <summary>The width each name gets: short names stay whole, and the long ones share what is left evenly.</summary>
        public static float[] Widths(float[] _wanted, float _room)
        {
            float[] widths = new float[_wanted.Length];
            float room = Math.Max(0f, _room);
            int[] shortestFirst = Enumerable.Range(0, _wanted.Length).OrderBy(_index => _wanted[_index]).ToArray();
            for (int i = 0; i < shortestFirst.Length; i++)
            {
                int index = shortestFirst[i];
                float evenShare = room / (shortestFirst.Length - i);
                widths[index] = Math.Min(_wanted[index], evenShare);
                room -= widths[index];
            }
            return widths;
        }
    }
}
