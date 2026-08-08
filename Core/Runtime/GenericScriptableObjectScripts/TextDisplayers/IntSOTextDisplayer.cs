using AbstractPixel.Core;
using TMPro;
using UnityEngine;
using static IntSOTextDisplayer;

public class IntSOTextDisplayer : MonoBehaviour
{
    public enum SignPlacement
    {
        AfterPrefix,  // Default: Prefix + "-" + Value (e.g., "HP: -5")
        BeforePrefix  // Currency style: "-" + Prefix + Value (e.g., "-$5")
    }
    [SerializeField] string startTextAddon;
    [SerializeField] string endTextAddon;
    [SerializeField] SignPlacement signPlacement = SignPlacement.AfterPrefix;
    [SerializeField] protected TMP_Text displayText;
    [SerializeField] protected IntSO intSo;

    protected virtual void OnEnable()
    {
        if (intSo != null)
        {
            intSo.OnValueChanged += UpdateDisplayText;
            UpdateDisplayText();
        }

    }

    protected virtual void OnDisable()
    {
        if (intSo != null)
        {
            intSo.OnValueChanged -= UpdateDisplayText;
        }

    }
    public  virtual void UpdateDisplayText()
    {
        if (displayText == null || intSo == null) return;
        ShowTextWithFormatting(intSo.CurrentValue);
    }

    protected void ShowTextWithFormatting(int _valueToSHow)
    {
        if (_valueToSHow < 0)
        {
            int absValue = Mathf.Abs(_valueToSHow);
            if (signPlacement == SignPlacement.BeforePrefix)
            {
                displayText.text = $"-{startTextAddon}{absValue}{endTextAddon}";
            }
            else
            {
                displayText.text = $"{startTextAddon}-{absValue}{endTextAddon}";
            }
        }
        else
        {
            displayText.text = $"{startTextAddon}{_valueToSHow}{endTextAddon}";
        }
    }
}