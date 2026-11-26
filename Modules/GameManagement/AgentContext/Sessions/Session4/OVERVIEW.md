# Session 4 overview: Sample game, Rally migration, parity gate, delete old system

`plan: gm-commercial, step 4`. Budget: **13%** of the 5-hour window, wrap reserve 2%. Session 4 has 18% in total, shared with its second part,
[Session 4B](../Session4B/OVERVIEW.md) (5%), which runs as a fresh session afterwards. Kind: implement.
"Finished" in general: [../../DEFINITION-OF-DONE.md](../../DEFINITION-OF-DONE.md). This session's own definition is in
PROMPT.md's "Done and verified (this session)".

## Goal

Build a small sample game copying Rally's flows that proves parity in both Editors, move Rally onto the new machine, pass the parity and
transition-break gate, then delete the old system.

## The sample game

A much smaller copy of Rally's flows, so parity can be checked anywhere, including the Unity 7 Lab, and so future
projects have a reference setup. A later onboarding flow may copy its menus as a template, so keep them clean and
self-explanatory.

- **Where:** `Modules/GameManagement/Samples/01 Minimal Game/`, laid out like the scene system's samples (own assembly
  with the `UNITY_EDITOR` constraint, autoReferenced off, plus a tests assembly; scene loaded with `EditorSceneManager`;
  one-page HTML README). Never in a build.
- **Flows (Rally's, smaller):** Main Menu, Play (a counter on scaled time), Pause (Esc and a button), Game Over and
  Game Win (Retry reloads the scene, Quit returns to Main Menu), and an **empty Settings menu** (a title and Back only).
  Main Menu and Pause each open their own Settings sub-state, as in Rally: two states on the old system, then the one
  merged Settings state with parents Main Menu and Pause after migration (DECISIONS.md). Graphics-free uGUI; at most
  two small scripts of its own, the rest is the module's components.
- **How parity is checked:** a Play-mode flow test walks Main Menu, Settings and back, Play, Pause, Settings and back,
  unpause, Game Over, Retry, Game Win, Quit, and at each step records the top state, `Time.timeScale` (after the
  ease), cursor lock and visibility, and which menus are shown. Built first on the **old** components, it writes
  `Samples/01 Minimal Game/Parity/baseline.json`. After the migration tool runs on the sample and the scene is rewired
  to the new requesters and reactors, the same test must match the baseline (intended differences listed, as for Rally).
  Retry also shows a scene change leaving the states marked to leave (the driver's hook has no unit test).
- **Both Editors:** the baseline and the migrated run happen in Rally (6.3) **and** in the Lab (7), with full tests
  there (PLAN-OVERVIEW.md, "Both Editors"): commit and push the library, move the Lab's submodule to that commit, run
  the module's EditMode tests and the flow test in the Lab Editor.
- The sample is also the first test of the migration tool on a project that is not Rally.

## Starting point (from Session 3)

- `GameStates` builds its machine through `GameStateDriver.CreateMachine`: Unity sink (time eased from its value at apply
  time, cursor via `GameCursorController.UseMachineCursor`, HUD via `GameHud`, input via `IInputSource`), driver ticks
  and calls `LeaveOnSceneChange` on `activeSceneChanged`.
- Adapters in `Runtime/Adapters/`, state watch and reactors in `Runtime/Reactors/`, migration tool in `Editor/Migration/`
  (menu: Tools > Abstract Pixel > Game Management > Migrate State Assets).
- Rally still on the old system; nothing calls `GameStates` yet.
- Session 3 also finished: ease curve on `StateEffectSettings`, input map dropdown (`Editor/Inspector/`; replaced in task 2), a
  `GameStateDriver.Step` test, and a dry run plus Apply of the migration on **copies** of Rally's 7 state assets.
- **Session 3's work is not committed.** The library working tree on `game-management-rework` holds it (renames in
  `Runtime/Machine/`, new `Runtime/Adapters/`, `Reactors/`, `Unity/`, `Editor/Migration/`, `Editor/Inspector/`, the
  AgentContext docs). Task 0 commits it.

## Tasks

| # | Task | Share | Must-have | Nice-to-have |
|---|---|---|---|---|
| 0 | Commit Session 3's work | 0.2% | Session 3's files committed on `game-management-rework` (ID `S3`) and pushed; owner's unrelated changes left alone | |
| 1 | Sample game on the old system | 3% | Flows, empty Settings menu, flow test and `baseline.json`, recorded in Rally and in the Lab (full tests) | README polish |
| 2 | Migrate Rally's state assets | 1% | First replace string input map IDs with `InputActionReference`s (field, `IInputSource`, effects, migration tool, tests; the ID dropdown and attribute go), per DECISIONS.md 2026-11-26. Then: (Copies already dry-run in Session 3's extra pass: 1 tie, 2 Settings sub-states without parents.) Dry run on Rally, fix reported problems (Pause and Settings parents, merged Settings state), apply with Undo | |
| 3 | Switch Rally's scripts and probes to `GameStates` and the new adapters/reactors | 3% | Every old condition, menu, HUD, audio and cursor use replaced; Game Over 0.6 owner decided | |
| 4 | Migrate the sample | 1.3% | Migration tool run on the sample, scene rewired, flow test matches the baseline in Rally and the Lab | |
| 5 | Parity and transition-break gate | 2.5% | All parity scenarios and the 9 break scenarios pass (both no-state options); cursor after unpause checked by hand with a mouse | Compare script |
| 6 | Delete the old system | 1% | Old registries, conditions, components removed with GUID and scene/prefab grep first | |
| 7 | Handoff and next session | 1% | DEFINITION-OF-DONE.md session list; next is **4B** (prompt written; update its starting point). If the budget ran out after the gate, task 6 moves to 4B | |

## Done when

DEFINITION-OF-DONE.md holds and PROMPT.md's "Done and verified (this session)" section holds. New and touched code keeps
one type per script and passes [../../NAMING.md](../../NAMING.md); the sample's own scripts too.
