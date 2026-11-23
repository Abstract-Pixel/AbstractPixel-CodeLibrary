# Screen and transitions <!-- level 1 -->

A screen effect covers the screen while scenes change. The scene system asks it to [hide the screen](term:hide-screen) and [show the screen](term:show-screen) through one [scene transition executor](term:scene-transition-executor).

## Use the basic fade <!-- level 0 -->

Put a screen effect, such as the basic fade, in a [manager scene](term:manager-scene). Setup puts one in the starter Managers scene.

**Try it in the sample**

<!-- action open-sample 01 "Open sample 01" -->
<!-- action select "Packages/com.abstractpixel.library/Modules/SceneTransitionSystem/Runtime/Transitions/BasicFade/ImageFadeProfile.asset" "Select the fade profile" -->

1. Press Play. <!-- done-when OnGameStarted -->
2. Click "Go to Room B". <!-- done-when screen:Hiding -->
3. Watch the screen cover. <!-- done-when screen:Hidden -->
4. Watch Room B appear. <!-- done-when screen:Visible -->

> [!TIP]
> The screen fades to black and back, with Room B behind it.

## Screen states <!-- level 1 -->

Read `TransitionActions.ScreenState`, or listen to `TransitionEventBus.OnScreenStateChanged`. The [screen state](term:screen-state) goes Visible, Hiding, Hidden, Showing, Visible.

| State | What the screen is doing |
|---|---|
| Visible | Nothing covers it |
| Hiding | Hide screen runs |
| Hidden | Covered; scenes close and open |
| Showing | Show screen runs |

<details><summary>Why it works</summary>

Neither module touches `Time.timeScale`: game management owns game time and reacts to the screen state ([design decision 10](../../../../docs/adr/0010-game-management-owns-time-and-transitions-report-screen-states.md)).

</details>

## Screen counts as visible <!-- level 2 -->

Set [Screen counts as visible at](term:screen-counts-as-visible-at) on the transition profile: the share of Show screen at which [relocatables](term:relocatable) are released. It is 0.75 by default.

## No screen effect <!-- level 2 -->

With no effect registered, scenes change as an instant cut. The loading scene still opens first, so no frame has no camera.

<details><summary>If it goes wrong</summary>

- No fade → no manager scene holds a screen effect.
- The game pauses after a change → your game code changed `Time.timeScale`; react to the screen state instead.

</details>
