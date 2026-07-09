using AbstractPixel.Core;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Replaces a TextMeshPro text component's text with a randomly selected string from a pre-defined list.
/// </summary>
public class RandomTextReplacer : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private TMP_Text textComponent;

    [Tooltip("List of possible texts to randomly select from.")]
    [SerializeField] private List<RandomTextData> textOptions = new List<RandomTextData>();

    [Header("Configuration")]
    [Tooltip("If true, the text replacement will trigger automatically on Start.")]
    [SerializeField] private bool replaceOnStart = true;

    [Tooltip("If true, uses weighted random selection based on the probability slider. If false, all options have an equal uniform chance.")]
    [SerializeField] private bool useProbability = false;

    [Header("Events")]
    [Tooltip("Fired when the text is successfully replaced. Passes the new string as a parameter.")]
    public UnityEvent<string> OnTextChanged;

    private void Start()
    {
        if (replaceOnStart)
        {
            ReplaceText();
        }
    }

    /// <summary>
    /// Randomly selects an item from the list and applies it to the TMP_Text component.
    /// Can be called manually from UI Buttons or other scripts.
    /// </summary>
    public void ReplaceText()
    {
        RandomTextData selectedData = default;

        if (!useProbability)
        {
            // Uniform Randomization: Pick a random item with equal chance
            int randomIndex = Random.Range(0, textOptions.Count);
            selectedData = textOptions[randomIndex];
        }
        else
        {
            if (textOptions.All(item => item.probability == 0))
            {
                int randomIndex = Random.Range(0, textOptions.Count);
                selectedData = textOptions[randomIndex];
            }
            else
            {
                selectedData = PickRandomTextOptionBasedOnProbability();
            }
        }

        textComponent.text = selectedData.text;
        if (selectedData.changeColor)
        {
            textComponent.color = selectedData.color;
        }
        OnTextChanged?.Invoke(selectedData.text);
    }

    private RandomTextData PickRandomTextOptionBasedOnProbability()
    {
        while (true)
        {
            int randomIndex = Random.Range(0, textOptions.Count);
            RandomTextData chosenData = textOptions[randomIndex];

            float diceRoll = Random.value;
            // (e.g. if probability is 0.8, we have an 80% chance the dice roll is lower than it)
            if (diceRoll <= chosenData.probability)
            {
                return chosenData;
            }
        }
    }
}
