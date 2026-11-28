using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>How the "over each parent" table shares its width. A value column is as wide as its longest content, so
    /// widening the Inspector never feeds Parent while a value is cut; Parent takes everything left over. When the values
    /// would leave Parent too little room, the table stacks instead.</summary>
    internal static class StateTableColumns
    {
        /// <summary>Wide enough for any column heading.</summary>
        public const float LeastValue = 40f;
        /// <summary>The least room worth giving a long parent name beside the values.</summary>
        public const float ParentLeast = 120f;
        /// <summary>A row's side padding and the table's border.</summary>
        public const float RowChrome = 14f;

        public static float[] ValueWidths(IReadOnlyList<float> _needs)
        {
            return _needs.Select(_need => Mathf.Max(_need, LeastValue)).ToArray();
        }

        public static bool Fits(float _tableWidth, IReadOnlyList<float> _valueWidths, float _parentNeed)
        {
            return _valueWidths.Sum() + Mathf.Min(_parentNeed, ParentLeast) + RowChrome <= _tableWidth;
        }
    }
}
