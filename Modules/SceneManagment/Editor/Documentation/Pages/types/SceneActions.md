# SceneActions

<!-- layout new -->

The calls a game makes to change scene: `ChangeTo`, `Retry` and `Preload`, each returning a `SceneChangeResult`, plus the open scene group, loading progress and the launch scene hold.

<!-- type-header AbstractPixel.SceneManagement.SceneActions -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/PublicAPI/SceneActions.cs" "Select the script" -->

Read more in [Change scene](../guide/change-scene.md#change-scene-from-code).

## Start here

Most games only call `ChangeTo`. The rest are for a few special cases, and some are called by the library's own parts, never by you.

| You want to | Use | Why |
|---|---|---|
| Go to another place | `ChangeTo` | Covers the screen, swaps the scenes and uncovers it. Check the `SceneChangeResult` it returns. |
| Start the same place again | `Retry` | A restart button or a respawn after losing. |
| Make the next change fast | `Preload` | Opens the next group in the background; the `ChangeTo` of that group then skips the wait. |
| Show your own progress text | `LoadingProgress` | Read it every frame. For a bar, a `LoadingProgressView` smooths it for you. |
| Block input during a change | `IsLoadingSceneGroup` | True from the start to the end of a change. |
| Hand off from a launch scene | `ReleaseLaunchScene` | Only in a launch scene: ends the hold so the first scene group opens. |

**Called for you, leave alone**

- **Register… / Unregister…:** the loading progress view, the transitions module and the relocation sub-module call these themselves.

## In code

Each example is closed; open the one you need.

**Example: From a sample**

```cs source=../../../../Samples/02 Change Level From Code/Scripts/Sample02LevelChanger.cs
SceneChangeResult result = await SceneActions.ChangeTo(otherGroup);
```

<!-- members AbstractPixel.SceneManagement.SceneActions -->

## See also

- [Change scene](../guide/change-scene.md)
- [Find a problem](../reference/find-a-problem.md)
