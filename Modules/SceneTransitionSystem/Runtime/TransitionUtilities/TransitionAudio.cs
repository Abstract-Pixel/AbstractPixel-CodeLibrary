using UnityEngine;
#if ABSTRACTPIXEL_BROAUDIO
using Ami.BroAudio;
#endif
using AbstractPixel.SceneTransitions;

public class TransitionAudio : MonoBehaviour
{
#if ABSTRACTPIXEL_BROAUDIO
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
#else
    private void OnEnable()
    {
        Debug.LogError($"[Transitions] TransitionAudio on '{name}' needs the BroAudio package (com.ami.broaudio). Install it or remove this component.", this);
    }
#endif
}
