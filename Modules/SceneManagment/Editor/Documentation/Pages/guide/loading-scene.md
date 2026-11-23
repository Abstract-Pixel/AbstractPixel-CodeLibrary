# Loading scene <!-- level 1 -->

A [loading scene](term:loading-scene) shows while the old place closes and the new one opens. Each [scene group](term:scene-group) says when it shows one.

## Show a loading scene <!-- level 0 -->

Set [Show the loading scene](term:show-the-loading-scene) on the group you go to: Never, When arriving from another scene group, or Every time, Retry included.

**Try it in the sample**

<!-- action open-sample 06 "Open sample 06" -->

1. Press Play. <!-- done-when OnGameStarted -->
2. Go to the destination. <!-- done-when OnNewSceneGroupTransitionTo -->

**Or do it yourself**

1. List the loading scene in Project Settings > Abstract Pixel > Scene System > Loading scenes.
2. On the scene group, set Show the loading scene and pick the scene.

<!-- action open-settings "Loading scenes" "Open Loading scenes" -->

The section lists the loading scenes, names the default loading scene and sets the minimum time on screen.

> [!TIP]
> Hide screen, the loading scene with its bar, then the new place.

> [!NOTE]
> Start-up and a switch to a preloaded group never show a loading scene.

## Minimum time <!-- level 0 -->

The minimum time on screen keeps a fast load from flashing the loading scene. It counts unscaled seconds from Show screen starting.

## Show the progress <!-- level 1 -->

Add a `LoadingProgressView` to the loading scene. It reads [loading progress](term:loading-progress), smooths it and raises On Shown, On Progress, On Full and On Hidden; it also drives an `ILoadingProgressDisplay` on its object.

```cs source=lib:Modules/SceneManagment/Samples/06 Loading Scene/Scripts/Sample06BarDisplay.cs
public void SetProgress(float _progress)
{
    ShownProgress = _progress;
```

`SceneActions.LoadingProgress` goes from 0 to 1, never goes back, and reaches 1 only once every scene is open.

<details><summary>Why it works</summary>

The loading scene opens before the old place closes, so no frame is without a camera ([design decision 3](../../../../docs/adr/0003-loading-scene-opens-before-the-old-level-closes.md)). Setup clones the ready-made loading scene into your project ([design decision 4](../../../../docs/adr/0004-ready-made-assets-are-cloned-at-setup.md)).

</details>

<details><summary>If it goes wrong</summary>

- No loading scene appears → the group's Show the loading scene is Never, or this is a Retry with "When arriving from another scene group".
- The bar jumps to full → that is right for a fast load; raise the minimum time to see it.
- The loading scene stays up → its `ILoadingProgressDisplay.PlayFull` never finishes.

</details>

## Details <!-- level 3 -->

- **With a screen effect:** Hide screen, the loading scene opens, Show screen, the old scenes close, memory is freed, the
  new scenes open while progress fills, the loading progress view plays its "full" animation, Hide screen, the loading
  scene closes, Show screen on the new level.
- **Without one:** the same steps make an instant cut; the loading scene opens before the old level closes, so no frame
  has no camera.
- **Loading progress:** `SceneActions.LoadingProgress`, 0 to 1. Every scene the change opens has an equal share, the value
  never goes back and is 1 only once every scene is open. Safe to read every frame. Not smoothed.
- **SceneScreens** (`Modules/SceneManagment/SceneScreens`, its own assembly; the scene module never references it):
  `LoadingProgressView` reads the progress, smooths it (feel: speed, optional curve, hold at full) and raises On Shown,
  On Progress (float), On Full and On Hidden. It also drives an `ILoadingProgressDisplay` (Show, SetProgress, PlayFull,
  Hide) found on its object or assigned. It registers itself with `SceneActions.RegisterLoadingProgressView`; the
  scene system calls it when Show screen starts and waits for its full beat and hold before Hide screen.
- **Feel with no code:** drag `MMProgressBar.UpdateBar01` into On Progress and an `MMF_Player.PlayFeedbacks` into On Full.
  **Code animations:** implement `ILoadingProgressDisplay` (Rally's `RallyLoadingFullBeat` plays `IAnimationCommand`s).
- **Ready-made:** `SceneScreens/ReadyMade/Loading Scene.unity` (opaque background in the ready-made fade's colour, a plain
  bar, no art). The setup clones it, lists the copy as the default, and clones the ready-made fade with its controller
  prefab, so nothing the project uses points into the library (ADR 0004). Visual rules: the effect's canvas draws above
  everything (Screen Space Overlay, highest sort order); the loading scene's canvas is opaque and full screen.
- **Launch scene helpers** also live in SceneScreens: `LaunchSceneReleaseMarker` + `LaunchSceneReleaseReceiver` (a
  Timeline marker that releases the launch scene hold once start-up has finished), `LaunchSceneRenderSettingsGuard`,
  `LaunchSceneTimelineClock` and `LaunchSceneStartUpLog`.
