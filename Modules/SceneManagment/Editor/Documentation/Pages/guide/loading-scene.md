# Loading scene <!-- level 1 -->

<!-- layout new -->

A [loading scene](term:loading-scene) shows while the old place closes and the new one opens. Each [scene group](term:scene-group) says when it shows one.

## Show a loading scene <!-- level 0 -->

Set [Show the loading scene](term:show-the-loading-scene) on the scene group you go to.

| Show the loading scene | When the loading scene shows |
|---|---|
| Never | Never |
| When arriving from another scene group | On a change from another scene group, not on a Retry |
| Every time | On every change, Retry included |

> [!TIP]
> Hide screen, the loading scene with its bar, then the new place.

> [!NOTE]
> Start-up and a switch to a preloaded group never show a loading scene.

**Try it in the sample**

<!-- action open-sample 06 "Open sample 06" -->

1. Press Play. <!-- done-when OnGameStarted -->
2. Go to the destination. <!-- done-when OnNewSceneGroupTransitionTo -->

**Or do it yourself**

1. List the loading scene in Project Settings > Abstract Pixel > Scene System > Loading scenes.
2. On the scene group, set Show the loading scene and pick the scene.

<!-- action open-settings "Loading scenes" "Open Loading scenes" -->

- **Loading scenes:** the list each scene group picks from.
- **Default loading scene:** named in the same section.
- **Minimum time:** set in the same section, see [Minimum time](#minimum-time).

## Minimum time <!-- level 0 -->

Keeps a fast load from flashing the loading scene. One value for the whole project, set in Project Settings, not on a scene group.

<!-- action open-settings "Loading scenes" "Open Loading scenes" -->

- **Field:** Minimum time on screen (s), in Project Settings › Abstract Pixel › Scene System › Loading scenes.
- **Applies to:** every loading scene in the project.
- **Counts:** unscaled seconds.
- **Starts:** when Show screen starts.

## Show the progress <!-- level 1 -->

Add a `LoadingProgressView` to the loading scene when you want to change how your progress bar looks and feels. It smooths the [loading progress](term:loading-progress) and fills your bar with it.

**Two ways to fill your bar**

| | Without the view | With the view |
|---|---|---|
| What you do | Your own script reads `SceneActions.LoadingProgress` every frame | Add a `LoadingProgressView` and connect your bar to it |
| What the player sees | The bar jumps as each scene opens: 0.33, 0.66, 1 | The bar glides at the speed you set and can hold at full |
| Good for | A quick test, or a bar that should show the raw value | Anything a player sees |

Without the view, in your own script:

```cs
private void Update()
{
    bar.fillAmount = SceneActions.LoadingProgress;
}
```

With the view, connect your bar in one of two ways:

| Your bar | Do this |
|---|---|
| Already has a function that sets its fill | No code. On the view's On Progress list, click +, drag in the object that has your bar, then pick that function in the dropdown. It works like a Button's On Click; the view calls the function every frame with the value from 0 to 1. |
| Is your own script | Implement `ILoadingProgressDisplay` in a script on the same object as the view. Sample 06 does this: `Sample06BarDisplay` sits next to the view on Loading Bar, and its event lists are empty. |

<!-- action select-in-scene "Packages/com.abstractpixel.library/Modules/SceneManagment/Samples/06 Loading Scene/Scenes/Sample 06 Loading.unity" "Loading Bar" "Select Loading Bar in sample 06" -->

Sample 06's script:

```cs source=lib:Modules/SceneManagment/Samples/06 Loading Scene/Scripts/Sample06BarDisplay.cs
public void SetProgress(float _progress)
{
    ShownProgress = _progress;
```

**LoadingProgressView fields, in the Inspector**

What you see when you select the object with the `LoadingProgressView` component in your loading scene.

| Field | What it does for you | Change it when |
|---|---|---|
| Display | Optional. A script of yours that implements `ILoadingProgressDisplay`. Empty uses one on the same object. | Your bar's script sits on another object. |
| **Feel: a foldout on this component that groups how the bar moves. Not the Feel package.** | | |
| Smoothing Speed | How fast the bar catches up with the real progress, per second. 1 fills an empty bar in one second. Default 1.5. | Raise it if the bar lags behind a fast load. Lower it for a calmer fill. |
| Curve | Optional. Bends the fill, for example fast at first and slow near the end. Empty fills evenly. | You want the bar to feel like it speeds up or slows down. |
| Hold At Full Seconds | How long the full bar stays on screen before the screen covers it. Default 0.3. | The player should see the bar reach 100%. |

**LoadingProgressView events, in the Inspector**

The four lists at the bottom of the same component. Each one calls the functions you drag into it.

| Event | When it fires | You'll use it to |
|---|---|---|
| On Progress (float) | Every frame while the loading scene shows, with the smoothed value from 0 to 1 | Fill the bar. Almost every setup uses this one. |
| On Full | Every scene is open and the bar has reached 1 | Play a finishing beat: a flash, a sound, a "ready" label. The screen waits for it. |
| On Shown | Show screen starts on the loading scene | Play the bar's appear animation. |
| On Hidden | The loading scene is closing | Stop sounds or reset effects. |

<details><summary>More: the progress value, in code</summary>

`SceneActions.LoadingProgress` is the raw value the scene system keeps. It is not a field on any component and you never set it. The view reads it; your own script can read it too.

- **Range:** 0 to 1. Every scene the change opens has an equal share.
- **Direction:** never goes back, and reaches 1 only once every scene is open.
- **Reading it:** safe every frame. Not smoothed; the view smooths it.

</details>

<details><summary>More: how the view fits in</summary>

- **Registers itself:** with `SceneActions.RegisterLoadingProgressView` when it is enabled. You never call it.
- **Drives:** an `ILoadingProgressDisplay` found on its object or assigned: Show, SetProgress, PlayFull, Hide.
- **Timing:** the scene system calls it when Show screen starts, and waits for its full beat and hold before Hide screen.
- **Example:** Rally's `RallyLoadingFullBeat` plays `IAnimationCommand`s.
- **Rally's bar, with no code:** `MMProgressBar.UpdateBar01` in On Progress and `MMF_Player.PlayFeedbacks` in On Full, both from the Feel package by More Mountains.

</details>

## The order of a change <!-- level 3 -->

The loading scene opens before the old level closes, so a frame never has no camera.

1. **Cover the old level**
   1. Hide screen
   2. The loading scene opens
   3. Show screen
2. **Swap the levels behind the loading scene**
   1. The old scenes close
   2. Unused memory is freed
   3. The new scenes open while the progress fills
   4. The loading progress view plays its "full" animation
3. **Uncover the new level**
   1. Hide screen
   2. The loading scene closes
   3. Show screen on the new level

> [!NOTE]
> With no screen effect, the same steps make an instant cut.

<details><summary>Why it works</summary>

- **No frame without a camera:** the loading scene opens before the old place closes ([design decision 3](../../../../docs/adr/0003-loading-scene-opens-before-the-old-level-closes.md)).
- **Your own copy:** setup clones the ready-made loading scene into your project ([design decision 4](../../../../docs/adr/0004-ready-made-assets-are-cloned-at-setup.md)).

</details>

## Parts <!-- level 3 -->

Everything for loading scenes lives in SceneScreens, and setup gives you your own copy of the ready-made scene.

<details><summary>More: the parts</summary>

- **SceneScreens:** `Modules/SceneManagment/SceneScreens`, its own assembly. The scene module never references it.
- **Ready-made scene:** `SceneScreens/ReadyMade/Loading Scene.unity`: an opaque background in the ready-made fade's colour, a plain bar, no art.
- **At setup:** setup clones it and lists the copy as the default. It also clones the ready-made fade with its controller prefab, so nothing the project uses points into the library ([design decision 4](../../../../docs/adr/0004-ready-made-assets-are-cloned-at-setup.md)).
- **Draw order:** the effect's canvas draws above everything (Screen Space Overlay, highest sort order). The loading scene's canvas is opaque and full screen.
- **Launch scene helpers:** `LaunchSceneReleaseMarker` and `LaunchSceneReleaseReceiver` (a Timeline marker that releases the launch scene hold once start-up has finished), `LaunchSceneRenderSettingsGuard`, `LaunchSceneTimelineClock` and `LaunchSceneStartUpLog`.

</details>

## Troubleshooting <!-- level 1 -->

<details><summary>If it goes wrong</summary>

| Symptom | Cause | Fix |
|---|---|---|
| No loading scene appears | The group's Show the loading scene is Never, or this is a Retry with "When arriving from another scene group" | Pick Every time to cover Retry too |
| The bar jumps to full | That is right for a fast load | Raise the minimum time to see it |
| The loading scene stays up | Its `ILoadingProgressDisplay.PlayFull` never finishes | Make the PlayFull task end |

</details>
