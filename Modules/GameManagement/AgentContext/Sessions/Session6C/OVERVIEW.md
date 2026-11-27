# Session 6C overview: Play-mode watchdog

`plan: gm-commercial, step 6C`. Product code, no new UI.

## What 6C covers

A Play-mode watchdog that warns when the game is not in the state the machine wants:

- The applied effects differ from the wanted ones, for example time is 0 but the top state wants 1
  (`../../DESIGN.md`, "Play-mode watchdog").
- No state is active and time stays 0 for more than a few seconds outside a scene change
  (`../../DECISIONS.md`, "No state active").

A warning says what is wrong, where and how to fix it, with the `[Game management]` prefix. It warns; it changes
nothing.

## Not in 6C

The state inspector, the state field drawer (6B-build) and the Project Settings page (6A-build).

## Do not lose: long names in the state inspector

6C builds no inspector. It must still pass the requirement on. **The real UI must accommodate long names:** everywhere
in the "What this state does over each parent" view and every name shown in it, including UI input action map names,
parent state names (including really long ones) and long text in general. Rules: `../../UI-STYLE.md`, "Long names
(required in the build)". Also pass on that **the Input comparison is still sample data and does not work yet**
(`../Session6B/HANDOFF.md` §2).

## What to read

1. `../../PLAN-OVERVIEW.md` (rules for every session) and `../../DEFINITION-OF-DONE.md`.
2. `../Session6B/HANDOFF.md`, all of it.
3. `../../DESIGN.md` (watchdog) and `../../DECISIONS.md` (guards in three layers; no state active).
4. Before code: `Runtime/Machine/GameStateMachine.cs`, `Runtime/Machine/Effects/`, `Runtime/Unity/Sink/`,
   `Runtime/Unity/Driver/GameStateDriver.cs`.

## Settle first

1. **Open checks from Session 4B:** Rally EditMode tests and parity scenarios, and a clean rerun of the Lab sample's
   PlayMode flow test. Clear them before changing product code.
2. **Branch.** The library rule is: commit game management code on `game-management-rework` (Rally tracks it, on save
   system v1), then merge into `main`; never merge `main` into it. But the Session 5, 6A and 6B documents were
   committed on `main` only, and the Lab's checkout is on `main`. Tell the owner and agree on the branch before the
   first commit. Check `git branch --show-current` before every library commit.
3. **Push.** PLAN-OVERVIEW says push per task; the 6A and 6B prompts said never push. Don't push unless the owner says so.
4. **Numbers:** how long "a few seconds" is, how close two time scales count as equal, and how to treat running eases,
   execution delays and scripts that set time on purpose. Propose them and get the owner's OK. A watchdog that warns
   during normal play is worse than none.

## Done when

- Both warnings work in Play mode and stay silent through the sample's normal flows.
- Every check in the prompt's "Done and verified" section passes, or the handoff says which did not and why.
- The handoff and the next session's OVERVIEW and PROMPT carry the long-name requirement and the Input note.
