# SceneChangeResult

<!-- layout new -->

What a scene change request tells its caller when it ends: Done, Refused (with the reason) or Failed (with the scene that failed to load).

<!-- type-header AbstractPixel.SceneManagement.SceneChangeResult -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/PublicAPI/SceneChangeResult.cs" "Select the script" -->

Read more in [Change scene](../guide/change-scene.md#read-the-result).

## Start here

`ChangeTo`, `Retry` and `Preload` return one. Read it after the await.

| You want to | Use | Why |
|---|---|---|
| Know the change worked | `IsDone` | True when the outcome is Done. |
| Tell refused from failed | `Outcome` | A `SceneChangeOutcome`: Done, Refused or Failed. |
| Know why it was refused | `Refusal` | A `SceneChangeRefusal`. None unless the outcome is Refused. |
| Know which scene failed | `FailedScene` | Null unless the outcome is Failed. |
| Show it on a label or in a log | `ToString` | "Done", "Refused (Busy)" or "Failed (scene name)". |

**Called for you, leave alone**

- **Done / Refused / Failed:** the scene system makes the result; you only read it.

## In code

From a sample:

```cs source=../../../../Samples/02 Change Level From Code/Scripts/Sample02LevelChanger.cs
SceneChangeResult result = await SceneActions.ChangeTo(otherGroup);
ShowResults("Change to " + otherGroupName + ": " + result);
```

<!-- members AbstractPixel.SceneManagement.SceneChangeResult -->

## See also

- [Change scene](../guide/change-scene.md)
- [Find a problem](../reference/find-a-problem.md)
