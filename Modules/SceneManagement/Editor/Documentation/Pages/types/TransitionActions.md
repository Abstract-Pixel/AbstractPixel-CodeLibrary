# TransitionActions

<!-- layout new -->

Play Hide screen or Show screen on their own, read the screen state, and set the transition profile from code.

<!-- type-header AbstractPixel.SceneTransitions.TransitionActions -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneTransitionSystem/Runtime/PublicAPI/TransitionActions.cs" "Select the script" -->

Read more in [Screen and transitions](../guide/screen-and-transitions.md#screen-states).

## Start here

Scene changes run their screen effect inside `SceneActions.ChangeTo` and `SceneActions.Retry`; you do not call this class for that. Use it for an effect without a scene change, or to read the screen state.

| You want to | Use | Why |
|---|---|---|
| Know what the effect is doing | `ScreenState` | Read-only. Game management reacts to it. |
| Cover the screen without changing scene | `HideScreen` | Hide screen on its own. |
| Uncover the screen without changing scene | `ShowScreen` | Show screen on its own. |
| Use another screen effect | `SetTransitionProfile` | Swaps the transition profile; the new effect is used from the next change. |
| Check the effect is ready | `IsInitialized` | True once a profile is set and its controller was found. |

**Called for you, leave alone**

- **Initialize:** the `InitializeTransitions` component calls it when its scene wakes.
- **TransitionToSceneWithEffects:** a button name kept for menus that call it by name (ADR 0001); it forwards to `SceneActions.ChangeTo`.
- **ReloadActiveScenesWithEffects:** a button name kept for menus that call it by name (ADR 0001); it forwards to `SceneActions.Retry`.

## In code

Each example is closed; open the one you need.

**Example: Cover and uncover the screen without changing scene, for example…**

Cover and uncover the screen without changing scene, for example around a cutscene:

```cs
await TransitionActions.HideScreen();
// Move the camera, swap the UI...
await TransitionActions.ShowScreen();
```

<!-- members AbstractPixel.SceneTransitions.TransitionActions -->
