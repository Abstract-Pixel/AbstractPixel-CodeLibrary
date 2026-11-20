# Scene Management

Opens, closes and switches groups of scenes for a single-player Unity game: a main scene plus the manager scenes and shared scenes that come with it, with optional preload, Addressables and a screen effect around each change.

## Start here

- **Words:** [GLOSSARY.md](GLOSSARY.md). Use these terms in code, docs and issues.
- **Why it is built this way:** [docs/adr/](docs/adr/). One short file per decision that is hard to reverse.
- **How a scene change runs today:** the sections below (messages, order per entry point, loader choice, contract tests).
- **How the game starts:** no start-up object is needed in any scene. Set the start scene group (and an optional launch scene, such as a splash) in Project Settings > Abstract Pixel > Scene System > Start-up. When the game launches or Play is pressed, start-up (`SceneSystemStartUp`) opens the right scene group, then raises `OnNewSceneGroupTransitionTo` once and `OnGameStarted` once. A launch scene waits for `SceneActions.HasGameStarted` instead of loading scenes itself (ADR 0009). While it plays, the **launch scene hold** keeps the start scene group back: its scenes load and wake behind the launch scene, and scripts that show something (camera, music, entrance) wait with `SceneActions.RunWhenLaunchSceneReleased` (or read `SceneActions.IsLaunchSceneHeld`). The launch scene calls `SceneActions.ReleaseLaunchScene()` at its hand-off. Start-up holds only when the launch scene is one of the scenes Unity opens, so Play from a level or the start scene group never waits.
- **Work in progress:** the rework plan lives in the game that tests this module, Project Rally Shift, at `.scratch/scene-coordinator/plan.md` (with `map.md` and `issues/`). Parts of the glossary and the ADRs (scene sets, loading scene, scene transition executor, placement) describe decisions for sessions that are not built yet; the plan says which.

## Editor tools (sessions 3C, 4A and 4B)

- **Open the group with its main scene** (4B). Opening a scene on its own also opens what Play would load with it, so the Hierarchy shows what Play loads: a group's main scene opens its manager and shared scenes; a manager or shared scene opens the start scene group; a scene in no group opens the default scene set's scenes; the launch scene opens nothing. The opened scene stays active. Switching groups closes only scenes the tool opened that the new group does not need (unsaved ones get Unity's save prompt); scenes you opened by hand are never closed. Off in Play mode, builds, prefab mode, test runs, "Open group", and while Unity restores scenes. Never marks a scene dirty or changes Build Settings. Preferences > Abstract Pixel > Scenes, or Tools > External Project Tools > Scene System > Open the Group with Its Main Scene, turns it off (per user, per project); Play still loads the missing scenes.
- **Start-up section** (4B) on the Scene System page: start scene group (required), launch scene (optional) with "Create a launch scene…". The launch scene is kept at build index 0: choosing it puts it there, the page warns with Fix now when Build Settings were reordered, a build from the Build Profiles window puts it back first, and a scripted build that finds it moved puts it back and stops with a message (build again). The setup wizard has an optional Launch Scene step (none, a scene you have, or a new one).

- **Setup.** In a project with no scene system settings asset, a short setup window opens once per Editor session (not in batch mode or test runs): settings folder, where new scenes go, and whether to organize existing scene groups into scene sets. The Abstract Pixel logo sits on top. The same wizard sits in the Scene System page's Setup section. Without scene groups it makes a starter scene set: one empty manager scene holding a copy of the basic fade (ADR 0004). "Don't show again" is per project; the Setup section turns it back on.
- **Scene system settings.** One asset per project, in any folder: found by type (its GUID is cached), kept in Player Settings > Preloaded Assets, and put back there before a build. It holds the scene sets and the default, where new scenes go, the pre-build check, and the list of scene groups (kept current by `SceneSystemAssetWatcher`).
- **Scene System page** (Project Settings > Abstract Pixel > Scene System). Explanations are in tooltips; names, values, status and problems are always visible. Setup (settings asset path with Move…, and whether builds can read the settings); Scene sets (numbered foldout cards: name, path that pings the asset, Make default or "Chosen default scene set", group count; open a card for its manager scenes, shared scenes, Reset shared scenes and the scene groups using it; + makes a set, − deletes the selected one after asking, and is off while any group uses it); New scenes (Build Settings or Addressables, "Move all scenes of all groups to Addressables…"); Scene Groups Debugger (the pre-build check toggle, then every group: name that selects it, main scene, scene set dropdown that keeps overrides, problems with their fixes, Open group; "Check again" and the summary sit in its header; Preview migration and Migrate while groups have no scene set). Edits have Undo. Every edit is written to disk at once, and so is its Undo or Redo (`SceneSystemUndoSave`); Build Settings changes are saved through `SceneBuildSettingsWriter`, because setting `EditorBuildSettings.scenes` alone stays in memory until the next project save (4B-2 persistence audit). "Open Scene System" in the scene group and scene set inspectors opens the page; a build stopped by the group check opens it at the Debugger.
- **Scene sets fill scene groups** (ADR 0008). A group names its set and still stores its own lists, which the loader reads. Each of manager scenes, shared scenes and Reset shared scenes is either "From set" or an Override; editing a part in the group inspector makes it an override, editing it back clears it, and Revert to set ends it. Changing a set fills every group that uses it, except its overrides. New groups from Create Scene Group start on the default set.
- **Migration.** One scene set per distinct (manager scenes, shared scenes, Reset shared scenes) among groups with a main scene; each group points at its set with no overrides. A markdown report next to the settings asset lists every group before and after; groups without a main scene are listed as unused, with the files that still reference them, and never deleted.
- **Rename tracking.** Renaming or moving a scene re-saves every .asset, .prefab and .unity under Assets/ (up to 16 MB each) that holds its GUID, so stored names and paths follow; scenes open with unsaved changes are skipped with a warning. In the Editor a scene field reads its scene's live name, so Play works before any re-save. Before a build, scene groups with stale names are re-saved.
- **Scene field.** One status box per scene: In build, Addressable, Not in build, or Missing. A scene that is Not in build gets one fix button that follows where new scenes go ("Add to build" or "Make Addressable"). Right-click: Move to Addressables / Move to Build Settings. Each move removes the other entry. Core never references Addressables or this module: this module registers an `IAddressableSceneProvider` and sets `SceneStatusReader.ProjectDefaultPlace`.
- **Create Scene Group.** Right-click a scene, Create > Scene Group: a new group next to the scene, on the default scene set, its scenes placed where new scenes go.
- **Saving a group** places its scenes that are Not in build where new scenes go, with one console line each.
- **Group check.** The inspector lists problems only when there are some: no main scene, a Missing scene, a scene Not in build, the same scene listed twice (every problem stops a build when the check is on). A group with no main scene and no scene set that lists boot scenes is a boot group from before scene sets, not a problem. The Scene System page's Scene Groups Debugger lists every group.
- **Deferred work** from asset postprocessors and save hooks runs through `EditorIdleQueue` (Core), not `EditorApplication.delayCall`, which can wait until the Editor window is focused.

---

# Scene messages

What `SceneEventBus` sends, in what order, and who listens in Project Rally Shift. Read from the code as of the
`scene-system-rework` branch, after session 3B. Go to a level, Retry, Preload and Switch to preloaded share one start step
and one finish step. A preload is settled before any other scene change starts. Nothing in Rally Shift calls preload yet.

## The messages

| Message | Argument | Sent by |
|---|---|---|
| `OnNewSceneGroupTransitionTo` | the new group | Start-up, go to a level, Retry, switch to preloaded. Not sent when a scene fails to load |
| `OnGameStarted` | the first group | Start-up only, once per session, right after its `OnNewSceneGroupTransitionTo` |
| `OnLaunchSceneReleased` | none | `SceneActions.ReleaseLaunchScene()`, once, when the launch scene hands over. Never sent when nothing was held (Play from a level or the start scene group) |
| `OnMainSceneLoaded` | the main scene | `DefaultSceneLoader`, after it activates a main scene and makes it the active scene. A held (preloaded) main scene fires it when the switch activates it |
| `OnSceneGroupUnloaded` | the group that **closed** (the one that was active when the change started) | Go to a level, Retry, switch to preloaded. Not sent when there was no previous group. Not sent when a preload replaces an earlier preload |
| `OnSceneGroupPreloaded` | the preloaded group | Preload, when it finished |
| `OnPreloadedSceneGroupActivated` | the preloaded group | Switch to preloaded, after `OnNewSceneGroupTransitionTo` |

## Order, per entry point

The fade wraps every entry point that goes through `TransitionActions`: `OnTransitionInStarted`, `OnTransitionInCompleted`,
then the scene work below, then `OnTransitionOutStarted` and `OnTransitionOutCompleted`. These are `TransitionEventBus`
messages from the fade system, not scene messages.

**Start-up** (`SceneSystemStartUp`, `[RuntimeInitializeOnLoadMethod]`): scene changes are refused from before the first
scene loads (BeforeSceneLoad) until start-up ends, with a logged reason.
0. Before any scene's Awake (BeforeSceneLoad) it holds the **launch scene hold** when the settings name a launch scene and
   Unity is opening it (`StartUpRule.ShouldHoldLaunchScene`), with one log line. Unity lists the first scenes by then (a
   player's first scene is listed but not loaded yet, so it counts as opening: `StartUpRule.CountsAsOpening`). With no settings found yet it decides again after the first scenes
   load, before any `Start`. The hold lasts until the launch scene calls `SceneActions.ReleaseLaunchScene()` (after
   `OnGameStarted`, or after `HasStartUpFailed` to recover); that runs the waiting actions, raises
   `OnLaunchSceneReleased` once and logs one line. Released before `OnGameStarted`: one warning, and it still releases.
1. After the first scenes load (AfterSceneLoad) it reads the scene system settings (Preloaded Assets in a build; found by
   type in the Editor) and picks a group from the scenes Unity opened (`StartUpRule`): the launch scene open means the
   start scene group behind it, one frame later; otherwise the first open scene, active first, that is a group's main
   scene picks that real group (two open: an error, the first wins); a manager or shared scene active means the start
   scene group, with one log line; a scene in no group becomes the main scene of a temporary group from the default scene
   set, with one warning. Scenes match by GUID, then path, then name. No settings: one error and nothing else.
2. It creates the coordinator if no scene holds one, and runs the start step.
3. It asks for every scene of the group that is not open, at once and in order (main scene, manager scenes, shared
   scenes), so Unity opens the main scene first and makes it active before the others wake, and every request is made
   before the first frame's `Start`. It waits for all of them. Nothing is closed; a launch scene closes itself.
4. Finish step: records the group, `OnNewSceneGroupTransitionTo` once, then `OnGameStarted` once
   (`SceneActions.HasGameStarted` turns true). A failed load raises neither; `SceneActions.HasStartUpFailed` turns true.

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

The coordinator creates its `SceneLoaderSelector` with itself (not in `Start`), so a change in its first frame has a loader. Each scene picks its loader from the scene asset's state, not from a
field you fill in:

| The scene is | Loader | How it is found |
|---|---|---|
| In Build Settings (or known only by name, like the active scene or a scene restored from a save) | `DefaultSceneLoader` | `SceneManager.LoadSceneAsync(name)`. An In-build scene is recognised by its path at once, without starting Addressables |
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
| `OnGameStarted` | `Splash Screen/Scripts/SplashSequenceDirector.cs` | Hands off to the menu one frame after the game started |
| | `ProjectServices/LevelManager/LevelManagerScripts/Logic/RallyLevelManager.cs` | Syncs the current level from the real scene group asset (its `Awake` runs before start-up records the group) |
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
