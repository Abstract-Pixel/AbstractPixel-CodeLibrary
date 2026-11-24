# SceneChangeResult

<!-- layout new -->

A class: what a scene change request tells its caller when it ends. Its `Outcome` (a `SceneChangeOutcome` enum) is Done, Refused or Failed; `Refusal` and `FailedScene` carry the details.

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

- **`Done()`, `Refused()` and `Failed()`:** static methods the scene system uses to make a result. They are not the outcome values; to check how a change ended, read `Outcome` or `IsDone`.

## In code

Each example is closed; open the one you need.

**Example: From a sample**

```cs source=../../../../Samples/02 Change Level From Code/Scripts/Sample02LevelChanger.cs
SceneChangeResult result = await SceneActions.ChangeTo(otherGroup);
ShowResults("Change to " + otherGroupName + ": " + result);
```

<!-- members AbstractPixel.SceneManagement.SceneChangeResult -->

## See also

- [Change scene](../guide/change-scene.md)
- [Find a problem](../reference/find-a-problem.md)
