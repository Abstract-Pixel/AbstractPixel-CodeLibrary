using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Samples
{
    /// <summary>
    /// The on-screen label every scene sample shows: a box in the top-left corner with "Sample NN · Name" and up to five
    /// short lines. Lines are typed in the Inspector, or set by a sample script with <see cref="SetLine"/>. A line may hold
    /// several rows separated by new lines. Drawn with IMGUI, so no font asset is needed.
    /// </summary>
    public class SampleLabel : MonoBehaviour
    {
        public const int MaxLines = 5;

        private const float BoxX = 10f;
        private const float BoxY = 10f;
        private const float BoxWidth = 460f;
        private const float Padding = 8f;

        [SerializeField] private string title = "Sample NN · Name";
        [SerializeField] private List<string> lines = new List<string>();

        private readonly float[] lineHeights = new float[MaxLines];
        private GUIStyle titleStyle;
        private GUIStyle lineStyle;
        private bool heightsAreStale = true;
        private float titleHeight;

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                heightsAreStale = true;
            }
        }

        /// <summary>The text of a line, or an empty string when the line is not set.</summary>
        public string GetLine(int _index)
        {
            if (_index < 0 || _index >= lines.Count)
            {
                return string.Empty;
            }
            return lines[_index];
        }

        /// <summary>Sets one of the five lines (0 to 4). An index outside that range is ignored.</summary>
        public void SetLine(int _index, string _text)
        {
            if (_index < 0 || _index >= MaxLines)
            {
                return;
            }
            FillLinesUpTo(_index);
            if (lines[_index] == _text)
            {
                return;
            }
            lines[_index] = _text;
            heightsAreStale = true;
        }

        private void OnValidate()
        {
            heightsAreStale = true;
        }

        private void OnGUI()
        {
            CreateStylesOnce();
            if (heightsAreStale)
            {
                MeasureHeights();
            }

            float boxHeight = TotalHeight();
            Rect box = new Rect(BoxX, BoxY, BoxWidth, boxHeight);
            GUI.Box(box, GUIContent.none);

            float y = BoxY + Padding;
            Rect titleRect = new Rect(BoxX + Padding, y, BoxWidth - Padding * 2f, titleHeight);
            GUI.Label(titleRect, title, titleStyle);
            y += titleHeight;

            int lineCount = Mathf.Min(lines.Count, MaxLines);
            for (int index = 0; index < lineCount; index++)
            {
                if (string.IsNullOrEmpty(lines[index]))
                {
                    continue;
                }
                Rect lineRect = new Rect(BoxX + Padding, y, BoxWidth - Padding * 2f, lineHeights[index]);
                GUI.Label(lineRect, lines[index], lineStyle);
                y += lineHeights[index];
            }
        }

        // The styles are made the first time OnGUI runs, because GUI.skin exists only there.
        private void CreateStylesOnce()
        {
            if (titleStyle != null)
            {
                return;
            }
            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = 15;
            titleStyle.wordWrap = true;
            lineStyle = new GUIStyle(GUI.skin.label);
            lineStyle.fontSize = 13;
            lineStyle.wordWrap = true;
        }

        // Heights are measured only after a line changes, so OnGUI allocates nothing on most frames.
        private void MeasureHeights()
        {
            float textWidth = BoxWidth - Padding * 2f;
            titleHeight = titleStyle.CalcHeight(new GUIContent(title), textWidth);
            int lineCount = Mathf.Min(lines.Count, MaxLines);
            for (int index = 0; index < MaxLines; index++)
            {
                bool hasText = index < lineCount && string.IsNullOrEmpty(lines[index]) == false;
                if (hasText == false)
                {
                    lineHeights[index] = 0f;
                    continue;
                }
                lineHeights[index] = lineStyle.CalcHeight(new GUIContent(lines[index]), textWidth);
            }
            heightsAreStale = false;
        }

        private float TotalHeight()
        {
            float height = Padding * 2f + titleHeight;
            for (int index = 0; index < MaxLines; index++)
            {
                height += lineHeights[index];
            }
            return height;
        }

        private void FillLinesUpTo(int _index)
        {
            while (lines.Count <= _index)
            {
                lines.Add(string.Empty);
            }
        }
    }
}
