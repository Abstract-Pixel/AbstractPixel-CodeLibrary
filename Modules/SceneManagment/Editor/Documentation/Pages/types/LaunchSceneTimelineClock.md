# LaunchSceneTimelineClock

Caps how far a launch scene's Timeline may advance in one frame, so a load hitch (the start scene group opening behind it) delays the picture instead of skipping it. The director keeps its own playback, so markers fire and audio clips play normally; on a long frame the clock only slows the root playable for that frame (speed = cap / frame time).

**Kind:** component · **Namespace:** `AbstractPixel.SceneManagement.SceneScreens` · **File:** `LaunchSceneTimelineClock.cs`

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/SceneScreens/Runtime/LaunchScene/LaunchSceneTimelineClock.cs" "Select the script" -->

Read more in [Start up](../guide/start-up.md#launch-scene).

Call Tick from Update, which runs before the director's own update in the same frame.

## Members

<!-- members AbstractPixel.SceneManagement.SceneScreens.LaunchSceneTimelineClock -->

## See also

- [Start up](../guide/start-up.md)
- [Find a problem](../reference/find-a-problem.md)
