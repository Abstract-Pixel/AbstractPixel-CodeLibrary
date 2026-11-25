# Start-up and Play from any scene <!-- level 0 -->

<!-- layout new -->

When the game launches or you press Play, [start-up](term:start-up) opens the right [scene group](term:scene-group) by itself. You only pick the [start scene group](term:start-scene-group).

<!-- your-project start-up -->

## Pick the start scene group <!-- level 0 -->

Pick it in Project Settings > Abstract Pixel > Scene System > Start-up.

<!-- action open-settings "Start-up" "Open Start-up" -->

- **Start scene group:** the group a build opens first.
- **Launch scene:** optional, such as a splash. See [Launch scene](#launch-scene).
- **If a scene change fails, go to:** the [fallback scene group](term:fallback-scene-group). See [When a scene change fails](when-a-scene-change-fails.md).

> [!TIP]
> The section shows a green line when it is ready.

## Play from any scene <!-- level 0 -->

Press Play in any scene and start-up opens that scene's group, so you test the place you are editing.

**Try it in the sample**

<!-- action open-sample 04 "Open sample 04" -->

1. Press Play. <!-- done-when OnGameStarted -->

**What start-up opens**

| You press Play in | Start-up opens |
|---|---|
| A group's main scene | That group |
| A manager or shared scene alone | The start scene group |
| A [scene in no group](term:scene-in-no-group) | A temporary group built from the default scene set |
| The launch scene | The start scene group, behind the launch scene |

> [!NOTE]
> Scene changes are refused with `StartUpRunning` until start-up ends.

## Launch scene <!-- level 1 -->

A [launch scene](term:launch-scene), such as a splash, plays first while the start scene group opens behind it. The [launch scene hold](term:launch-scene-hold) keeps the group hidden until the launch scene calls `SceneActions.ReleaseLaunchScene()`.

**Try it in the sample**

<!-- action open-sample 10 "Open sample 10" -->

**Or do it yourself**

Scripts that show something wait for the release:

```cs source=lib:Modules/SceneManagement/Samples/10 Launch Scene/Scripts/Sample10MenuIntro.cs
private void OnEnable()
{
    SceneActions.RunWhenLaunchSceneReleased(StartIntro);
}
```

**Build index 0**

The launch scene must stay at build index 0, the scene a build opens first. Choosing it in Start-up moves it there. If Build Settings were reordered, the Start-up section warns and offers Fix now; the next build also puts it back.

<details><summary>Why it works</summary>

- **Play from a level never waits:** start-up holds only when the launch scene is one of the scenes Unity opens ([design decision 9](../../../../docs/adr/0009-start-up-opens-the-start-scene-group-behind-a-launch-scene.md)).

</details>

<details><summary>If it goes wrong</summary>

- **The menu never shows:** the launch scene never calls `SceneActions.ReleaseLaunchScene()`.
- **`SceneActions.HasStartUpFailed` is true:** a scene of the start group failed to load. Start-up has no fallback.
- **The splash is skipped in a build:** the launch scene is not at build index 0. Press Fix now.

</details>
