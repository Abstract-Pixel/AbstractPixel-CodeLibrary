# LaunchSceneReleaseReceiver

Put it next to the launch scene's PlayableDirector. When the Timeline reaches a LaunchSceneReleaseMarker it waits until start-up has finished (or failed) and one more frame, then releases the launch scene hold with SceneActions.ReleaseLaunchScene and raises On Released, where the launch scene turns its own camera and listener off and closes itself.

**Kind:** component · **Namespace:** `AbstractPixel.SceneManagement.SceneScreens` · **File:** `LaunchSceneReleaseReceiver.cs`

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/SceneScreens/Runtime/LaunchScene/LaunchSceneReleaseReceiver.cs" "Select the script" -->

Read more in [Start up](../guide/start-up.md#launch-scene).

It also releases when it is destroyed, so leaving Play never leaves the start scene group held.

## In the Inspector

| Label | Member | What it does |
|---|---|---|
| On released | `onReleased` | Raised once, right after the launch scene hold is released. |

## Members

<!-- members AbstractPixel.SceneManagement.SceneScreens.LaunchSceneReleaseReceiver -->

## See also

- [Start up](../guide/start-up.md)
- [Find a problem](../reference/find-a-problem.md)
