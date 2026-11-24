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

> [!TIP]
> Hide screen, the loading scene with its bar, then the new place.

> [!NOTE]
> Start-up and a switch to a preloaded group never show a loading scene.

## Minimum time <!-- level 0 -->

Keeps a fast load from flashing the loading scene.

- **Counts:** unscaled seconds.
- **Starts:** when Show screen starts.

## Show the progress <!-- level 1 -->

Add a `LoadingProgressView` to the loading scene. It reads the [loading progress](term:loading-progress), smooths it and drives your bar.

| Your bar | Do this |
|---|---|
| No code | Drag `MMProgressBar.UpdateBar01` into On Progress and `MMF_Player.PlayFeedbacks` into On Full. |
| Code | Implement `ILoadingProgressDisplay`, as the sample does: |

```cs source=lib:Modules/SceneManagment/Samples/06 Loading Scene/Scripts/Sample06BarDisplay.cs
public void SetProgress(float _progress)
{
    ShownProgress = _progress;
```

<details><summary>More: what the view does</summary>

- **Reads:** `SceneActions.LoadingProgress`.
- **Smooths it:** speed, an optional curve, and a hold at full.
- **Raises:** On Shown, On Progress (float), On Full and On Hidden.
- **Drives:** an `ILoadingProgressDisplay` found on its object or assigned: Show, SetProgress, PlayFull, Hide.
- **Registers itself:** with `SceneActions.RegisterLoadingProgressView`.
- **Timing:** the scene system calls it when Show screen starts, and waits for its full beat and hold before Hide screen.
- **Example:** Rally's `RallyLoadingFullBeat` plays `IAnimationCommand`s.

</details>

<details><summary>More: the progress value</summary>

- **Range:** 0 to 1. Every scene the change opens has an equal share.
- **Direction:** never goes back, and reaches 1 only once every scene is open.
- **Reading it:** safe every frame. Not smoothed; the view smooths it.

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
