# SceneCoordinator

<!-- layout new -->

Runs every scene change: start-up, a change to a scene group, Retry and preload. One runs at a time.

<!-- type-header AbstractPixel.SceneManagement.SceneCoordinator -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/Core/SceneCoordinator.cs" "Select the script" -->

Read more in [Scene messages](../guide/scene-messages.md#order-in-a-scene-change).

## Start here

You never call it. `SceneActions` hands your calls to it and returns its `SceneChangeResult`.

| You want to | Use | Why |
|---|---|---|
| Change scene | `SceneActions.ChangeTo` | Its scene work is internal. |
| Know a change is running | `SceneActions.IsLoadingSceneGroup` | True until the screen is shown again. |
| Know start-up is done | `SceneActions.HasGameStarted` | True once the first scene group is open and `OnGameStarted` was raised. |

**Called for you, leave alone**

- **Plan:** what to open and close comes from `SceneChangePlan`.
- **Record:** what is open is kept in one record.
- **Screen:** an accepted change hides the screen through the registered scene transition executor and shows it again at the end.
- **Refused:** a refused change never touches the screen.

## In code

Each example is closed; open the one you need.

**Example: You never call it. Every call on SceneActions goes to it**

You never call it. Every call on `SceneActions` goes to it:

```cs
// Not SceneCoordinator: use SceneActions.
SceneChangeResult result = await SceneActions.ChangeTo(nextGroup);
bool busy = SceneActions.IsLoadingSceneGroup;
```

<!-- members AbstractPixel.SceneManagement.SceneCoordinator -->

## See also

- [Scene messages](../guide/scene-messages.md)
- [Find a problem](../reference/find-a-problem.md)
