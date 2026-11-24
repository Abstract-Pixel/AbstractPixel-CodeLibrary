# ShowLoadingScene

<!-- layout new -->

An enum: when a scene group shows its loading scene. You read it from `SceneGroup.ShowLoadingScene`; in the Inspector it is the group's [Show the loading scene](term:show-the-loading-scene) field.

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

## In code

Each example is closed; open the one you need.

**Example: Read a group's choice from code**

```cs
if (sceneGroup.ShowLoadingScene == ShowLoadingScene.Never)
{
    // The change into this group goes straight in.
}
```

<!-- members AbstractPixel.SceneManagement.ShowLoadingScene -->
