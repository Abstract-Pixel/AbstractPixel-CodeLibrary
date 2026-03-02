using System;
using UnityEngine;

public class AnimationCommandEventBroadcaster
{
    public static event Action<IAnimationCommand> OnAnimationStartedEvent;
    public static event Action<IAnimationCommand> OnAnimationInProgressEvent;
    public static event Action<IAnimationCommand> OnAnimationEndedEvent;

    public static void RaiseOnAnimationStartedEvent(IAnimationCommand command)
    {
        OnAnimationStartedEvent?.Invoke(command);
    }

    public static void RaiseOnAnimationInProgressEvent(IAnimationCommand command)
    {
        OnAnimationInProgressEvent?.Invoke(command);
    }

    public static void RaiseOnAnimationEndedEvent(IAnimationCommand command)
    {
        OnAnimationEndedEvent?.Invoke(command);
    }


    [RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void ResetEventSubscriptions()
    {
        OnAnimationEndedEvent = null;
        OnAnimationInProgressEvent = null;
        OnAnimationStartedEvent = null;
    }
}
