# ISceneTransitionExecutor

<!-- layout new -->

The one thing the scene system asks to hide and show the screen around a scene change (ADR 0001).

<!-- type-header AbstractPixel.SceneManagement.ISceneTransitionExecutor -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/PublicAPI/ISceneTransitionExecutor.cs" "Select the script" -->

Read more in [Screen and transitions](../guide/screen-and-transitions.md#use-the-basic-fade).

## Start here

The transitions module provides it: `TransitionManager` registers itself with `SceneActions.RegisterTransitionExecutor` once a transition profile is set. Implement it yourself only to replace that module.

| You want to | Use | Why |
|---|---|---|
| Cover the screen before scenes change | `TransitionToHideScreen` | Finishes once the screen counts as covered. |
| Uncover it after | `TransitionToShowScreen` | Finishes once the screen is uncovered; calls its argument once when the screen counts as visible. |

**Called for you, leave alone**

- **No executor:** with none registered, scenes change with no effect.
- **Refused changes:** the scene system calls Hide screen only after it accepted a change, so a refused change never covers the screen.
- **Release:** an executor that never calls the argument of `TransitionToShowScreen` is released at the end of Show screen. The argument may be null.

## In code

Each example is closed; open the one you need.

**Example: The transitions module registers its executor. Your own screen…**

The transitions module registers its executor. Your own screen effect would implement it like this:

```cs
public class MyEffect : ISceneTransitionExecutor
{
    public Task TransitionToHideScreen() { return Task.CompletedTask; }

    public Task TransitionToShowScreen(Action _screenCountsAsVisible)
    {
        _screenCountsAsVisible?.Invoke();
        return Task.CompletedTask;
    }
}

SceneActions.RegisterTransitionExecutor(new MyEffect());
```

<!-- members AbstractPixel.SceneManagement.ISceneTransitionExecutor -->

## See also

- [Screen and transitions](../guide/screen-and-transitions.md)
- [Find a problem](../reference/find-a-problem.md)
