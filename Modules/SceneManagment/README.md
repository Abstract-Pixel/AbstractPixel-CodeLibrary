# Scene Management

Opens, closes and switches groups of scenes for a single-player Unity game: a main scene plus the manager scenes and shared scenes that come with it, with optional preload, Addressables and a screen effect around each change.

## Start here

- **Words:** [GLOSSARY.md](GLOSSARY.md). Use these terms in code, docs and issues.
- **Why it is built this way:** [docs/adr/](docs/adr/). One short file per decision that is hard to reverse.
- **How a scene change runs today:** the sections below (messages, order per entry point, loader choice, contract tests).
- **How the game starts:** no start-up object is needed in any scene. Set the start scene group (and an optional launch scene, such as a splash) in Project Settings > Abstract Pixel > Scene System > Start-up. When the game launches or Play is pressed, start-up (`SceneSystemStartUp`) opens the right scene group, then raises `OnNewSceneGroupTransitionTo` once and `OnGameStarted` once. A launch scene waits for `SceneActions.HasGameStarted` instead of loading scenes itself (ADR 0009). While it plays, the **launch scene hold** keeps the start scene group back: its scenes load and wake behind the launch scene, and scripts that show something (camera, music, entrance) wait with `SceneActions.RunWhenLaunchSceneReleased` (or read `SceneActions.IsLaunchSceneHeld`). The launch scene calls `SceneActions.ReleaseLaunchScene()` at its hand-off. Start-up holds only when the launch scene is one of the scenes Unity opens, so Play from a level or the start scene group never waits.
- **Changing scene:** `SceneActions.ChangeTo(group)`, `Retry()`, `Preload(group)`, each returning a `SceneChangeResult`; the screen effect and the loading scene run inside the call. Read `SceneActions.LoadingProgress` and `TransitionActions.ScreenState`. Details below ("Scene messages", "Loading scene").
- **Moving the player and other persistent objects:** "Relocating objects" below (session 4D).
- **Work in progress:** the rework plan lives in the game that tests this module, Project Rally Shift, at `.scratch/scene-coordinator/plan.md` (with `map.md` and `issues/`). The plan says which decisions belong to later sessions.

## Editor tools (sessions 3C, 4A and 4B)

- **Scene opening preferences** (4B, split in 5A2). Opening a scene on its own also opens what Play would load with it, so the Hierarchy shows what Play loads. Two preferences decide it, per user and per project, both on by default:
  - **Open a main scene with its scene group:** a group's main scene opens its manager and shared scenes; a manager or shared scene opened alone opens the start scene group; a sample scene opens its sample scene group.
  - **Open a scene in no group with the default scene set:** a scene in no group opens the default scene set's manager and shared scenes.

  The launch scene opens nothing. The first time each is read, it takes the value of the old "Open the group with its main scene" preference, so a project keeps its choice. They act in edit mode only: Play always loads what a scene needs, and start-up never reads them. The opened scene stays active. Switching groups closes only scenes the tool opened that the new group does not need (unsaved ones get Unity's save prompt); scenes you opened by hand are never closed. Off in Play mode, builds, prefab mode, test runs, "Open group", and while Unity restores scenes. Never marks a scene dirty or changes Build Settings. Change them in Preferences > Abstract Pixel > Scenes, Tools > External Project Tools > Scene System (two toggles), setup's Opening Scenes step, or the scene picker's footer. Code: `Editor/OpenWithGroup/` (`SceneOpeningPreferences`, `SceneOpeningPreferenceStore`, `GroupScenesToOpen`).
- **Start-up section** (4B) on the Scene System page: start scene group (required), launch scene (optional) with "Create a launch scene…". The launch scene is kept at build index 0: choosing it puts it there, the page warns with Fix now when Build Settings were reordered, a build from the Build Profiles window puts it back first, and a scripted build that finds it moved puts it back and stops with a message (build again). The setup wizard has an optional Launch Scene step (none, a scene you have, or a new one).

- **Setup** (reworked in 5A2). In a project with no scene system settings asset, a short setup window opens once per Editor session (not in batch mode or test runs). Steps: 1 Scene System, 2 Settings Folder, 3 New Scenes, 4 Launch Scene, 5 Loading Scene, 6 Scene Sets, 7 Opening Scenes, then Ready and All set. A step that brings in a new word explains it under a small "Terminology" heading (the word in bold italics, a one-sentence definition from `SceneSystemTerms`); later steps carry the word's line as a tooltip. Scene Sets asks "Set up your scene sets now?": Yes lists the scene sets to make (the first is the default "Default Scene Set", which lists the starter Managers scene first, removable; pick manager and shared scenes and Reset shared scenes; "Add another scene set"), Not now makes the Default Scene Set with the starter Managers scene (one empty manager scene holding a copy of the basic fade, ADR 0004). Opening Scenes shows the two scene opening preferences. Setup no longer sorts existing scene groups into scene sets: Ready points to the page's Preview migration when there are game groups to sort. Once the project is set up, Tools > External Project Tools > Scene System > Set Up Scene System opens only the Opening Scenes step, with Done. The same wizard sits in the Scene System page's Setup section. "Don't show again" is per project; the Setup section turns it back on.
- **Scene system settings.** One asset per project, in any folder: found by type (its GUID is cached), kept in Player Settings > Preloaded Assets, and put back there before a build. It holds the scene sets and the default, where new scenes go, the pre-build check, and the list of scene groups (kept current by `SceneSystemAssetWatcher`).
- **Scene System page** (Project Settings > Abstract Pixel > Scene System). Explanations are in tooltips; names, values, status and problems are always visible. Setup (settings asset path with Move…, and whether builds can read the settings); Scene sets (numbered foldout cards: name, path that pings the asset, Make default or "Chosen default scene set", group count; open a card for its manager scenes, shared scenes, Reset shared scenes and the scene groups using it; + makes "New Scene Set" and opens its name for typing, − deletes the selected one after asking, and is off while any group uses it; renaming: the name turns link blue on hover, a double-click or F2 opens it as a text field, Enter or clicking away renames the asset file (GUID kept), Esc keeps the old name, and an empty, invalid or taken name shows a red line and keeps editing (`SceneSetNameField`, `SceneSetRename`, `SceneSetNames`)); New scenes (Build Settings or Addressables, "Move all scenes of all groups to Addressables…"); Scene Groups Debugger (the pre-build check toggle, then every group: name that selects it, main scene, scene set dropdown that keeps overrides, problems with their fixes, Open group; "Check again" and the summary sit in its header; Preview migration and Migrate while groups have no scene set). Edits have Undo. Every edit is written to disk at once, and so is its Undo or Redo (`SceneSystemUndoSave`); Build Settings changes are saved through `SceneBuildSettingsWriter`, because setting `EditorBuildSettings.scenes` alone stays in memory until the next project save (4B-2 persistence audit). "Open Scene System" in the scene group and scene set inspectors opens the page; a build stopped by the group check opens it at the Debugger.
- **Scene sets fill scene groups** (ADR 0008). A group names its set and still stores its own lists, which the loader reads. Each of manager scenes, shared scenes and Reset shared scenes is either "From set" or an Override; editing a part in the group inspector makes it an override, editing it back clears it, and Revert to set ends it. Changing a set fills every group that uses it, except its overrides. New groups from Create Scene Group start on the default set.
- **Migration.** One scene set per distinct (manager scenes, shared scenes, Reset shared scenes) among groups with a main scene; each group points at its set with no overrides. A markdown report next to the settings asset lists every group before and after; groups without a main scene are listed as unused, with the files that still reference them, and never deleted.
- **Rename tracking.** Renaming or moving a scene re-saves every .asset, .prefab and .unity under Assets/ (up to 16 MB each) that holds its GUID, so stored names and paths follow; scenes open with unsaved changes are skipped with a warning. In the Editor a scene field reads its scene's live name, so Play works before any re-save. Before a build, scene groups with stale names are re-saved.
- **Scene field.** One status box per scene: In build, Addressable, Not in build, or Missing. A scene that is Not in build gets one fix button that follows where new scenes go ("Add to build" or "Make Addressable"). Right-click: Move to Addressables / Move to Build Settings. Each move removes the other entry. Core never references Addressables or this module: this module registers an `IAddressableSceneProvider` and sets `SceneStatusReader.ProjectDefaultPlace`.
- **Create Scene Group.** Right-click a scene, Create > Scene Group: a new group next to the scene, on the default scene set, its scenes placed where new scenes go.
- **Saving a group** places its scenes that are Not in build where new scenes go, with one console line each.
- **Group check.** The inspector lists problems only when there are some: no main scene, a Missing scene, a scene Not in build, the same scene listed twice (every problem stops a build when the check is on). A group with no main scene and no scene set that lists boot scenes is a boot group from before scene sets, not a problem. The Scene System page's Scene Groups Debugger lists every group.
- **Deferred work** from asset postprocessors and save hooks runs through `EditorIdleQueue` (Core), not `EditorApplication.delayCall`, which can wait until the Editor window is focused.
- **Sample graphics** (5A2). Library samples use the library's own shader, `Abstract Pixel/Samples/Sample Lit` (`Core/Samples/SampleGraphics/SampleLit.shader`), and materials made from it (Sample Default, Sample Ground). Its one pass has no LightMode tag, so the Built-in pipeline, URP and HDRP all draw it, in edit mode, Play and a player, with a simple diffuse light from a fixed direction (no scene lights needed). Unity's Default-Material and the Built-in Standard shader draw magenta outside the Built-in pipeline, so `LibrarySamplesReferenceOnlyTheLibraryTests` fails on them in any sample.

---

## Scene picker

The scene picker is a dropdown on Unity's main toolbar, just left of Play. It opens or selects any scene or scene group without the Project window.

**Show it once.** Unity hides a new toolbar element until you tick it. On Unity 6.3, right-click an empty part of the toolbar (or open its More menu at the right end) and tick Scene picker. On Unity 7, click Show the scene picker in the toolbar on the Scene System page or on the setup window's last page; it opens Unity's toolbar picker at the scene picker, and one tick shows it.

**The button** names the open scene group. With no group's main scene open it shows the active scene's name followed by "(no group)". In Play it is greyed out and still names the open group.

**The popup** has two tabs and one search bar under them.

- Scenes lists the game's scenes first, each with its group, or with its scene set for a manager or shared scene. Scenes in no group follow under their own heading, and sample scenes sit under Samples at the bottom. Unity's test runner scenes and other packages' scenes are left out.
- Scene Groups lists game groups with their main scene and scene set, then sample scene groups. A group with no main scene can be selected but not opened.
- Before you type, a Recent section shows what you opened last from the picker, and the open group or scene is highlighted.
- Rows (5A2): the name on the left, with a number before it under Recent (1 is the last used) and on the Scene Groups tab (the group's number in the Scene Groups Debugger). Right-aligned: the label button, the status button and the folder button, flat until hovered.
- The label names the scene's group, its scene set ("Level (scene set)"), "Shared by X", or "No group" (plain text, not a button); on the Scene Groups tab it names the group's scene set, and the main scene moves into the row's tooltip. A click selects what it names and shows it in the Inspector.
- The status button is the scene status colour on the Scenes tab, and the group check on the Scene Groups tab (red with the first problem and the count; yellow for an unused boot group, as the Debugger shows it). Sample scenes and groups are grey, "Sample: never goes into a build". Its tooltip says the status, then what a click does. A click never opens the row:

  | Tab | Status | Click |
  |---|---|---|
  | Scenes | In build (green) | Build Profiles at the scene list in use (the active build profile's own list when it overrides) |
  | Scenes | Addressable (blue) | Addressables Groups window |
  | Scenes | Red, with a group | Selects the group; its group check shows the fix |
  | Scenes | Red, no group or a scene set label | Build Profiles' scene list, or the Addressables Groups window when new scenes go to Addressables |
  | Scenes | Red, new scenes to Addressables with no settings or no default group | Addressables Groups window |
  | Both | Sample (grey) | Selects its sample scene group |
  | Scene Groups | Green or blue | Scene System page at the Scene Groups Debugger, scrolled to the group's row, highlighted for a moment |
  | Scene Groups | Red | Selects the group (Addressables setup problems open the Addressables Groups window) |

- The folder button selects the asset in the Project window. Right-click a row for Open, Open additively, Select scene, Select scene group and Copy path; entries that do not apply are greyed out.
- Build Profiles opens through `BuildPlayerWindow.ShowBuildPlayerWindow`; the scene list is picked by guarded reflection on Unity's window (nothing logged if Unity renames it). The Addressables Groups window opens through its menu item, `Window/Asset Management/Addressables/Groups` (`AddressablesSetup`).

**Search** filters as you type. Words are separated by spaces, case is ignored, and every word must appear in the name, the folder or, in the Scenes tab, the group name. Rows whose name holds every word come first, with the matched letters in bold.

**Keys and clicks.**

| Action | What it does |
|---|---|
| Click a row, or Enter | Opens it. A scene opens as a double-click in the Project window does; a group runs Open group. |
| Folder button, or Shift+Enter | Selects the asset in the Project window and pings it. Nothing opens. |
| Label or status button | See above; each closes the picker. |
| Right-click a row | Open, Open additively, Select scene, Select scene group, Copy path. |
| Up and Down | Move the highlight. |
| Tab | Switches between the Scenes and Scene Groups tabs. |
| Esc | Closes the picker. |

Opening asks Unity's usual save prompt first; Cancel changes nothing. The footer reads "Open with: Group, Default set": the two scene opening preferences, the same ones as Preferences > Abstract Pixel > Scenes (full names in the tooltips). The keys are in the search bar's tooltip.

**What it never does.** It never changes Build Settings and never marks a scene dirty. It finds each asset again by GUID when you click, so a scene or group renamed while the picker is open still opens, and a deleted one opens nothing and logs one warning. A pick made while Unity compiles or imports waits until Unity is idle, and survives the script reload that follows. It works with no scene system settings and with no scene groups.

**Code.** `Editor/ScenePicker/`: `ScenePickerRows`, `ScenePickerSearch`, `ScenePickerLabel`, `ScenePickerRecent`, `ScenePickerTarget`, `ScenePickerStatusRules`, `ScenePickerLabelRules` and `ScenePickerRowMenu` are plain code with tests in `ScenePickerRowsTests` and `ScenePickerStatusRulesTests`; `ScenePickerWindows` opens Build Profiles and the Addressables Groups window; `ScenePickerToolbar` is the toolbar element, `ScenePickerPopup` the popup, `ScenePickerActions` the open and select calls, `ScenePickerProject` reads the project. Unity 7-only code sits behind `#if UNITY_7000_0_OR_NEWER`; the 6.3 picker is complete without it.

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
3. **Hold** every relocatable, then ask for every missing scene of the group at once and in order (main scene, manager
   scenes, shared scenes).
4. Records the group and **Relocates** (a failed load relocates too, so objects with no position stay held), then
   `OnNewSceneGroupTransitionTo` once, `OnGameStarted` once, and **Release** when start-up ends, or when the launch
   scene hold ends if it still holds. A failed load raises neither;
   `SceneActions.HasStartUpFailed` turns true (no fallback for start-up).

**A change** (`ChangeTo`) and **Retry** (`Retry`) share one body. A `SceneChangePlan` works out which scenes to close and open.
1. Refusals first, each before anything touches the screen: no group (`NoGroupGiven`), the group asked for is the open
   asset (`AlreadyOpen`; groups are equal only to themselves), nothing open to Retry (`NothingOpenToRetry`), start-up
   running, another change running (`Busy`). Busy is set before the first `await` and stays set until Show screen ends.
2. **Hold** every relocatable. **Hide screen** through the registered `ISceneTransitionExecutor` (none registered: nothing). A Hide screen asked for
   while an effect still runs waits for it, so the screen is always covered.
3. A preload this change does not use is closed (held main scene first). Unity holds every later scene operation while a
   preloaded main scene waits. A change to the preloaded group itself runs as the preloaded switch in step 5.
4. **Loading scene**, when the group shows one (next section): it opens under the cover and Show screen uncovers it.
5. Close the old main scene and the shared scenes (all with Reset shared scenes), free memory once,
   `OnSceneGroupUnloaded`, open manager scenes, shared scenes, then the main scene (`OnMainSceneLoaded`). Loading
   progress fills. A switch to a preloaded group activates the held main scene first, then closes and opens the rest.
6. Done: record the open scenes, **Relocate**, `OnNewSceneGroupTransitionTo` once (then `OnPreloadedSceneGroupActivated` for a switch).
   Failed: record what is really open (no group counts as open), `OnSceneChangeFailed` once, then the **fallback scene
   group** (settings: "If a scene change fails, go to"; empty means the start scene group) runs steps 5 and 6 once,
   with the screen still covered and the loading scene still up. If the fallback fails too, or the failed change was
   the fallback, the screen is shown on whatever is open and an error is logged. A failed load is never tried again. A
   scene that fails to close is logged and the change carries on.
7. The loading scene leaves (minimum time, "full" animation, Hide screen, it closes).
   Relocate runs once per change: a failed change relocates on whatever is open.
8. **Show screen**, then busy clears. **Release** comes once, when the screen counts as visible (the transition profile's
   `ScreenCountsAsVisibleAt`, 0.75 by default), or when Show screen ends if it never reports; while the launch scene hold
   is on, it waits for the hold to end. The result says Done, or Failed with the scene.

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

## Relocating objects

The relocation sub-module (`Relocation/`, assembly `AbstractPixel.SceneManagement.Relocation`) moves objects that live in a
manager scene or shared scene, such as the player, to the right place in each new scene group. Words:
[GLOSSARY.md](GLOSSARY.md) "Relocating objects". Decisions: ADR 0006, 0012, 0013. Example: `Samples/05 Persistent Player
And Spawn Points`.

**Set up.** Add a `Relocatable` to the object (its label: "No label" or a relocation label asset; "New label" asks where to
save one). Add a `SpawnPoint` with the same label to each main scene; its transform is Instance 1, and "Add position" or
Shift+D in the Scene view adds more. The inspector lists the Scene view controls, lights the one in use and names the
mode. The Scene System page's Relocation section holds the Ground layers and the steepest slope for "No label".

**What happens in a scene change** (the coordinator calls the relocation broadcaster, ADR 0012):
1. **Hold**, when the change is accepted: rigid bodies turn kinematic with zero velocity, interpolation off, and
   `IRelocationHold` components on the object hear `OnHold`.
2. **Relocate**, once the new scenes are open and before `OnNewSceneGroupTransitionTo`: every relocatable is matched
   to a spawn point position in the open group's main scene by label and instance (a set instance wins, the rest take
   the next free one in Hierarchy order), written in one batch with one `Physics.SyncTransforms`, checked one frame
   later, then, with Wait for ground below, kept held until a collider of the Ground layers is under it. Trails and
   world-space particles are cleared so the jump leaves no streak. A body nested under another held body is left to
   follow its parent.
3. **Release**, when the screen counts as visible: bodies get back their own settings, `IRelocationHold.OnRelease` runs,
   then `Released` fires.

A relocatable that has no position, starts deeper than 0.25 m inside the ground, or sits under a terrain surface stays
held, and one error names it, its label, the scene group and how many positions the main scene has. Every wait counts
unscaled frames, so a paused game (time scale 0) still relocates. Labels match by their ID (the asset GUID, stamped on
import), so the copy of a label that an Addressables bundle carries matches the original.

**Events for game code** on `Relocatable`: `Relocated`, `Released`, and `IsHeld`; `IRelocationHold` for components that pause
their own behaviour. Rally Shift's `CarRelease` turns the
car's controls on at `Released`, and `BombInitializer` starts the bomb there. A game with no relocatable changes scenes
as before: the broadcaster is never registered.

**Sample scene groups** (ADR 0014): tick "Sample scene group" on a group whose scenes are only for the Editor. They open
in Play mode from their path, the Build Settings writer leaves them out, and a build that would include one stops and
names it. The page's "Sample scene groups" section lists them. A sample scene group can also name a **sample launch
scene** ("Launch scene (sample only)", shown once the box is ticked): Play started from that scene in the Editor runs the
launch scene hold with the sample scene group as the start scene group. The project's own launch scene wins when both
are open.
