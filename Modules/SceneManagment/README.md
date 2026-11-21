# Scene Management

Opens, closes and switches groups of scenes for a single-player Unity game: a main scene plus the manager scenes and shared scenes that come with it, with optional preload, Addressables and a screen effect around each change.

## Start here

- **Words:** [GLOSSARY.md](GLOSSARY.md). Use these terms in code, docs and issues.
- **Why it is built this way:** [docs/adr/](docs/adr/). One short file per decision that is hard to reverse.
- **How a scene change runs today:** the sections below (messages, order per entry point, loader choice, contract tests).
- **How the game starts:** no start-up object is needed in any scene. Set the start scene group (and an optional launch scene, such as a splash) in Project Settings > Abstract Pixel > Scene System > Start-up. When the game launches or Play is pressed, start-up (`SceneSystemStartUp`) opens the right scene group, then raises `OnNewSceneGroupTransitionTo` once and `OnGameStarted` once. A launch scene waits for `SceneActions.HasGameStarted` instead of loading scenes itself (ADR 0009). While it plays, the **launch scene hold** keeps the start scene group back: its scenes load and wake behind the launch scene, and scripts that show something (camera, music, entrance) wait with `SceneActions.RunWhenLaunchSceneReleased` (or read `SceneActions.IsLaunchSceneHeld`). The launch scene calls `SceneActions.ReleaseLaunchScene()` at its hand-off. Start-up holds only when the launch scene is one of the scenes Unity opens, so Play from a level or the start scene group never waits.
- **Changing scene:** `SceneActions.ChangeTo(group)`, `Retry()`, `Preload(group)`, each returning a `SceneChangeResult`; the screen effect and the loading scene run inside the call. Read `SceneActions.LoadingProgress` and `TransitionActions.ScreenState`. Details below ("Scene messages", "Loading scene").
- **Work in progress:** the rework plan lives in the game that tests this module, Project Rally Shift, at `.scratch/scene-coordinator/plan.md` (with `map.md` and `issues/`). Placement (ADR 0006) is not built yet; the plan says which decisions belong to later sessions.

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

What `SceneEventBus` and `TransitionEventBus` send, in what order, and who listens in Project Rally Shift. Read from the
code as of the `scene-system-rework` branch, after session 4C. A game changes scene with one call (ADR 0001):
`SceneActions.ChangeTo(group)`, `SceneActions.Retry()` or `SceneActions.Preload(group)`. Each returns a
`SceneChangeResult`: **Done**, **Refused** with a reason (`Busy`, `StartUpRunning`, `AlreadyOpen`, `NoGroupGiven`,
`NothingOpenToRetry`) or **Failed** with the scene that failed. None of them throws. `MonoSceneActions` has the same three
as void methods for UnityEvents, and `TransitionActions.TransitionToSceneWithEffects` / `ReloadActiveScenesWithEffects`
stay as one-line forwards to `ChangeTo` and `Retry` for buttons that call them by name.

## The messages

| Message | Argument | Sent by |
|---|---|---|
| `OnNewSceneGroupTransitionTo` | the new group | Start-up, a change, Retry, a switch to a preloaded group, the fallback scene group. Not sent for a group whose scene failed |
| `OnGameStarted` | the first group | Start-up only, once per session, right after its `OnNewSceneGroupTransitionTo` |
| `OnLaunchSceneReleased` | none | `SceneActions.ReleaseLaunchScene()`, once, when the launch scene hands over. Never sent when nothing was held |
| `OnMainSceneLoaded` | the main scene | The loader, after it activates a main scene and makes it the active scene |
| `OnSceneGroupUnloaded` | the group that **closed** | A change, Retry, a switch to a preloaded group. Not sent when there was no previous group |
| `OnSceneGroupPreloaded` | the preloaded group | Preload, when it finished |
| `OnPreloadedSceneGroupActivated` | the preloaded group | A change to the preloaded group, after `OnNewSceneGroupTransitionTo` |
| `OnSceneChangeFailed` | the group asked for, the scene that failed | A change, Retry or the fallback, once per failed change (4C, ADR 0011). Not sent for a failed preload |
| `TransitionEventBus.OnScreenStateChanged` | the new `ScreenState` | The transitions module: Visible → Hiding → Hidden → Showing → Visible |
| `OnStartHideScreenTransition`, `OnEndHideScreenTransition` | none | Hide screen starts / the screen counts as covered (was In started / In completed) |
| `OnStartShowScreenTransition`, `OnEndShowScreenTransition` | none | Show screen starts / ends (was Out started / Out completed) |

`SceneEventBus`'s `Raise*` methods and `ResetStatics` are internal: only the scene system raises scene messages.
`TransitionActions.ScreenState` is read-only and resets with domain reload off. Neither module reads or writes
`Time.timeScale`: game management owns game time and reacts to the screen state (ADR 0010).

## Order, per entry point

**Start-up** (`SceneSystemStartUp`, `[RuntimeInitializeOnLoadMethod]`): scene changes are refused (`StartUpRunning`) from
before the first scene loads until start-up ends. It never uses the screen effect or a loading scene.
0. Before any scene's Awake (BeforeSceneLoad) it holds the **launch scene hold** when the settings name a launch scene and
   Unity is opening it (`StartUpRule.ShouldHoldLaunchScene`, `StartUpRule.CountsAsOpening`). The launch scene releases
   it with `SceneActions.ReleaseLaunchScene()`: waiting actions run, `OnLaunchSceneReleased` once, one log line.
1. After the first scenes load it reads the settings and picks a group from the scenes Unity opened (`StartUpRule`).
2. It creates the coordinator if no scene holds one.
3. It asks for every missing scene of the group at once and in order (main scene, manager scenes, shared scenes).
4. Records the group, `OnNewSceneGroupTransitionTo` once, then `OnGameStarted` once. A failed load raises neither;
   `SceneActions.HasStartUpFailed` turns true (no fallback for start-up).

**A change** (`ChangeTo`) and **Retry** (`Retry`) share one body. A `SceneChangePlan` works out which scenes to close and open.
1. Refusals first, each before anything touches the screen: no group (`NoGroupGiven`), the group asked for is the open
   asset (`AlreadyOpen`; groups are equal only to themselves), nothing open to Retry (`NothingOpenToRetry`), start-up
   running, another change running (`Busy`). Busy is set before the first `await` and stays set until Show screen ends.
2. **Hide screen** through the registered `ISceneTransitionExecutor` (none registered: nothing). A Hide screen asked for
   while an effect still runs waits for it, so the screen is always covered.
3. A preload this change does not use is closed (held main scene first). Unity holds every later scene operation while a
   preloaded main scene waits. A change to the preloaded group itself runs as the preloaded switch in step 5.
4. **Loading scene**, when the group shows one (next section): it opens under the cover and Show screen uncovers it.
5. Close the old main scene and the shared scenes (all with Reset shared scenes), free memory once,
   `OnSceneGroupUnloaded`, open manager scenes, shared scenes, then the main scene (`OnMainSceneLoaded`). Loading
   progress fills. A switch to a preloaded group activates the held main scene first, then closes and opens the rest.
6. Done: record the open scenes, `OnNewSceneGroupTransitionTo` once (then `OnPreloadedSceneGroupActivated` for a switch).
   Failed: record what is really open (no group counts as open), `OnSceneChangeFailed` once, then the **fallback scene
   group** (settings: "If a scene change fails, go to"; empty means the start scene group) runs steps 5 and 6 once,
   with the screen still covered and the loading scene still up. If the fallback fails too, or the failed change was
   the fallback, the screen is shown on whatever is open and an error is logged. A failed load is never tried again. A
   scene that fails to close is logged and the change carries on.
7. The loading scene leaves (minimum time, "full" animation, Hide screen, it closes).
8. **Show screen**, then busy clears. The result says Done, or Failed with the scene.

**Preload** (`Preload`): refused for no group, the open group, start-up or a running change; Done at once when the group is
already preloaded. It never touches the screen and never shows a loading scene. An earlier preload closes first (memory
freed once), then the missing manager and shared scenes open and the main scene is held at 90%. `OnSceneGroupPreloaded`.
A failed preload closes what it opened, returns Failed and goes nowhere. The scene system never preloads by itself: a
change uses a preload only when the game preloaded that same group.

The coordinator makes no scene load or unload call of its own: every open and close goes through `ISceneLoader`, whose
held-scene contract is written on the interface and tested with the recording loader in `Tests/Editor`. A loader fails a
load it cannot do (a scene missing from the build, an Addressable scene without a GUID) by throwing; it never logs and
returns. `ISceneLoader.GetLoadProgress` reports a scene's own progress for the loading progress.

# Loading scene

One scene shown while the old place closes and the new one opens (ADR 0003, amended in 4C). Project Settings > Abstract
Pixel > Scene System > **Loading scenes** lists them, names the default and sets the **minimum time on screen** (unscaled
seconds, counted from Show screen starting). Each scene group says **Show the loading scene**: Never, When arriving from
another scene group (a Retry goes straight back in), or Every time, Retry included, with a loading scene picker (the
default preselected). Start-up and a switch to a preloaded group never show it.

- **With a screen effect:** Hide screen, the loading scene opens, Show screen, the old scenes close, memory is freed, the
  new scenes open while progress fills, the loading progress view plays its "full" animation, Hide screen, the loading
  scene closes, Show screen on the new level.
- **Without one:** the same steps make an instant cut; the loading scene opens before the old level closes, so no frame
  has no camera.
- **Loading progress:** `SceneActions.LoadingProgress`, 0 to 1. Every scene the change opens has an equal share, the value
  never goes back and is 1 only once every scene is open. Safe to read every frame. Not smoothed.
- **SceneScreens** (`Modules/SceneManagment/SceneScreens`, its own assembly; the scene module never references it):
  `LoadingProgressView` reads the progress, smooths it (feel: speed, optional curve, hold at full) and raises On Shown,
  On Progress (float), On Full and On Hidden. It also drives an `ILoadingProgressDisplay` (Show, SetProgress, PlayFull,
  Hide) found on its object or assigned. It registers itself with `SceneActions.RegisterLoadingProgressView`; the
  scene system calls it when Show screen starts and waits for its full beat and hold before Hide screen.
- **Feel with no code:** drag `MMProgressBar.UpdateBar01` into On Progress and an `MMF_Player.PlayFeedbacks` into On Full.
  **Code animations:** implement `ILoadingProgressDisplay` (Rally's `RallyLoadingFullBeat` plays `IAnimationCommand`s).
- **Ready-made:** `SceneScreens/ReadyMade/Loading Scene.unity` (opaque background in the ready-made fade's colour, a plain
  bar, no art). The setup clones it, lists the copy as the default, and clones the ready-made fade with its controller
  prefab, so nothing the project uses points into the library (ADR 0004). Visual rules: the effect's canvas draws above
  everything (Screen Space Overlay, highest sort order); the loading scene's canvas is opaque and full screen.
- **Launch scene helpers** also live in SceneScreens: `LaunchSceneReleaseMarker` + `LaunchSceneReleaseReceiver` (a
  Timeline marker that releases the launch scene hold once start-up has finished), `LaunchSceneRenderSettingsGuard`,
  `LaunchSceneTimelineClock` and `LaunchSceneStartUpLog`.

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
| `OnSceneGroupUnloaded`, `OnSceneGroupPreloaded`, `OnPreloadedSceneGroupActivated`, `OnSceneChangeFailed` | none | |

Rally paths are under `Assets/[1]Project/` unless shown otherwise. Many other systems read
`SceneActions.ActiveSceneGroup` directly (win screen, leaderboards, ghosts, level finish, `RallyLevelManager`) instead of
listening.

## Screen messages used to relocate the car (until session 4D replaces them)

| Message | Listener | Purpose |
|---|---|---|
| `TransitionEventBus.OnStartShowScreenTransition` | `GameplaySystems/Car/CarCore/CarScripts/CarBootPhysicsToggle.cs` | Unlocks the car (it was frozen while the screen was covered), turns on its safety recorder, and sets `CarCanMove` |
| | `SceneTransitionSystem/.../FallbackAudioListener.cs`, `TransitionAudio.cs` (library) | Hands audio back to the scene's listener, plays the reveal sound |
| `TransitionEventBus.OnEndShowScreenTransition` | `CarBootPhysicsToggle.cs` | Same handler, as a safety net. It runs once |

With a loading scene, the new car loads while the loading scene covers the level and is released by the last Show screen. Rally's pause is refused while a scene change runs (`RallyPauseCondition`). With Play pressed inside a level there is no fade. `CarBootPhysicsToggle.Start` unlocks the car instead, in the Editor
only.
