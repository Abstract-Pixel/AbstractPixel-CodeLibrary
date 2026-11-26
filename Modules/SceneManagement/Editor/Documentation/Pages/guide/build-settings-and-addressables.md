# Build Settings and Addressables <!-- level 1 -->

<!-- layout new -->

Each scene loads from Build Settings or from Addressables, never both. The scene system picks the loader from the scene asset itself, so you only choose where new scenes go.

## Where new scenes go <!-- level 0 -->

Pick Build Settings or Addressables in the New scenes section of the Scene System page. Create Scene Group, and saving a group, put their scenes there.

<!-- action open-settings "New scenes" "Open New scenes" -->

## Which loader opens a scene <!-- level 1 -->

You never pick a loader; the scene system reads the scene asset.

| The scene is | Loader |
|---|---|
| In Build Settings, or known only by name | `DefaultSceneLoader` |
| Addressable | `AddressablesSceneLoader` (needs the scene's asset GUID) |

- **Once per open scene:** the choice is made when a scene opens; its close goes to the loader that opened it.
- **Memory:** the Addressables loader keeps each scene's handle and releases it when the scene closes, so the level's assets leave memory.
- **No Addressables content:** the first check for a scene with a GUID starts Addressables. A game with no Addressables content logs a warning and loads every scene from Build Settings.

> [!WARNING]
> An address must not contain `[` or `]`: Addressables reads them as runtime variables and never finds the scene. Give an Addressable scene a plain address.

**Try it in the sample**

<!-- action open-sample 08 "Open sample 08" -->

**How the loaders work**

- **One scene field:** `SceneReference` wraps a `SceneField`, which stores the scene asset's GUID, path and name. Two references match by GUID when both have one, otherwise by name. The Editor refreshes the three strings every time the field is saved, so a rename or move follows the asset.
- **The selector:** asks Addressables (`AddressableSceneCheck`) whether the GUID has an entry, remembers which loader opened the scene (by GUID and by name), and sends a close, or a second request for a held scene, to that same loader.
- **Freeing memory:** goes through the Build Settings loader. It is one `Resources.UnloadUnusedAssets`.
- **Only the loader talks to Unity:** `AddressablesSceneLoader` calls Addressables through `IAddressablesSceneApi`, so the contract tests can run it on a fake.
- **Starting Addressables:** the first check for a scene with a GUID calls `Addressables.InitializeAsync()`.
- **Addresses:** a scene whose address is its asset path under `Assets/[1]Project/...` never resolves ("No Location found").

<details><summary>More: why the Addressables handle is kept</summary>

- **What it does:** the loader keeps the Addressables handle of every open scene and releases it when the scene closes. That is what lets the level's assets leave memory.
- **Release mode:** scenes are requested with `SceneReleaseMode.OnlyReleaseSceneOnHandleRelease` and released by hand.
- **Why:** Addressables reports a held scene as the last scene that was in the list when the load was asked for, and would otherwise release or close the wrong one.
- **Finding the scene again:** by its path, then by its name.

</details>

<details><summary>More: contract tests for loaders</summary>

`SceneLoaderContractTests` (in `Tests/Editor`) is the four-rule held scene contract on `ISceneLoader`, written once and run by every loader's fake through an `ILoaderContractHarness`.

| Subclass | What it runs on |
|---|---|
| `RecordingSceneLoaderContractTests` | `RecordingSceneLoader`, the fake the coordinator tests use |
| `AddressablesSceneLoaderContractTests` | `AddressablesSceneLoader` on `FakeAddressablesApi`, which follows Unity's rule for a held scene |

- **A new loader:** adds one harness and one subclass.
- **Loader-specific tests:** `AddressablesSceneLoaderTests` and `SceneLoaderSelectorTests` hold what is specific to the Addressables loader and the selector: one open copy per scene, the handle kept until close, the loader a scene closes with.

</details>

<details><summary>More: known limits of preload</summary>

- **Reloaded shared scenes:** in a switch to a preloaded group, a force-reloaded shared scene opens after the level is active, not before.
- **Discarding a preload:** discarding a held preload lets its level wake for a moment before it closes.

</details>

## Fix a scene status <!-- level 0 -->

A scene field shows its [scene status](term:scene-status). Not in build gets one fix button that follows where new scenes go.

- **Fix button:** Add to build, or Make Addressable when new scenes go to Addressables.
- **Right-click the field:** Move to Addressables or Move to Build Settings.

<details><summary>Why it works</summary>

- **Addressables is optional:** the module works without it ([design decision 5](../../../../docs/adr/0005-addressables-and-broaudio-are-optional.md)).
- **Samples stay out:** [sample scene groups](term:sample-scene-group) never reach a build ([design decision 14](../../../../docs/adr/0014-sample-scene-groups-never-reach-a-build.md)).

</details>

<details><summary>If it goes wrong</summary>

- **"No Location found":** the address holds `[` or `]`.
- **A scene is Missing:** the asset was deleted. Pick another scene.
- **A build stops with a group check:** open the [Scene Groups Debugger](editor-tools.md#scene-groups-debugger) and fix the problem it names.

</details>
