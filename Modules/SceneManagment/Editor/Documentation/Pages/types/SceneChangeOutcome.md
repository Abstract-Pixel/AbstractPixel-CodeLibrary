# SceneChangeOutcome

<!-- layout new -->

How a scene change request ended: Done, Refused or Failed.

<!-- type-header AbstractPixel.SceneManagement.SceneChangeOutcome -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/PublicAPI/SceneChangeOutcome.cs" "Select the script" -->

Read more in [Change scene](../guide/change-scene.md#read-the-result).

## Start here

Read it from `SceneChangeResult.Outcome`. For a plain yes or no, `SceneChangeResult.IsDone` is shorter.

| You want to | Use | Why |
|---|---|---|
| Carry on after the change | `Done` | The scene change ran to its end. |
| Tell the player to wait or try later | `Refused` | The change never started; the screen was never covered. The reason is in `SceneChangeResult.Refusal`. |
| Handle a broken scene | `Failed` | A scene failed to load; the change did not finish. The scene is in `SceneChangeResult.FailedScene`. |

<!-- members AbstractPixel.SceneManagement.SceneChangeOutcome -->

## See also

- [Change scene](../guide/change-scene.md)
- [When a scene change fails](../guide/when-a-scene-change-fails.md)
- [Find a problem](../reference/find-a-problem.md)
