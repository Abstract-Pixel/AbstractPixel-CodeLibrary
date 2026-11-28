# S1 handoff (ticket 07, onboarding window build)

## Result
The setup window works: 7 pages for a new project, 4 for a run later. It makes settings, state list, states, copied or
own menus; nothing existing is written over; a failed run deletes what it made. Package settings are read-only; project
settings win. Module tests 178/178 in the Lab. Rally (6.3) compiles clean and shows R1, R4 (pill), R5.

## Verification
No independent verifier, skipped by the owner; no separate code review. Verified commit: none (builder's own checks only).
Library: gmr `68c65b1`, main `45d6000`. Evidence: `stills/` (all read), `tools/` (scripts used), `PROGRESS.md`.

## Checks run
| Check | Result | Input |
|---|---|---|
| Module EditMode tests | 178/178 (new: plan, builder, menu buttons, package guard) | tests |
| Trials in the Lab with no project settings: all copy, all own no prefab, mix with own prefabs and long names, only Play, cancel at Ready, run again (add Game Win; add Pause to existing Settings) | pass; each removed after, Preloaded Assets restored (`git diff ProjectSettings` empty) | window driven by script (SendEvent clicks, `Set` for text and fields) |
| Settings page and inspector after full creation | "6 states, no problems"; no Problems section | script-opened windows |
| Play on the all-copy result, each device alone | mouse: Start, Pause>Settings>Back>Resume, Game Over>Restart, Win>Main Menu, Pause>Main Menu, Game Over>Main Menu all work; keyboard: Esc pause, Backspace back, arrows+Enter work; gamepad: auto-select, Start pause, D-pad, South, East work | queued Mouse, Keyboard, Gamepad device events, Game view in the Editor; pause and lose came from game code (`GameStates.Request`) |
| Auto-open | opens once per session (Editor foregrounded), not again after a reload, not when "Don't show again" is on, opens again after the Setup card switch | real reloads; batch mode and tests only by the rule test |
| Long folder and prefab names, 330 / 620 / 1000 px | read; fixes made (wrap, full path line, radio and NOTE wrap) | stills |
| Rally R1, R4, R5 on `68c65b1` | R1 page normal; R4 setup window shows the "Scene System" pill; R5 opens at step 1 of 4 with all four menus locked, closed without Set up, Rally files unchanged | script-opened; Finish never pressed |
Real OS mouse and keyboard typing were not used.

## What changed
- Library `Modules/GameManagement/Editor/Onboarding/` (Build/, Window/, Window/Pages/ with UXML and USS), `Runtime/MenuPrefabs/Settings Menu.prefab`.
- Settings page: package warning, read-only body, "Set up states...", setup-window switch; state list groups a sub-state over a sub-state under the main state.
- `GameManagementSettingsAssets.Find` prefers Assets; `FindProjectSettings`, `IsProjectPath`, `IsPackageAsset`; `StateListEdits` refuses package catalogs.
- Core: `SystemPill`, `SelectOnKeyboardNavigation`; `TestRunDetector` moved to Core.Editor; scene setup window shows a Scene System pill.
- Developer effect: running the window gives a working menu set; copied menus use `AutoSpatialNavigation` (gamepad) and keyboard first-button selection.
- Lab only, uncommitted: removed the onboarding prototype windows and their state class and menu lines (USS/UXML kept; `GameManagementOnboardingPillIcon.cs` kept, the other prototype code uses it).

## Repo and Editor state
| Repo | Branch | HEAD | Uncommitted |
|---|---|---|---|
| LIB | main | `45d6000` (gmr `68c65b1`), pushed both | `DebugDataBase.asset`, Sample 01 catalog (never committed), mega-07-09 files (chief's) |
| LAB | learn | pointer commit after this handoff | prototype removals; `Assets/GameManagement/` (owner's, `New State.asset` untouched) |
| RALLY LIB | game-management-rework | `68c65b1` | untracked `.meta` files that were already there |
| RALLY | game-management-rework | pointer commit | four fonts, `RallyGameStateCatalog.asset` (owner's), as at the start |
Temporary assets (`Assets/ZZ_Trial`), settings and windows removed or restored in both Editors. Rally `.slnx` reverted. A trial once ran into the owner's `Assets/GameManagement` (default folder); only my files were deleted.

## Decisions made without you
- Package settings, Settings menu prefab (title and Back), one Settings sub-state over every ticking menu, unique names reported on Ready, library prefab names kept: as the ticket said.
- A run later shows 4 pages (no Welcome, folder or Input); existing menus ticked, locked, hidden from the paths page.
- Existing menus are found by loose name (spaces, capitals, trailing "State"/"StateSO"; Start counts as Main Menu), because Rally's are `StartStateSO`, `PauseStateSO`.
- The Settings screen gets its own card (Copy / Use my own) on "How to set up each menu".
- Copied menus: root switched on (library prefabs are saved off); parts moved under a `Content` child that the reactor shows; buttons wired; Restart/Next Scene removed from Pause; Main Menu button removed if no Main Menu state; Game Over/Win buttons leave their state first (they outrank Play and Main Menu).
- Own prefab: reactor added to the root, Content = first child; a prefab that already has a reactor, or sits outside Assets, is left alone and reported.
- Added `SelectOnKeyboardNavigation` (Core): arrows alone selected nothing on Pause.
- Pause key and Back key are the developer's to add (Input Toggle / Back Requester); copies have buttons only.
- Defaults: Main Menu time 1, Pause/Over/Win time 0, Game Over delay and ease 0.5, Play locked cursor with HUD; Settings follows its parent.
- "Learn more" opens the module read-me.

## Not done / not verified
No verifier and no code review (owner). Not checked, carried to ticket 11: keyboard arrows on Main Menu pick the second button first (library `AutoSpatialNavigation`); real OS mouse and typing in the window; Choose... folder dialog and the object picker; a copied menu in a real scene with real input actions; Unity 6.7; module tests in 6.3 (compile only); batch-mode auto-open (rule test only); every own-prefab combination in the UI; "Set up" object field can still pick a package asset; automated Play-mode test of the menus (a test-runner version hung, so none is committed).

## Traps found
- No pop-ups.
- `EditorApplication.delayCall` waits for focus: an Editor in the background never auto-opens the window.
- UnityEvent calls added by script run only in Play mode; EditMode tests must set them to EditorAndRuntime.
- Library menu prefabs are saved with the root switched off; a reactor on the root then never runs.
- Pixel probes in Rally need `focus_unity_project.ps1`; otherwise another app can cover the still (one still showed the chief's window and was deleted).
- A scripted click on Ready's Next in a new project builds into `Assets/GameManagement`.
- Bash tool fails on some quote patterns in long heredocs; write files with the Write tool.

## Next session
S2 (ticket 08, debugger overlay build) per `Wayfinder/map.md` and MEGA-TICKET order. Start from LIB `main` `45d6000`; Rally library stays on `68c65b1`. Do not redo the setup window, its tests or any check above. Read ticket 08 "From ticket 07". Lab has uncommitted prototype removals that are the owner's to keep. Helper scripts: `Sessions/07-onboarding-build/tools/`.
