# Start-up and Play from any scene <!-- level 0 -->

When the game launches or you press Play, [start-up](term:start-up) opens the right [scene group](term:scene-group) by itself. You only pick the [start scene group](term:start-scene-group).

<!-- your-project start-up -->

## Pick the start scene group <!-- level 0 -->

Pick the start scene group in Project Settings > Abstract Pixel > Scene System > Start-up.

<!-- action open-settings "Start-up" "Open Start-up" -->

The section has three fields: Start scene group, Launch scene (optional) and "If a scene change fails, go to".

> [!TIP]
> A build opens that group first; the section shows a green line when it is ready.

## Play from any scene <!-- level 0 -->

Press Play in any scene: start-up opens that scene's group, so you test the place you are editing.

**Try it in the sample**

<!-- action open-sample 04 "Open sample 04" -->

1. Press Play. <!-- done-when OnGameStarted -->

| You press Play in | Start-up opens |
|---|---|
| A group's main scene | That group |
| A manager or shared scene alone | The start scene group |
| A [scene in no group](term:scene-in-no-group) | A temporary group built from the default scene set |
| The launch scene | The start scene group, behind the launch scene |

> [!NOTE]
> Scene changes are refused with `StartUpRunning` until start-up ends.

## Launch scene <!-- level 1 -->

A [launch scene](term:launch-scene), such as a splash, plays first while the start scene group opens behind it. The [launch scene hold](term:launch-scene-hold) keeps the group from showing itself until the launch scene calls `SceneActions.ReleaseLaunchScene()`.

**Try it in the sample**

<!-- action open-sample 10 "Open sample 10" -->

**Or do it yourself**

Scripts that show something wait for the release:

```cs source=lib:Modules/SceneManagment/Samples/10 Launch Scene/Scripts/Sample10MenuIntro.cs
private void OnEnable()
{
    SceneActions.RunWhenLaunchSceneReleased(StartIntro);
}
```

The launch scene stays at build index 0: the page warns with Fix now when Build Settings were reordered.

<details><summary>Why it works</summary>

Start-up holds only when the launch scene is one of the scenes Unity opens, so Play from a level never waits ([design decision 9](../../../../docs/adr/0009-start-up-opens-the-start-scene-group-behind-a-launch-scene.md)).

</details>

<details><summary>If it goes wrong</summary>

- The menu never shows → the launch scene never calls `SceneActions.ReleaseLaunchScene()`.
- `SceneActions.HasStartUpFailed` is true → a scene of the start group failed to load; start-up has no fallback.
- The splash is skipped in a build → the launch scene is not at build index 0; press Fix now.

</details>
