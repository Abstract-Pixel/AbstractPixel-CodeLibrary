# TransitionEventBus

<!-- layout new -->

The screen effect's messages: Hide screen covers the screen, Show screen uncovers it.

<!-- type-header AbstractPixel.SceneTransitions.TransitionEventBus -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneTransitionSystem/Runtime/Core/TransitionEventBus.cs" "Select the script" -->

Read more in [Screen and transitions](../guide/screen-and-transitions.md#screen-states).

## Start here

Listen here instead of polling. Each message is a code event; subscribe in `OnEnable` and unsubscribe in `OnDisable`.

| You want to | Use | Why |
|---|---|---|
| React to every screen state | `OnScreenStateChanged` | Carries the new `ScreenState`. Raised before the matching message below. |
| Start something as the cover starts | `OnStartHideScreenTransition` | Raised when the state becomes Hiding. |
| Act while the screen is covered | `OnEndHideScreenTransition` | Raised when the state becomes Hidden. |
| Start something as the screen uncovers | `OnStartShowScreenTransition` | Raised when the state becomes Showing. |
| Act once the screen is uncovered | `OnEndShowScreenTransition` | Raised when the state becomes Visible. |

**Called for you, leave alone**

- **Listeners at start-up:** every event is cleared when Play starts, so subscribe from your component, not once from the Editor.

## In code

Each example is closed; open the one you need.

**Example: From a sample**

```cs source=../../../../Samples/02 Change Level From Code/Scripts/Sample02MessageLog.cs
TransitionEventBus.OnScreenStateChanged += OnScreenStateChanged;
```

<!-- members AbstractPixel.SceneTransitions.TransitionEventBus -->

## See also

- [Screen and transitions](../guide/screen-and-transitions.md)
- [Find a problem](../reference/find-a-problem.md)
