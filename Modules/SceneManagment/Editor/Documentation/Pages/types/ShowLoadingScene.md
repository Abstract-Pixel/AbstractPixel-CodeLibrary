# ShowLoadingScene

<!-- layout new -->

When a scene group shows its loading scene ([Show the loading scene](term:show-the-loading-scene)).

<!-- type-header AbstractPixel.SceneManagement.ShowLoadingScene -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/Configuration/ShowLoadingScene.cs" "Select the script" -->

Read more in [Loading scene](../guide/loading-scene.md#show-a-loading-scene).

## Start here

Set it in the scene group's Show the loading scene field. In code, read or set `SceneGroup.ShowLoadingScene`.

| You want to | Use | Why |
|---|---|---|
| Go straight in | `Never` | The default. No loading scene. |
| Show it on arrival only | `WhenArrivingFromAnotherSceneGroup` | A Retry goes straight back in without it. |
| Show it every time | `EveryTimeRetryIncluded` | Shows on a Retry too. |

<!-- members AbstractPixel.SceneManagement.ShowLoadingScene -->

## See also

- [Loading scene](../guide/loading-scene.md)
- [Find a problem](../reference/find-a-problem.md)
