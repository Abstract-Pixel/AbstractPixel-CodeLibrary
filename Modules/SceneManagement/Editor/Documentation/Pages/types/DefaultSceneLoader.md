# DefaultSceneLoader

<!-- layout new -->

Opens and closes scenes that are in Build Settings, or known only by name, with Unity's SceneManager.

<!-- type-header AbstractPixel.SceneManagement.DefaultSceneLoader -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagement/Runtime/SceneLoaderTypes/DefaultSceneLoader.cs" "Select the script" -->

Read more in [Build settings and addressables](../guide/build-settings-and-addressables.md#which-loader-opens-a-scene).

## Start here

You do not create this loader. The `SceneLoaderSelector` sends it every scene that is not Addressable.

| You want to | Use | Why |
|---|---|---|
| Open a scene that is in Build Settings | Nothing; the selector picks this loader | It reads the scene asset, so you only choose where new scenes go. |
| Know why a scene change failed | The exception from `LoadScene` | A scene missing from the build fails the load, so the change fails instead of looking finished. |

**What it does for you**

- **Missing scene:** checked before loading, so Unity does not log an error of its own for the same scene.
- **Sample scenes:** a scene of a sample scene group is never in Build Settings. In the Editor's Play mode it opens from its path with `EditorSceneManager.LoadSceneAsyncInPlayMode`.
- **Progress:** Unity stops a load at 0.9 until the scene activates, so the loader reports progress / 0.9.
- **Freeing memory:** one `Resources.UnloadUnusedAssets`.

## In code

Each example is closed; open the one you need.

**Example: You never call it yourself. It is the loader SceneLoaderSelector…**

You never call it yourself. It is the loader `SceneLoaderSelector` uses for every scene in Build Settings:

```cs
ISceneLoader loader = new DefaultSceneLoader();
await loader.LoadScene(levelScene, true);
float progress = loader.GetLoadProgress(levelScene);
```

<!-- members AbstractPixel.SceneManagement.DefaultSceneLoader -->
