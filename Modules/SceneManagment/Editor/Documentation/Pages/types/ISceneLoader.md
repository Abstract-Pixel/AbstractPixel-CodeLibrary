# ISceneLoader

<!-- layout new -->

The contract every scene loader follows: it opens and closes scenes for the `SceneCoordinator`. Only a loader calls Unity's scene loading API.

<!-- type-header AbstractPixel.SceneManagement.ISceneLoader -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/SceneLoaderTypes/ISceneLoader.cs" "Select the script" -->

Read more in [Build settings and addressables](../guide/build-settings-and-addressables.md#which-loader-opens-a-scene).

## Start here

You do not call a loader yourself. The coordinator does, through a `SceneLoaderSelector`. Read this page when you write a new loader or a test fake.

| You want to | Use | Why |
|---|---|---|
| Open a scene | `LoadScene` | Pass false for `sceneActivatedByDefault` to hold a main scene ready (a preload). |
| Close a scene | `UnloadScene` | A held scene is activated first, then closed. |
| Show how far a load has got | `GetLoadProgress` | 0 to 1; the loading progress reads it. |
| Free unused assets | `ReleaseUnusedMemory` | Slow, so the coordinator calls it once per scene change. |

**The held scene contract**

- **Rule 1, held:** a main scene loaded with `sceneActivatedByDefault = false` is held. Only one scene is held at a time, and only a main scene.
- **Rule 2, nothing else meanwhile:** while a scene is held, a load or unload of a different scene may never finish. The coordinator settles a preload before any other scene change.
- **Rule 3, activate:** loading the held scene again with `sceneActivatedByDefault = true` activates it. With `isMainScene` true it also becomes the active scene and raises `SceneEventBus.OnMainSceneLoaded`.
- **Rule 4, close:** unloading the held scene activates it first, because a held load cannot be cancelled, without making it the active scene, and then closes it.
- **Who follows it:** the coordinator relies on these four rules, and the fake loader in the tests follows them.

## In code

Each example is closed; open the one you need.

**Example: Every loader has these four calls; the coordinator makes them for you**

```cs
ISceneLoader loader = new SceneLoaderSelector();
await loader.LoadScene(levelScene, true);
float progress = loader.GetLoadProgress(levelScene);
await loader.UnloadScene(levelScene);
await loader.ReleaseUnusedMemory();
```

<!-- members AbstractPixel.SceneManagement.ISceneLoader -->

## See also

- [Build settings and addressables](../guide/build-settings-and-addressables.md)
- [Find a problem](../reference/find-a-problem.md)
