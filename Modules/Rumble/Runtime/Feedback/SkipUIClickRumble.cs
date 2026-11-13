using UnityEngine;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// Marker: clicking this UI element does not play the general UI click rumble (UIRumbleFeedback).
    /// Use on buttons that already play their own shake (e.g. a level's Play button with its own roar).
    /// </summary>
    [DisallowMultipleComponent]
    public class SkipUIClickRumble : MonoBehaviour
    {
    }
}
