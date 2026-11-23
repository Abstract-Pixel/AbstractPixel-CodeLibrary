# AddressablesSceneLoader

Opens and closes scenes that are Addressable. It follows the held scene contract on ISceneLoader, and keeps the Addressables handle of every open scene so a level's assets leave memory when it closes.

**Kind:** class · **Namespace:** `AbstractPixel.SceneManagement` · **File:** `AddressablesScemeLoader.cs`

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/SceneLoaderTypes/AddressablesScemeLoader.cs" "Select the script" -->

Read more in [Build settings and addressables](../guide/build-settings-and-addressables.md#which-loader-opens-a-scene).

A scene is known to Addressables by its asset GUID, so a reference without one is refused.

## Members

<!-- members AbstractPixel.SceneManagement.AddressablesSceneLoader -->

## See also

- [Build settings and addressables](../guide/build-settings-and-addressables.md)
- [Find a problem](../reference/find-a-problem.md)
