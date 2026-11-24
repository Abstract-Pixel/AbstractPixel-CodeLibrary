# AddressablesSceneLoader

<!-- layout new -->

Opens and closes scenes that are Addressable, and keeps each open scene's Addressables handle so a level's assets leave memory when it closes.

<!-- type-header AbstractPixel.SceneManagement.AddressablesSceneLoader -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/SceneLoaderTypes/AddressablesScemeLoader.cs" "Select the script" -->

Read more in [Build settings and addressables](../guide/build-settings-and-addressables.md#which-loader-opens-a-scene).

## Start here

You do not create this loader. The `SceneLoaderSelector` sends it every Addressable scene.

| You want to | Use | Why |
|---|---|---|
| Load a scene through Addressables | Make the scene Addressable | The selector reads the scene asset and picks this loader. |
| Free a level's assets | Close its scene | `UnloadScene` releases the handle, which lets the assets leave memory. |

**What it does for you**

- **Contract:** follows the held scene contract on `ISceneLoader`.
- **Asset GUID:** a scene is known to Addressables by its asset GUID, so a reference without one is refused.
- **One copy per scene:** asking again for an open scene only activates it when it is held.
- **Only through the API:** it calls Addressables through `IAddressablesSceneApi`, so the contract tests can run it on a fake.

<!-- members AbstractPixel.SceneManagement.AddressablesSceneLoader -->

## See also

- [Build settings and addressables](../guide/build-settings-and-addressables.md)
- [Find a problem](../reference/find-a-problem.md)
