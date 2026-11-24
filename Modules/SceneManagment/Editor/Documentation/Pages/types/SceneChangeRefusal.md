# SceneChangeRefusal

<!-- layout new -->

Why the scene system refused a scene change request. A refusal happens before anything touches the screen.

<!-- type-header AbstractPixel.SceneManagement.SceneChangeRefusal -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/PublicAPI/SceneChangeRefusal.cs" "Select the script" -->

Read more in [Change scene](../guide/change-scene.md#read-the-result).

## Start here

Read it from `SceneChangeResult.Refusal`. It is None unless the outcome is Refused.

| You got | Do this | Why |
|---|---|---|
| `Busy` | Ask again after the running change | Another scene change is running. Only one runs at a time. |
| `StartUpRunning` | Wait for `SceneEventBus.OnGameStarted` | Start-up is still opening the first scene group. |
| `AlreadyOpen` | Call `Retry` | The group asked for is already open. |
| `NoGroupGiven` | Pass a scene group | No scene group was given. |
| `NothingOpenToRetry` | Call `ChangeTo` | Retry was asked for while no scene group is open. |

<!-- members AbstractPixel.SceneManagement.SceneChangeRefusal -->

## See also

- [Change scene](../guide/change-scene.md)
- [Find a problem](../reference/find-a-problem.md)
