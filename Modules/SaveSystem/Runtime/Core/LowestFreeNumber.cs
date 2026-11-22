using System.Collections.Generic;

namespace AbstractPixel.SaveSystem
{
    /// <summary>Numbers that start at 1 and are reused: slot numbers and the numbers in automatic save IDs.</summary>
    internal static class LowestFreeNumber
    {
        /// <summary>The lowest number from 1 up that isn't taken.</summary>
        public static int NotIn(ICollection<int> takenNumbers)
        {
            int number = 1;
            while (takenNumbers.Contains(number))
            {
                number++;
            }
            return number;
        }
    }
}
