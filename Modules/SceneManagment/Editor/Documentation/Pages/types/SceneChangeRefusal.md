# SceneChangeRefusal

<!-- layout new -->

An enum: why the scene system refused a scene change request. You get it from `SceneChangeResult.Refusal`, which is `None` unless `SceneChangeResult.Outcome` is `Refused`.

<!-- type-header AbstractPixel.SceneManagement.SceneChangeRefusal -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/PublicAPI/SceneChangeRefusal.cs" "Select the script" -->

Read more in [Change scene](../guide/change-scene.md#read-the-result).

## Start here

A refusal happens before anything touches the screen, so the player sees nothing. Check `Outcome` first, then read `Refusal` for the reason.

| Value | Means | Do this |
|---|---|---|
| `None` | The request was not refused. | Nothing: the outcome is Done or Failed. |
| `Busy` | Another scene change is running. Only one runs at a time. | Ask again after the running change. |
| `StartUpRunning` | Start-up is still opening the first scene group. | Wait for `SceneEventBus.OnGameStarted`. |
| `AlreadyOpen` | The group asked for is already open. | Call `SceneActions.Retry` to start it again. |
| `NoGroupGiven` | No scene group was given. | Pass a scene group. |
| `NothingOpenToRetry` | Retry was asked for while no scene group is open. | Call `SceneActions.ChangeTo` instead. |

## In code

React to the reason when the outcome is Refused:

```cs
SceneChangeResult result = await SceneActions.ChangeTo(nextGroup);
if (result.Outcome == SceneChangeOutcome.Refused)
{
    switch (result.Refusal)
    {
        case SceneChangeRefusal.Busy:
            // A change is already running: let it finish.
            break;
        case SceneChangeRefusal.AlreadyOpen:
            await SceneActions.Retry();
            break;
        default:
            Debug.Log("Refused: " + result.Refusal);
            break;
    }
}
```

<!-- members AbstractPixel.SceneManagement.SceneChangeRefusal -->
