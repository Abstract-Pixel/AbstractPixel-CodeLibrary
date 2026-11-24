# LaunchSceneTimelineClock

<!-- layout new -->

Caps how far a launch scene's Timeline may advance in one frame, so a load hitch delays the picture instead of skipping it.

<!-- type-header AbstractPixel.SceneManagement.SceneScreens.LaunchSceneTimelineClock -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/SceneScreens/Runtime/LaunchScene/LaunchSceneTimelineClock.cs" "Select the script" -->

Read more in [Start up](../guide/start-up.md#launch-scene).

## Start here

The load hitch is the start scene group opening behind the launch scene. The director keeps its own playback, so markers fire and audio clips play normally; on a long frame the clock only slows the root playable for that frame.

| You want to | Use | Why |
|---|---|---|
| Set the cap | The constructor | Pass the PlayableDirector and the longest step in seconds. |
| Cap this frame | `Tick` | Call it from Update, which runs before the director's own update in the same frame. |
| Hold the playhead for a frame | `Freeze` | For a game's own hold, such as waiting for saved volumes. |

- **Speed on a long frame:** cap / frame time.
- **Frame time:** the same delta the director uses (unscaled when its update mode is Unscaled Game Time).
- **Needs:** the Timeline package (`com.unity.timeline`).

## In code

From sample 10:

```cs source=../../../../Samples/10 Launch Scene/Scripts/Sample10Splash.cs
clock = new LaunchSceneTimelineClock(timeline, MaximumStepSeconds);
```

```cs source=../../../../Samples/10 Launch Scene/Scripts/Sample10Splash.cs
clock.Tick();
```

<!-- members AbstractPixel.SceneManagement.SceneScreens.LaunchSceneTimelineClock -->

## See also

- [Start up](../guide/start-up.md)
- [Find a problem](../reference/find-a-problem.md)
