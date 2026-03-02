using TMPro;
using UnityEngine;

public class IntSOTextDisplayer : MonoBehaviour
{
    [SerializeField] string startTextAddon;
    [SerializeField] string endTextAddon;
    [SerializeField] TMP_Text displayText;
    [SerializeField] IntSO intSo;

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
    public void UpdateDisplayText()
    {
        if (displayText == null || intSo == null) return;
        displayText.text = $"{startTextAddon}{intSo.CurrentValue}{endTextAddon}";
    }
}