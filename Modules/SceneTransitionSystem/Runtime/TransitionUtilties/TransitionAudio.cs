using UnityEngine;
using Ami.BroAudio;
using AbstractPixel.SceneTransitions;

public class TransitionAudio : MonoBehaviour
{
    [SerializeField] private SoundID FadeInSound;
    [SerializeField] private SoundID FadeOutSound;
    private void OnEnable()
    {
        TransitionEventBus.OnStartHideScreenTransition += PlayFadeInSound;
        TransitionEventBus.OnStartShowScreenTransition += PlayFadeOutSound;
    }

    private void OnDisable()
    {
        TransitionEventBus.OnStartHideScreenTransition -= PlayFadeInSound;
        TransitionEventBus.OnStartShowScreenTransition -= PlayFadeOutSound;
    }

    private void PlayFadeInSound()
    {
        if (FadeInSound.IsValid())
        {
            BroAudio.Play(FadeInSound);
        }
    }

    private void PlayFadeOutSound()
    {
        if(FadeOutSound.IsValid())
        {
            BroAudio.Play(FadeOutSound);
        }
    }
}
