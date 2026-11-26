# Session 4B overview: Naming, folders and core cleanup

`plan: gm-commercial, step 4` (second part of Session 4; runs as a fresh session after it).
Budget: **5%** of the 5-hour window (Session 4's 18% total minus the 13% of its first part), wrap reserve 1%. Kind: refactor.
"Finished" in general: [../../DEFINITION-OF-DONE.md](../../DEFINITION-OF-DONE.md). This session's own definition is in
PROMPT.md's "Done and verified (this session)".

## Goal

Make the module easy to read and search before the UI sessions build on it: every name passes the read-alone test in
[../../NAMING.md](../../NAMING.md), no folder holds more than 8 scripts, and the code reads plainly. **No behaviour
change.**

## Why here

After Session 4 the old system is deleted, so only the new code is renamed, and Rally plus the sample game run on it, so
their tests catch any break. Before Session 5, so the UI grill and Sessions 6-8 name things on screen and in code with
the final words.

## Starting point (Session 4 updates this)

- **Session 4 is done (2026-11-26):** the old system, the old `StateSO` fields and the migration tool are deleted; Rally and `Samples/01 Minimal Game` run on `GameStates`, adapters and reactors. Module EditMode tests: 77. `GameCursorController` stays (now fed only by the effect sink; `UseMachineCursor` / `MachineCursor` are rename candidates). Task 0 below does not apply. The owner's hand cursor check passed.
- Already built: Sample 01 (keyboard, controller and UI paths tested with real input; its baseline is `Samples/01 Minimal Game/Parity/baseline.json`); input maps by `InputActionReference`; `BackRequest.alsoLeave` and `wentBack`; menus hide while covered.
- **Nothing is pushed** (Session 4 run 4, owner's instruction). Library work is on `game-management-rework` in Rally's submodule. The Lab's library `main` merges it from that local folder.
- Last checks: Rally compiles clean; EditMode 77/77 in Rally and the Lab; sample flow test 1/1 in the Lab (not rerun in Rally after the deletion); 9/9 parity scenarios pass with KeepLast (SetToOne last ran before the deletion; parity JSON not compared by hand after it).
- `FLOWS.md` still describes the old system.
- Budget: the 5% above was set before Session 4 ran over. Step 4 stands at 54.9% against 18% (before the last doc commits), so the owner should confirm 4B's budget.
- `Runtime/Adapters/` holds 12 scripts; `...Request` adapters become `...Requester` (glossary, 2026-11-26). The owner flagged `WhenLeavesGoTo`, `FollowUpRule` and `RisingEdge` (see NAMING.md).

## Tasks

| # | Task | Share | Must-have | Nice-to-have |
|---|---|---|---|---|
| 1 | Name and folder audit | 1% | Every public type in `Runtime/` and `Editor/` checked against NAMING.md; a table old name, what it does, new name, folder; the owner approves it in one question round before any rename | |
| 2 | Folders | 0.75% | No folder above 8 scripts; folders by part; `git mv` with `.meta` | |
| 3 | Renames | 1.25% | Approved renames done with serialized data safe (`[MovedFrom]`, `[FormerlySerializedAs]`, `AddComponentMenu` paths updated); Rally scenes and prefabs grepped before and after; tests and the sample renamed to match | |
| 4 | Readability pass | 1% | Run the `simplify` skill on the module; long methods split, dead code and stale comments gone, error messages say what, where and how to fix; no behaviour change | `codebase-design` deepening notes for Session 9 |
| 5 | Docs, handoff and next session | 1% | GLOSSARY, README, DESIGN mapping, NAMING.md and AgentContext paths use the new names; handoff; Session 5's OVERVIEW and PROMPT | |

## Done when

DEFINITION-OF-DONE.md holds and PROMPT.md's "Done and verified (this session)" section holds.
