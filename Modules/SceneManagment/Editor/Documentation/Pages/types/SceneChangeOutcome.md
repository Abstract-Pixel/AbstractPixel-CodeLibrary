# SceneChangeOutcome

<!-- layout new -->

An enum: how a scene change request ended. You get it from `SceneChangeResult.Outcome`, on the result that `SceneActions.ChangeTo`, `Retry` and `Preload` return.

<!-- type-header AbstractPixel.SceneManagement.SceneChangeOutcome -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/PublicAPI/SceneChangeOutcome.cs" "Select the script" -->

Read more in [Change scene](../guide/change-scene.md#read-the-result).

## Start here

Three values. Each one says how the request ended; the result next to it carries the details.

| Value | Means | Details on the result |
|---|---|---|
| `Done` | The scene change ran to its end. | Nothing more. `SceneChangeResult.IsDone` is true. |
| `Refused` | The change never started; the screen was never covered. | Why, in `SceneChangeResult.Refusal` (a `SceneChangeRefusal`). |
| `Failed` | A scene failed to load; the change did not finish. | Which scene, in `SceneChangeResult.FailedScene`. |

**Outcome or IsDone?**

- **The enum:** `SceneChangeResult.Outcome` holds one of the three values above. Switch on it when you treat Refused and Failed differently.
- **The shortcut:** `SceneChangeResult.IsDone` is a bool, true when `Outcome` is `Done`. Use it when you only need yes or no.
- **Not the same Done:** `SceneChangeResult.Done()` is a method the scene system uses to make a result. You never call it; you read `Outcome` or `IsDone`.

## In code

Switch on the outcome after the await:

```cs
SceneChangeResult result = await SceneActions.ChangeTo(nextGroup);
switch (result.Outcome)
{
    case SceneChangeOutcome.Done:
        // The new scene group is open.
        break;
    case SceneChangeOutcome.Refused:
        Debug.Log("Refused: " + result.Refusal);
        break;
    case SceneChangeOutcome.Failed:
        Debug.LogWarning("Failed to load " + result.FailedScene.SceneName);
        break;
}
```

Or, for a plain yes or no:

```cs
SceneChangeResult result = await SceneActions.ChangeTo(nextGroup);
if (result.IsDone)
{
    // The new scene group is open.
}
```

<!-- members AbstractPixel.SceneManagement.SceneChangeOutcome -->
