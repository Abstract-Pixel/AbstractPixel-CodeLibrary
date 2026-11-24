# SceneLoaderSelector

<!-- layout new -->

The loader the coordinator uses: each scene picks its own loader from the asset's state.

<!-- type-header AbstractPixel.SceneManagement.SceneLoaderSelector -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/SceneLoaderTypes/SceneLoaderSelector.cs" "Select the script" -->

Read more in [Build settings and addressables](../guide/build-settings-and-addressables.md#which-loader-opens-a-scene).

## Start here

The scene system creates one for you. You choose only where a scene lives.

| You want to | Use | Why |
|---|---|---|
| Open a scene from Addressables | Make the scene Addressable | It opens through the `AddressablesSceneLoader`. |
| Open any other scene | Put it in Build Settings | It opens through the `DefaultSceneLoader`. |

**What it does for you**

- **Same loader to close:** a scene is closed by the loader that opened it, so a held scene and its handle never change hands.
- **Remembers by GUID and by name:** a close that only knows the name (the active scene) still finds its loader.
- **In build first:** a scene in Build Settings is answered without the Addressables check, which waits for Addressables to start. Start-up can then ask for its scenes before any Start runs.
- **Progress:** the larger of the two loaders' values; a scene is 0 to the loader that does not know it.
- **Last scene:** keeps one scene open while a change closes every scene (a group with no manager scene).
- **Freeing memory:** goes through the Build Settings loader.

## In code

Each example is closed; open the one you need.

**Example: The coordinator uses one; you never need to. It picks the loader…**

The coordinator uses one; you never need to. It picks the loader per scene:

```cs
ISceneLoader loader = new SceneLoaderSelector();
await loader.LoadScene(levelScene, true);
```

<!-- members AbstractPixel.SceneManagement.SceneLoaderSelector -->

## See also

- [Build settings and addressables](../guide/build-settings-and-addressables.md)
- [Find a problem](../reference/find-a-problem.md)
