using AbstractPixel.Core;
using System;
using TMPro;
using UnityEngine;

namespace AbstractPixel.Core.UI
{
    // The [Flags] attribute allows multiple selections in the Unity Inspector
    [Flags]
    public enum TimeDisplayFormat
    {
        None = 0,
        Hours = 1 << 0,       // 1
        Minutes = 1 << 1,     // 2
        Seconds = 1 << 2,     // 4
        Milliseconds = 1 << 3 // 8
    }

    public class TimeSOTextDisplayer : MonoBehaviour
    {
        [Header("Data & UI References")]
        [SerializeField] private FloatSO floatSo;
        [SerializeField] private TMP_Text displayText;

        [Header("Text Formatting")]
        [SerializeField] private string startTextAddon;
        [SerializeField] private string endTextAddon;

        [Tooltip("Select which time units to display. They will be separated by colons automatically.")]
        [SerializeField] private TimeDisplayFormat displayFormat = TimeDisplayFormat.Minutes | TimeDisplayFormat.Seconds;

        [Tooltip("Configure how many digits of precision to show for milliseconds (1 to 3).")]
        [Range(1, 3)]
        [SerializeField] private int millisecondDigitCount = 2;

        private void OnEnable()
        {
            if (floatSo != null)
            {
                floatSo.OnValueChanged += UpdateDisplayText;
                UpdateDisplayText(); // Force initial update
            }
        }

        private void OnDisable()
        {
            if (floatSo != null)
            {
                floatSo.OnValueChanged -= UpdateDisplayText;
            }
        }

        public void UpdateDisplayText()
        {
            // Silent guard clause to prevent Null Reference Exceptions
            if (displayText == null || floatSo == null) return;

            // 1. Fetch raw total seconds
            float totalSeconds = floatSo.CurrentValue;

            // 2. Perform Truncated Math
            // We use Mathf.FloorToInt to ensure 1.9s does not round up to 2s.
            int hours = Mathf.FloorToInt(totalSeconds / 3600f);
            int minutes = Mathf.FloorToInt((totalSeconds % 3600f) / 60f);
            int seconds = Mathf.FloorToInt(totalSeconds % 60f);

            // 3. Milliseconds Calculation with Dynamic Precision
            float fraction = totalSeconds - Mathf.Floor(totalSeconds);
            int multiplier = 100;
            string msFormat = "00";

            if (millisecondDigitCount == 1)
            {
                multiplier = 10;
                msFormat = "0";
            }
            else if (millisecondDigitCount == 2)
            {
                multiplier = 100;
                msFormat = "00";
            }
            else if (millisecondDigitCount == 3)
            {
                multiplier = 1000;
                msFormat = "000";
            }

            int milliseconds = Mathf.FloorToInt(fraction * multiplier);

            // 4. Build the formatted string dynamically based on selected Enum Flags
            string timeString = "";
            bool needsSeparator = false;

            if (displayFormat.HasFlag(TimeDisplayFormat.Hours))
            {
                timeString += hours.ToString("00");
                needsSeparator = true;
            }

            if (displayFormat.HasFlag(TimeDisplayFormat.Minutes))
            {
                if (needsSeparator) timeString += ":";
                timeString += minutes.ToString("00");
                needsSeparator = true;
            }

            if (displayFormat.HasFlag(TimeDisplayFormat.Seconds))
            {
                if (needsSeparator) timeString += ":";
                timeString += seconds.ToString("00");
                needsSeparator = true;
            }

            if (displayFormat.HasFlag(TimeDisplayFormat.Milliseconds))
            {
                if (needsSeparator) timeString += ":";
                timeString += milliseconds.ToString(msFormat);
            }

            // 5. Apply to UI
            displayText.text = $"{startTextAddon}{timeString}{endTextAddon}";
        }
    }
}