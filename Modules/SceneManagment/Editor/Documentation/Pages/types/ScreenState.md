# ScreenState

<!-- layout new -->

What the screen effect is doing to the screen. Game management reacts to it; the effect never changes game time.

<!-- type-header AbstractPixel.SceneTransitions.ScreenState -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneTransitionSystem/Runtime/Core/ScreenState.cs" "Select the script" -->

Read more in [Screen and transitions](../guide/screen-and-transitions.md#screen-states).

## Start here

Read it from `TransitionActions.ScreenState`, or get it from `TransitionEventBus.OnScreenStateChanged`. It goes Visible, Hiding, Hidden, Showing, then Visible again.

| You want to | Use | Why |
|---|---|---|
| Let the player act | `Visible` | Nothing covers the screen. Also the value when Play starts. |
| Stop input as the cover starts | `Hiding` | Hide screen runs. |
| Do work nobody should see | `Hidden` | Covered; scenes close and open. |
| Get ready to give control back | `Showing` | Show screen runs. |

<!-- members AbstractPixel.SceneTransitions.ScreenState -->

## See also

- [Screen and transitions](../guide/screen-and-transitions.md)
- [Find a problem](../reference/find-a-problem.md)
