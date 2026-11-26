# Session 3 prompt: adapters, conditions, reactors, input source

You are an autonomous agent working in ProjectRallyShift (Unity 6000.3) on the AbstractPixelLibrary submodule's game
management module. plan: gm-commercial, step 3. Budget 10% of the 5-hour window.

## What "finished" means

Read `LocalPackages/AbstractPixelLibrary/Modules/GameManagement/AgentContext/DEFINITION-OF-DONE.md` before you start.
A task, and the session, is finished only when every point there holds. Anything cut goes in the handoff's "Not done" table.

## Read first

1. `Modules/GameManagement/AgentContext/`: `PLAN-OVERVIEW.md`, `DECISIONS.md` (source of truth), `DESIGN.md`,
   `DEFINITION-OF-DONE.md`, `Sessions/Session2/HANDOFF.html`, `Sessions/Session3/OVERVIEW.md`.
2. The module's `README.md`, `GLOSSARY.md`, `Runtime/Machine/` (Session 2's code) and `Tests/Machine/`.
3. The library's `CLAUDE.md` and Rally's `CLAUDE.md`; `AgentScripts/Tools/UNITY_CLI_QUICKREF.md`.

## Start

- First tool call: `budget start` with `plan: "gm-commercial"`, `step: 3`, `kind: "implement"`, `budgetPct: 10`,
  `wrapReservePct: 2`, phases `sink 3, input 1.5, adapters 2, reactors 1.5, migration 1, handoff 1`.
  If the scope check fires, do every task's must-haves first and spend what is left on the rest. Ask for more only if
  the must-haves themselves will not fit.
- Check `unity command editor_status` and that no other agent drives the Editor. `unity command set_autotick --enable true`.
- Branch `game-management-rework` exists in both repos, with Sessions 1 and 2 committed. Still, check `git status` in both
  and do not disturb the owner's other uncommitted files (hash them before Play-mode runs and compare after).

## What Session 2 left that matters here

- The machine is plain C# with no driver: nothing calls `Tick` or `LeaveOnSceneChange`, and the facade's machine
  applies no effects. The sink and driver are this session's first task.
- `GameStateCatalog` has validator helpers (`FindEmptySlots`, `FindDuplicates`, `FindParentsMissingFromCatalog`);
  the migration tool can use them to report problems in its dry run.
- `Changed` fires for every step, refusals included; `Applied` fires when effects reach the sink. Menus that reveal
  "on apply" (`MenuRevealTiming`) listen to `Applied`.
- Game Over: 1.4 s delay, then a 1 s ease from the **current** time scale (Rally's `CarDeathSequence` sets 0.6 first).
  The sink eases from the value at apply time. Who owns that 0.6 is Session 4's job; do not block it.
- Cursor bug the owner confirmed: after unpausing, the cursor stays visible. The new model re-applies Play's cursor
  when Pause closes, and the Unity sink must make that real. Rally keeps the old system this session, so do not change
  Rally's live flow; Session 4 checks it by hand with a mouse.

## Work

Follow the task table in `Sessions/Session3/OVERVIEW.md`. Tests first for every rule, in `Modules/GameManagement/Tests/`
(the one test assembly). The Unity sink implements `IGameEffectSink`; the driver is the only MonoBehaviour that calls
the machine each frame. The migration tool runs dry first and does not edit Rally's state assets this session.

## Check

- EditMode tests: `unity command run_tests --mode editor --filter AbstractPixel.GameManagement.Tests --filter_type assembly`.
  Recompile with `bash AgentScripts/Tools/editor_cycle.sh noplay`.
- If any old runtime file changes, run `parity_pause` and `parity_gameover` and compare (`Parity/README.md`).
- Unity 7: the Lab uses a separate library clone outside the allowed folders. Check the way Session 2 did (see its
  handoff): copy the changed files into a throwaway `Assets/_GmUnity7Check/` with non-auto-referenced asmdefs, compile,
  run the tests with `unity command test run --mode editor --filter <assembly> --filter-type assembly`, then delete it.

## Finish

As DEFINITION-OF-DONE.md lists: `Sessions/Session3/HANDOFF.html`, update `Wayfinder/map.md`, write
`Sessions/Session4/OVERVIEW.md` and `PROMPT.md` (linking DEFINITION-OF-DONE.md), `budget end`. Commit only if the
user's launch message allows it.

## Rules

- Stay inside ProjectRallyShift, `../Abstract_Pixel_Lab` and the session scratchpad; put this rule in any subagent brief.
- No game behaviour changes this session. Never touch live Steam leaderboards or stats.
- If the budget runs low: finish the must-haves, keep Rally working, and list the rest in the handoff.
