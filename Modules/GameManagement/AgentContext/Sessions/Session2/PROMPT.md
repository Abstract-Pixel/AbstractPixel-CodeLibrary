# Session 2 prompt: core state machine

You are an autonomous agent working in ProjectRallyShift (Unity 6000.3) on the AbstractPixelLibrary submodule's game
management module. plan: gm-commercial, step 2. Budget 10% of the 5-hour window.

## Read first

1. `LocalPackages/AbstractPixelLibrary/Modules/GameManagement/AgentContext/`: `PLAN-OVERVIEW.md`, `DECISIONS.md`
   (source of truth), `DESIGN.md`, `Sessions/Session1/HANDOFF.html`, `Sessions/Session2/OVERVIEW.md`.
   In `FLOWS.md` read the flow table and the findings.
2. The module's `README.md` and `GLOSSARY.md`. New names come from the glossary.
3. The library's `CLAUDE.md` and Rally's `CLAUDE.md`; `AgentScripts/Tools/UNITY_CLI_QUICKREF.md`.

## Start

- First tool call: `budget start` with `plan: "gm-commercial"`, `step: 2`, `kind: "implement"`, `budgetPct: 10`,
  `wrapReservePct: 2`, phases `catalog 2, machine 4, effects 2.5, facade 0.5, handoff 1`.
- Check `unity command editor_status` and that no other agent drives the Editor. `unity command set_autotick --enable true`.
- Branch `game-management-rework` exists in both repos. Session 1 may have left its work uncommitted: check
  `git status` in both and do not disturb the owner's other uncommitted files.

## What Session 1 learned that matters here

- Rally today has **no state active after unpause** (no trigger reactivates Play). The new machine must treat Pause as
  a sub-state of Play, so Play stays active and suspended.
- Scene changes leave a one-frame "none" between the old state leaving and Play entering. With keep-last as the
  default this is invisible. Keep that property.
- Game Over: in control at once, effects after 1.4 s unscaled, then a 1 s eased time change. `CarDeathSequence` sets
  time to 0.6 itself when Game Over enters. The effect model needs an ease that starts from the **current** value at
  apply time. How Rally's 0.6 fits "one owner of time" is Session 3/4's job; do not block it.
- A scene change during the delay must cancel the pending apply. Today time then stays at 0.6 through the change.
- Menus can show either on enter or on apply (`MenuRevealTiming`), so `Changed` and "applied" are separate signals.
- Today the order for a restore is registered, applied, restored. The new design has no restore event for parents.

## Work

Build under `Modules/GameManagement/Runtime/Machine/` (plain C#, no MonoBehaviour in the machine) with its tests in
`Modules/GameManagement/Tests/`, the one test assembly. Tests first for every rule. Use the interface and seams in
DESIGN.md: daily calls `Request`, `Leave`, `IsActive`, `Top` and one `Changed` event with a transition record; seams
for the effect sink and the clock (a recording sink and a fake clock in tests). The Unity sink, input source,
conditions and reactors are Session 3.

Decide how the new state data coexists with `StateSO`: the old components must keep working until Session 4, and the
asset migration is Session 3. Prefer adding fields or a new type over changing the meaning of existing fields, and
write the choice down.

### DECISIONS round 3 rules, each with tests

1. Pause is a sub-state of Play; closing Pause returns to Play. Settings is one sub-state with two parents.
2. Execution delay: the state is in control during its delay (other requests refused); time, cursor, HUD and input
   change when the delay ends. Skip-delay applies at once.
3. Closing a parent closes its sub-states first, top down, each step in the journal.
4. No state active: effects keep their last values by default; "set time to 1" is an option in the settings asset.
5. The settings asset has a default location and the user can choose where it lives; it must load in builds wherever
   it is. Copy the scene system's loading method if it does not depend on location, otherwise use Preloaded Assets.
   (The settings page UI is Session 6.)

Also every row of DESIGN.md's edge-case table: order refusals with a reason, equal or higher main state replaces,
sub-state refused unless a parent is on top, queued requests processed in order, repeat requests ignored silently,
leave-on-scene-change.

## Check

- Run the EditMode tests with the unity-pipeline loop. Recompile with `bash AgentScripts/Tools/editor_cycle.sh noplay`.
- Unity 7: `../Abstract_Pixel_Lab` uses the library at `ExternalCodeAssets/AbstractPixel-CodeLibrary`, **not** this
  submodule, and that folder is outside the allowed folders. See Session 1's handoff for what was done instead.
- Rally behaviour: nothing it uses changes. If you edit any old runtime file, run `parity_pause` and
  `parity_gameover` and compare.

## Finish

Write `Sessions/Session2/HANDOFF.html`, update `Wayfinder/map.md`, write `Sessions/Session3/OVERVIEW.md` and
`PROMPT.md` in this format ("Adapters, conditions, reactors, input source", 10%). `budget end`. Commit only if the
user's launch message allows it.

## Rules

- Stay inside ProjectRallyShift, `../Abstract_Pixel_Lab` and the session scratchpad; put this rule in any subagent brief.
- No game behaviour changes this session. Never touch live Steam leaderboards or stats.
- If the budget runs low: finish the must-haves, keep Rally working, and list the rest in the handoff.
