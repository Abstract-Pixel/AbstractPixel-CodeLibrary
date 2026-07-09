using UnityEngine;

namespace AbstractPixel.Core
{
    [System.Serializable]
    public struct RandomTextData
    {
        [Tooltip("The text to display.")]
        [TextArea(2, 5)]
        public string text;

        [Tooltip("Should the text color be changed when this option is chosen?")]
        public bool changeColor;

        [Tooltip("The color to apply if changeColor is true.")]
        public Color color;

        [Tooltip("The weight/chance of this item being picked when useProbability is enabled.")]
        [Range(0f, 1f)]
        public float probability;
    }
}
