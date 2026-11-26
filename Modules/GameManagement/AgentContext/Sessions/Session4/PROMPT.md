# Session 4 prompt: Sample game, Rally migration, parity gate, delete old system

You are an autonomous agent working in ProjectRallyShift (Unity 6000.3) on the AbstractPixelLibrary submodule's game
management module. plan: gm-commercial, step 4. Budget 13% of the 5-hour window (Session 4 has 18%; 5% is for its second
part, Session 4B).

## What "finished" means

`LocalPackages/AbstractPixelLibrary/Modules/GameManagement/AgentContext/DEFINITION-OF-DONE.md` is the general
guideline; read it first. This session is finished only when it holds **and** the section below holds. Anything cut goes
in the handoff's "Not done" table.

## Done and verified (this session)

- **Session 3 committed:** its files are committed on `game-management-rework` and pushed (hashes in the handoff).
- **Sample game:** `Modules/GameManagement/Samples/01 Minimal Game/` exists as described in OVERVIEW.md (Main Menu,
  Play, Pause, Game Over, Game Win, empty Settings opened from Main Menu and from Pause). Its PlayMode
  flow test recorded `Parity/baseline.json` on the old components, and after migration it matches that baseline in Rally
  (Unity 6.3) **and** the Lab (Unity 7, its library submodule moved to the pushed commit), with intended differences
  listed. Pass counts for both Editors in the handoff.
- **Input maps by action reference:** `StateEffectSettings` lists `InputActionReference`s; `IInputSource`, the effects
  and the migration tool take no string map IDs; a grep for `InputMapId` finds nothing; tests cover turning maps on by
  reference. No new string configuration anywhere (DECISIONS.md, 2026-11-26).
- **Rally's assets migrated:** the migration dry run on Rally's state assets is logged in the handoff; every reported
  problem is fixed (Pause's parent is Play, one merged Settings state with parents MainMenu and Pause); Apply ran with Undo.
  Already done in Session 3's extra pass: a dry run and Apply on copies of the 7 state assets (copies deleted). It reported
  the PauseSettings/StartSettings priority-3 tie and both Settings sub-states without parents; expect the same on the originals.
- **Driver in Play mode:** `GameStateDriver.Step` is unit-tested (`Tests/Unity/GameStateDriverTests`); its scene-change hook
  is not. The sample's Play-mode flow test must show a scene change leaving the states marked to leave.
- **Rally on the new system:** a grep of Rally's `Assets/` and `AgentScripts/` finds no use of `GameStateRegistry`,
  `StateConditionRegistry`, `BaseCondition` or the old menu/UI toggle/audio trigger; scenes use `EnterOnSceneStart`,
  `StateMenuReactor`, `HudToggleReactor`, request adapters and `StateRequester`.
- **Gate passed:** every parity scenario and the 9 transition-break scenarios (PLAN-OVERVIEW.md) run through
  `run_scenario.sh` and pass, with both "no state active" options; differences from `Parity/baseline/` are frame-rate
  noise or listed as intended (merged Settings, parents suspended). Results with numbers in the handoff.
- **Cursor bug fixed:** after unpausing with a mouse, the cursor is hidden and locked (checked by hand in Play mode;
  a still in the handoff).
- **Old system deleted:** the old files are gone after a GUID grep of scenes and prefabs; Rally compiles clean.
- **Checks:** full `AbstractPixel.GameManagement.Tests` passes; `editor_cycle.sh noplay` clean; in the Lab (Unity 7) the full
  module EditMode tests and the sample's flow test pass; owner's file hashes match.
- Every new or touched script keeps one type per script, and new names pass `AgentContext/NAMING.md`.

## Read first

`AgentContext/PLAN-OVERVIEW.md` (rules), `DEFINITION-OF-DONE.md`, `Sessions/Session3/HANDOFF.html`, this session's
`OVERVIEW.md`, the library's and Rally's `CLAUDE.md`. Nothing else yet: the rest is read stage by stage below.

## Before each stage, read this first

Read the named documents **before** starting the stage, so you follow decisions already made instead of making your
own. If something is not covered, stop and ask the owner rather than deciding it.

| Stage | Read first |
|---|---|
| 0 commit | PLAN-OVERVIEW.md "Commits by default"; `git status` of the library and Rally |
| 1 sample | FLOWS.md (Rally's flows to copy), DECISIONS.md (Settings merge, parents, Pause), the scene system's `Samples/` layout and `README.md`, `Parity/README.md`, NAMING.md, GLOSSARY.md, PLAN-OVERVIEW.md "Both Editors" |
| 2 assets | DECISIONS.md "2026-11-26" (no strings, input maps by action reference), `Runtime/Unity/Input/`, `Editor/Inspector/`, `Editor/Migration/` code, DECISIONS.md (Pause's parent, merged Settings), Session 3 handoff migration rows |
| 3 switch | DESIGN.md old-to-new mapping, FLOWS.md, `Wayfinder/issues/03-rally-helper-audit.md`, the module's `README.md`, `Runtime/Unity/`, `Runtime/Adapters/`, `Runtime/Reactors/`, `AgentScripts/Tools/UNITY_CLI_QUICKREF.md` |
| 4 sample-migrate | The stage 2 and 3 documents; `Samples/01 Minimal Game/Parity/baseline.json` |
| 5 gate | PLAN-OVERVIEW.md transition break scenarios, `Parity/README.md`, `Parity/baseline/` |
| 6 delete | DESIGN.md cut list; PLAN-OVERVIEW.md "Serialized data survives" |
| 7 handoff | DEFINITION-OF-DONE.md "A session is finished", `Sessions/Session4B/OVERVIEW.md` and `PROMPT.md` |

## Start

- First tool call: `budget start` with `plan: "gm-commercial"`, `step: 4`, `kind: "implement"`, `budgetPct: 13`,
  `wrapReservePct: 2`, phases `commit 0.2, sample 3, assets 1, switch 3, sample-migrate 1.3, gate 2.5, delete 1, handoff 1`. Build the
  sample first: the old components must still exist. If the scope check fires, do the
  must-haves first and spend what is left on the rest.
- Check `unity command editor_status`, `set_autotick --enable true`, `git status` in both repos; hash the owner's
  uncommitted files before Play-mode runs.

## Check

- Tests: `unity command run_tests --mode editor --filter AbstractPixel.GameManagement.Tests --filter_type assembly`.
- Parity: `bash AgentScripts/Probes/run_scenario.sh <name>`, compare as in `Parity/README.md`. `parity_pause`'s save
  isolation End step reported a failure in Session 3 while the run passed; look at it first.
- Already done, do not redo: the ease curve (`StateEffectSettings.TimeScaleEaseCurve`, empty = straight line; the migration
  copies each old state's curve). The input map dropdown from Session 3 is replaced in task 2 (action references).
- Sample flow test: `unity command run_tests --mode play --filter <sample tests assembly> --filter_type assembly`, in
  Rally and in the Lab.
- Unity 7: push the library, move `../Abstract_Pixel_Lab/LocalPackages/AbstractPixelLibrary` to that commit (keep the
  owner's uncommitted `DebugDataBase.asset` change; in the Lab commit only the submodule pointer, by path; the Lab
  uses the library as a local package, so nothing else is needed), then run the module's EditMode tests and
  the sample's flow test in the Lab Editor. No throwaway copies, no compile-only check.

## Finish

`Sessions/Session4/HANDOFF.html`, update `Wayfinder/map.md`, update `Sessions/Session4B/OVERVIEW.md` and `PROMPT.md`
(starting point from what was built; they already link DEFINITION-OF-DONE.md and have their own "Done and verified" section), `budget end`. Commit and push by default (PLAN-OVERVIEW.md);
the handoff lists the commits.

## Rules

- Stay inside ProjectRallyShift, `../Abstract_Pixel_Lab` and the session scratchpad; put this rule in any subagent brief.
- One type per script: each class, struct, enum or interface in its own file, in folders by part; at most one extra
  small type when it belongs to the main one. Never dump several types in one script (the old `Effects.cs` is the
  example to avoid). New names pass the read-alone test in `NAMING.md`; at most 8 scripts per folder.
- Rally must behave as on Steam today, or better. Never touch live Steam leaderboards or stats.
- If the budget runs low: finish the must-haves, keep Rally working, and list the rest in the handoff. If it runs out
  after the gate, the old-system deletion moves to Session 4B.
- **No strings configure anything** (DECISIONS.md, 2026-11-26). A developer or agent never types a string to set
  something up: no string IDs, names or keys in inspector fields, settings, or the public API's setup calls. Use
  dragged asset references (states, `InputActionReference`), typed values or enums instead. Strings stay only for
  labels, logs and messages (for example a transition's requester name).
