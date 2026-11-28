# Rally dogfood and checks (mega run)

Project: `C:\Users\avig7\Desktop\AllUnity\ActiveProjects\ProjectRallyShift` (below: RALLY). Unity 6000.3.15f1, branch
`game-management-rework`. Read its `CLAUDE.md` and `AgentScripts/Tools/UNITY_CLI_QUICKREF.md` first.
Its Pipeline package is 0.8.0-exp.1, older than the Lab's, so command names can differ: list them with
`unity command` before using one.

## Why Rally lacked editor UI

Rally gets library code only through commits in its submodule (`RALLY/LocalPackages/AbstractPixelLibrary`). The
ticket 06 code was never committed, so agents copied files into Rally to test, then restored the clone to `89a97d7`.
Rally therefore lacks the state inspector, the state field drawer and ticket 05's two owner fixes.

**Rules:** never copy library files into Rally. Commit in the Lab library, then in Rally LIB fetch and check out the
`game-management-rework` commit (from `origin`, or `git fetch
"C:/Users/avig7/Desktop/AllUnity/ActiveProjects/Abstract_Pixel_Lab/LocalPackages/AbstractPixelLibrary"
game-management-rework`). Confirm `Modules/SaveSystem` still has `SaveableAttribute.cs` (save v1; Rally breaks on
`main`). If untracked `.meta` files block the checkout, move them to the scratchpad and compare after. **Leave Rally
on the tested commit**, commit only the pointer, and never report "works in Rally" for a state Rally is no longer in.

## Owner's files

Before touching Rally, record `git status --porcelain` and hashes of modified and untracked files. On 2026-11-28 these
were already modified: four TMP font assets, `RallyGameStateCatalog.asset` and the submodule pointer. Never revert or
commit the fonts or the catalog. Never run `revert_probe_churn.sh` without first checking it would not discard them.
Play mode can rewrite fonts, `GameManagment.prefab` and `SaveSystemConfigSO.asset`: back up first, restore after.

## Editor UI in Rally

Check each row that exists at your step, on the commit Rally is left on. Open it through its real menu, look at a still
you read, use it, and confirm Rally's assets are unchanged afterwards (edit, then Undo, or restore byte for byte).

| Id | UI | Built by | Must show and work in Rally |
|---|---|---|---|
| R1 | Project Settings > Abstract Pixel > Game Management | 05 | Five cards with Rally's states, tabs, problems, `+` and `-`, reorder, New state, Create/Move not offered wrongly |
| R2 | State inspector on Rally's own state assets (main and sub) | 06 | Switches, parents, over-each-parent table with Rally's values, input maps, long names, Problems |
| R3 | State field drawer on a Rally Requester or Reactor and on Parents | 06 | Normal field; red icon only for an unlisted state |
| R4 | Shared editor look: the Scene System settings page and setup window | 05, 07 | Unchanged apart from the new "Scene System" pill |
| R5 | Onboarding window | 07 | Opens from its menu in "run again" mode: existing menus ticked and locked; closing changes nothing; does **not** auto-open |
| R6 | Debugger settings card (journal size, show/hide action, release opt-in) | 08 | Enabled, saves, Undo works |
| R7 | Debugger overlay in Play mode | 08 | See the overlay section |
| R8 | Game State Debugger window in Play mode | 09 | Real journal from a Rally run, filter, sort, footer, click-to-navigate to Rally's states and requesters |

Do not press Finish in R5: it would create assets in Rally.

## Parity (any step that changed runtime files)

Run the scenarios in `../../Parity/README.md` with `bash AgentScripts/Probes/run_scenario.sh <name>` and compare with
`Parity/baseline/` and Session 4's intended differences. Pass rule: PLAN-OVERVIEW "Transition break scenarios".

## One-level Mono player builds (ticket 08; S4 reuses the result)

### Setup

- Backend: Rally's Standalone backend is IL2CPP. Use Mono for these builds and **restore IL2CPP in a `finally`**.
- Do not edit `EditorBuildSettings.asset`. Pass the scene list to `BuildPipeline.BuildPlayer` from a Rally probe,
  `AgentScripts/GameManagementRework/BuildOneLevelPlayer.cs`, modelled on `AgentScripts/Tools/BuildDevPlayer.cs`
  (it already has `RunMonoTo`, a dirty-scene refusal and a watchdog script).
- Scenes, in order (paths as in Rally's build settings): `SplashScreen`, `MainMenu`, `LevelSelectionScene`,
  `ManagerBootScene`, `GameBootScene`, `RallyLoadingScene`.
- The one level is the one a fresh save offers first: Stage1 "Level1" = `level3Group` (scene `level 3`). It is not in
  the build list; it is the Addressables entry `Level3`. Build Addressables content for the player and confirm it
  loads. Do not add the other levels. If a build check fails only because other levels are left out, handle it in the
  probe without changing Rally's data, and record it.
- Output: `RALLY/Builds/GmDogfood/<Dev|Release|ReleaseOptIn>/RallyShift.exe` (check `git status`: never committed).
- To drive the dev player, the Pipeline runtime needs a Development Build **and** `enableInBuilds` (Project Settings >
  Pipeline > Runtime; docs in `Library/PackageCache/com.unity.pipeline@*/Documentation~/runtime-setup.md`). It is off
  by default: turn it on for the dev build only, then restore it. It runs a local code-execution server, so it must
  be off for the release builds. If this version cannot drive the player, add a `#if DEVELOPMENT_BUILD`
  command-line probe like `AgentScripts/PerfStaging/PerformanceUtilities/DevBuildFinishEvent.cs`.
- Show/hide in a player: F8 is Editor-only, so Rally needs an action assigned in the Debugger card. Add a debug
  toggle action (keyboard F8 and a free gamepad control) to Rally's input actions and assign it. It must keep
  working in every state, including ones whose input effect disables other maps. Leave this Rally change
  **uncommitted**; list its files in the handoff.

### Safety before launching the dev player

1. Close the player after every run. Steam stays closed. Never pass `-allowSteamWrites`. Development builds block
   Steam writes (`SteamWriteGuard`); confirm no write is logged as allowed in `Player.log`.
2. The player uses the owner's real save. Copy
   `%USERPROFILE%\AppData\LocalLow\Abstract Pixel Studios\RallyShift\SaveFiles` to the session scratchpad **and** to
   `LAB\Logs\mega-07-09\save-backup\`, with hashes. Move the original aside so the run starts fresh. Afterwards
   restore it and compare hashes. Do this even if a run fails. Never overwrite an existing backup.
3. **Never launch a release build.** It is allowed to write to Steam and the real save.

### Dev build flow (real input: queued virtual gamepad and keyboard events)

Screenshots come from inside the player (`ScreenCapture` or the runtime capture command), never from Windows. Read
every one. Skipping slow setup is fine (teleport the car near the finish, shorten the bomb timer); the event itself
must come from the real trigger: the car crosses the finish collider, the bomb timer expires.

| Id | Flow | At each stop, check the overlay and the game |
|---|---|---|
| P1 | Splash to main menu | Overlay hidden by default; show and hide with the action |
| P2 | Main menu to level select to the level, through loading | Stack and effects follow the real states; no input stolen from menus |
| P3 | Driving | Time 1, cursor and maps right; overlay does not block the view's key HUD parts beyond the approved corner |
| P4 | Pause, Settings, Back, Resume (keyboard run and gamepad run) | Suspended states dimmed; works at time 0; journal lines grouped and numbered |
| P5 | A refused request | Last refusal shown with its reason |
| P6 | Game over from the bomb timer, then Retry | Overlay survives the scene change; no duplicate overlay |
| P7 | Win through the finish trigger, then win menu, then quit to main menu | Same; do not press Next Level (that level is not built) |
| P8 | Repeat P2 to P6 with the overlay shown, then hidden | Game behaves identically; mouse and gamepad menu navigation unaffected |

Afterwards: `Player.log` has no exceptions or new errors; no frame-time regression visible from per-frame allocations
by the overlay (check with a profiler sample or an allocation count while shown).

### Release checks (build only, never launch)

| Id | Build | Check (Mono assemblies can be searched) |
|---|---|---|
| B1 | Release (not development), opt-in **off** | No overlay type in `RallyShift_Data/Managed/*.dll`, no overlay UI asset in the build report |
| B2 | Release, opt-in **on** | The overlay type and its assets are present |

Search the dev build the same way first: it must find them, or the search proves nothing. After all builds: backend is
IL2CPP, `enableInBuilds` is off, the release opt-in is off, defines are as before, and `git diff` of
`ProjectSettings/` is empty.
