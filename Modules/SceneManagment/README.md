# Scene Management

Opens, closes and switches groups of scenes for a single-player Unity game: a main scene plus the manager scenes and shared scenes that come with it, with optional preload, Addressables and a screen effect around each change.

## Start here

- **Words:** [GLOSSARY.md](GLOSSARY.md). Use these terms in code, docs and issues.
- **Why it is built this way:** [docs/adr/](docs/adr/). One short file per decision that is hard to reverse.
- **How a scene change runs today:** the sections below (messages, order per entry point, loader choice, contract tests).
- **Work in progress:** the rework plan lives in the game that tests this module, Project Rally Shift, at `.scratch/scene-coordinator/plan.md` (with `map.md` and `issues/`). Parts of the glossary and the ADRs (scene sets, loading scene, scene transition executor, placement) describe decisions for sessions that are not built yet; the plan says which.

## Editor tools (session 3C)

- **Scene field.** Every scene field shows its scene status once, in one box: In build, Addressable, Not in build, or Missing (the asset is gone; the box shows the old path). A scene that is Not in build gets one fix button, "Add to build" (the project default is Build Settings until Project Settings exists). Each fix also removes the scene's entry from the other place, so a scene is never In build and Addressable. Core never references Addressables: this module registers an `IAddressableSceneProvider` when the package is installed (`ABSTRACTPIXEL_ADDRESSABLES`, from the editor assembly's `versionDefines`). Without it no scene counts as Addressable and only "Add to build" is offered.
- **Create Scene Group.** Right-click a scene in the Project window, Create > Scene Group: a new `<scene>Group.asset` next to the scene with it as the main scene, and the scene put into Build Settings unless it is Addressable. If another group already has the scene as its main scene, that group is selected instead.
- **Saving a group** puts its scenes that are Not in build into Build Settings, with one console line. Addressable scenes are left alone; so is "Add scenes to Build Settings".
- **Group check.** The scene group inspector lists problems only when there are some: no main scene, a Missing scene, a scene Not in build, the same scene listed twice. Each names the group and the scene and has a one-click fix where one exists ("Add to build", "Remove from group"). Tools > External Project Tools > Scene System > Check All Scene Groups lists every group. "Check Scene Groups Before Build" in the same menu (off by default, an editor preference on this machine) stops a build on any problem, naming each group and scene.

---

# Scene messages

What `SceneEventBus` sends, in what order, and who listens in Project Rally Shift. Read from the code as of the
`scene-system-rework` branch, after session 3B. Go to a level, Retry, Preload and Switch to preloaded share one start step
and one finish step. A preload is settled before any other scene change starts. Nothing in Rally Shift calls preload yet.

## The messages

| Message | Argument | Sent by |
|---|---|---|
| `OnNewSceneGroupTransitionTo` | the new group | Start-up, go to a level, Retry, switch to preloaded. Not sent when a scene fails to load |
| `OnMainSceneLoaded` | the main scene | `DefaultSceneLoader`, after it activates a main scene and makes it the active scene. A held (preloaded) main scene fires it when the switch activates it |
| `OnSceneGroupUnloaded` | the group that **closed** (the one that was active when the change started) | Go to a level, Retry, switch to preloaded. Not sent when there was no previous group. Not sent when a preload replaces an earlier preload |
| `OnSceneGroupPreloaded` | the preloaded group | Preload, when it finished |
| `OnPreloadedSceneGroupActivated` | the preloaded group | Switch to preloaded, after `OnNewSceneGroupTransitionTo` |

## Order, per entry point

The fade wraps every entry point that goes through `TransitionActions`: `OnTransitionInStarted`, `OnTransitionInCompleted`,
then the scene work below, then `OnTransitionOutStarted` and `OnTransitionOutCompleted`. These are `TransitionEventBus`
messages from the fade system, not scene messages.

**Start-up** (`SceneBootstrapper` calls `SceneActions.InitializeStartSceneData`): the bootstrapper starts loading its
missing boot scenes without waiting for them, then the coordinator records the group and sends
`OnNewSceneGroupTransitionTo`. The boot scenes may still be loading at that moment.

**Go to a level** (`TransitionToSceneGroup`) and **Retry** (`ReloadActiveSceneGroup`) share the same steps. A
`SceneChangePlan` works out first which scenes to close and open. Retry is "go to the group that is already open".
1. Start step (`TryBeginSceneChange`): if a scene change is already running, log a warning and refuse. Otherwise set
   "busy" (`IsLoadingSceneGroup` and `IsUnloadingSceneGroup`) before the first `await`, so a second request in the same
   frame is refused.
2. **Settle the preload** (`SettlePreloadBeforeChange`), before any scene closes. Unity holds every later scene
   operation, unloads included, while a preloaded main scene waits, so a change that starts with a preload held would
   never finish. With no preload this does nothing. When go to a level targets the preloaded group, the whole change
   runs as **Switch to preloaded** below (one "new level", then `OnPreloadedSceneGroupActivated`) and the steps after
   this one are skipped. For anything else (a different level, or Retry) the preload is discarded: the held main scene
   closes first, then its other scenes. That discard does not release memory, because the change releases it once.
3. Close the old main scene, then the shared scenes (all of them with `ForceReloadContextualScenes`, otherwise only the
   ones the new group does not use). Manager scenes stay open.
4. Release unused memory once.
5. `OnSceneGroupUnloaded` with the group that closed.
6. Open manager scenes, shared scenes, then the main scene. `OnMainSceneLoaded` fires as the main scene becomes active.
   A scene that throws is logged by name and stops the change.
7. Finish step (`EndSceneChange`): clear "busy", then record the open shared and manager scenes, make the group the active
   one, then `OnNewSceneGroupTransitionTo` once. A listener that reads `SceneActions.ActiveSceneGroup` sees the new group.
   After a failed load nothing is recorded and `OnNewSceneGroupTransitionTo` is not sent.

**Preload** (`PreloadSceneGroup`):
1. Nothing happens when the group is the open level or is already preloaded.
2. Start step, as above. A preload is refused while any scene change runs.
3. An earlier preload is closed first: its held main scene first, then its other scenes, then unused memory is released
   once. Replacing a preload counts as a scene change for the memory release.
4. Open the manager and shared scenes that are not open yet, then the main scene, held at 90% without activating it (no
   `OnMainSceneLoaded`). Only the main scene is held, because Unity holds every later scene operation while a scene
   waits. A shared scene that is already open is never opened a second time, even with `ForceReloadContextualScenes`.
5. `OnSceneGroupPreloaded`. A preload that fails closes what it opened and raises nothing.

**Switch to preloaded** (`TransitionToPreloadedSceneGroup`):
1. Start step, as above. Refused while any scene change runs. Go to a level to the preloaded group joins here, after its
   own start step. If the preloaded group is already the open level, the held scene is closed, a warning is logged and
   nothing else changes.
2. Activate the held main scene. The loader makes it the active scene and raises `OnMainSceneLoaded`. This comes first
   because Unity holds the close and open steps below while the main scene waits.
3. Close the old main scene and the shared scenes that must reload (all open ones with `ForceReloadContextualScenes`,
   otherwise the ones the new group does not use), release unused memory once, `OnSceneGroupUnloaded` with the group
   that closed.
4. Open the shared scenes that are still missing. The ones the preload opened stay as they are.
5. Finish step (`EndSceneChange`): as for go to a level, and the preload record is cleared before the event. Then
   `OnNewSceneGroupTransitionTo` once, then `OnPreloadedSceneGroupActivated`.

The coordinator makes no scene load or unload call of its own: every open and close goes through `ISceneLoader`, whose
four-rule held-scene contract is written on the interface and tested with the recording loader in `Tests/Editor`.

## Which loader opens a scene

The coordinator starts with a `SceneLoaderSelector`. Each scene picks its loader from the scene asset's state, not from a
field you fill in:

| The scene is | Loader | How it is found |
|---|---|---|
| In Build Settings (or known only by name, like the active scene or a scene restored from a save) | `DefaultSceneLoader` | `SceneManager.LoadSceneAsync(name)` |
| Addressable | `AddressablesSceneLoader` | `Addressables.LoadSceneAsync(guid)`, so the scene needs its asset GUID |

- **One scene field.** `SceneReference` wraps a `SceneField`, which stores the scene asset's GUID, path and name. Two
  references match by GUID when both have one, otherwise by name. The Editor refreshes the three strings every time the
  field is saved, so a rename or move follows the asset. A scene is either in Build Settings or Addressable, never both.
- **The choice is made once per open scene.** The selector asks Addressables (`AddressableSceneCheck`) whether the GUID has
  an entry, remembers which loader opened the scene (by GUID and by name), and sends a close, or a second request for a
  held scene, to that same loader. Memory is released through the Build Settings loader (it is one `Resources.UnloadUnusedAssets`).
- **Only the loader talks to Unity.** `AddressablesSceneLoader` calls Addressables through `IAddressablesSceneApi`, so the
  contract tests can run it on a fake.
- **The handle is kept.** The loader keeps the Addressables handle of every open scene and releases it when the scene
  closes. That is what lets the level's assets leave memory. Scenes are requested with
  `SceneReleaseMode.OnlyReleaseSceneOnHandleRelease` and released by hand, because Addressables reports a held scene as
  the last scene that was in the list when the load was asked for, and would otherwise release or close the wrong one.
  The loader finds the open scene again by its path, then by its name.
- **Addresses must not contain `[` or `]`.** Addressables reads them as runtime variables, so a scene whose address is its
  asset path under `Assets/[1]Project/...` never resolves ("No Location found"). Give an Addressable scene a plain address.
- **Starting Addressables.** The first check for a scene with a GUID calls `Addressables.InitializeAsync()`. A game with no
  Addressables content logs a warning and loads every scene from Build Settings.

### Contract tests

`SceneLoaderContractTests` (in `Tests/Editor`) is the four-rule held scene contract on `ISceneLoader`, written once and run
by every loader's fake through an `ILoaderContractHarness`:

| Subclass | What it runs on |
|---|---|
| `RecordingSceneLoaderContractTests` | `RecordingSceneLoader`, the fake the coordinator tests use |
| `AddressablesSceneLoaderContractTests` | `AddressablesSceneLoader` on `FakeAddressablesApi`, which follows Unity's rule for a held scene |

A new loader adds one harness and one subclass. `AddressablesSceneLoaderTests` and `SceneLoaderSelectorTests` hold what is
specific to the Addressables loader and the selector (one open copy per scene, the handle kept until close, the loader a
scene closes with).

Known limits of preload, on `.scratch/scene-coordinator/map.md`: a force-reloaded shared scene opens after the level is
active in a switch, not before, and discarding a held preload lets its level wake for a moment before it closes.

## Who listens in Rally Shift

| Message | Listener | Purpose |
|---|---|---|
| `OnNewSceneGroupTransitionTo` | `Modules/SaveSystem/Runtime/Core/SaveManager.cs` (library) | At the end of the frame, restores saved data into the new scenes. The first time, it also loads the save profile |
| | `GameplaySystems/Car/CarSubsystems/CarGhostRecorder/LevelGhostSaveLibrary.cs` | Writes ghosts to disk if a run changed them |
| | `ProjectServices/levelMusicPlayer/LevelMusicPlayer.cs` | Fades out the current level music |
| | `ProjectServices/PlatformServices/Achievements/Core/AchievementManager.cs` | Sends pending achievements to Steam |
| `OnMainSceneLoaded` | `Assets/SceneUtility/SceneToObjectChanger.cs` | Turns objects on or off for the scene that just became active. Also hears a preloaded switch now |
| `OnSceneGroupUnloaded`, `OnSceneGroupPreloaded`, `OnPreloadedSceneGroupActivated` | none | |

Rally paths are under `Assets/[1]Project/` unless shown otherwise. Many other systems read
`SceneActions.ActiveSceneGroup` directly (win screen, leaderboards, ghosts, level finish, `RallyLevelManager`) instead of
listening.

## Fade messages used for placement

| Message | Listener | Purpose |
|---|---|---|
| `TransitionEventBus.OnTransitionOutStarted` | `GameplaySystems/Car/CarCore/CarScripts/CarBootPhysicsToggle.cs` | Unlocks the car (it was frozen while the screen was covered), turns on its safety recorder, and sets `CarCanMove` |
| | `SceneTransitionSystem/.../FallbackAudioListener.cs`, `TransitionAudio.cs` (library) | Hands audio back to the scene's listener, plays the reveal sound |
| `TransitionEventBus.OnTransitionOutCompleted` | `CarBootPhysicsToggle.cs` | Same handler, as a safety net. It runs once |

With Play pressed inside a level there is no fade. `CarBootPhysicsToggle.Start` unlocks the car instead, in the Editor
only.
