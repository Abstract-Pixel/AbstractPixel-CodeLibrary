# SceneLoaderSelector

The loader the coordinator uses. Each scene picks its own loader from the asset's state: an Addressable scene opens through the AddressablesSceneLoader, every other scene through Build Settings.

**Kind:** class · **Namespace:** `AbstractPixel.SceneManagement` · **File:** `SceneLoaderSelector.cs`

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/SceneLoaderTypes/SceneLoaderSelector.cs" "Select the script" -->

Read more in [Build settings and addressables](../guide/build-settings-and-addressables.md#which-loader-opens-a-scene).

A scene is closed by the loader that opened it, so a held scene and its handle never change hands.

## Members

<!-- members AbstractPixel.SceneManagement.SceneLoaderSelector -->

## See also

- [Build settings and addressables](../guide/build-settings-and-addressables.md)
- [Find a problem](../reference/find-a-problem.md)
