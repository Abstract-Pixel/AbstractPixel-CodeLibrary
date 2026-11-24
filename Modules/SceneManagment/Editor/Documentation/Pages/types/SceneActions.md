# SceneActions

<!-- layout new -->

The calls a game makes to change scene: `ChangeTo`, `Retry` and `Preload`, each returning a `SceneChangeResult`, plus the open scene group, loading progress and the launch scene hold.

<!-- type-header AbstractPixel.SceneManagement.SceneActions -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/PublicAPI/SceneActions.cs" "Select the script" -->

Read more in [Change scene](../guide/change-scene.md#change-scene-from-code).

## In code

From a sample:

```cs source=../../../../Samples/02 Change Level From Code/Scripts/Sample02LevelChanger.cs
SceneChangeResult result = await SceneActions.ChangeTo(otherGroup);
```

<!-- members AbstractPixel.SceneManagement.SceneActions -->

## See also

- [Change scene](../guide/change-scene.md)
- [Find a problem](../reference/find-a-problem.md)
