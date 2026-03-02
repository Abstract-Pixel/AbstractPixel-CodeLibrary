using TMPro;
using UnityEngine;

/// <summary>
/// Displays the current value and max value of an IntSO as a fraction (e.g., "5 / 10").
/// Listens to the IntSO's OnValueChanged event to update the text automatically.
/// </summary>
public class IntSOFractionTextDisplayer : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("The TextMeshPro UI element to display the text on.")]
    [SerializeField] private TMP_Text displayText;

    [Header("Data Source")]
    [Tooltip("The IntSO to read the CurrentValue (numerator) and MaxValue (denominator) from.")]
    [SerializeField] private IntSO intSo;

    [Header("Formatting")]
    [SerializeField] private string prefix;
    
    [Tooltip("The character or string to use as the separator between the numerator and denominator.")]
    [SerializeField] private string separator = " / ";

    private void OnEnable()
    {
        if (intSo != null)
        {
            intSo.OnValueChanged += UpdateDisplayText;
            UpdateDisplayText();
        }
    }

    private void OnDisable()
    {
        if (intSo != null)
        {
            intSo.OnValueChanged -= UpdateDisplayText;
        }
    }

    /// <summary>
    /// Updates the display text to show the fraction.
    /// </summary>
    public void UpdateDisplayText()
    {
        if (displayText == null || intSo == null) return;

        // Construct the final string using the prefix, values, and separator.
        displayText.text = $"{prefix}{intSo.CurrentValue}{separator}{intSo.MaxValue}";
    }
}