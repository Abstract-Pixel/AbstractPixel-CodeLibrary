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

- Old system deleted; Rally and `Samples/01 Minimal Game` on `GameStates`, adapters and reactors.
- `Runtime/Adapters/` holds 12 scripts; `...Request` adapters become `...Requester` (glossary, 2026-11-26). The owner flagged `WhenLeavesGoTo`, `FollowUpRule` and `RisingEdge` (see NAMING.md).

## Tasks

| # | Task | Share | Must-have | Nice-to-have |
|---|---|---|---|---|
| 0 | Delete the old system, only if Session 4 moved it here | from the shares below | As Session 4's task 6 | |
| 1 | Name and folder audit | 1% | Every public type in `Runtime/` and `Editor/` checked against NAMING.md; a table old name, what it does, new name, folder; the owner approves it in one question round before any rename | |
| 2 | Folders | 0.75% | No folder above 8 scripts; folders by part; `git mv` with `.meta` | |
| 3 | Renames | 1.25% | Approved renames done with serialized data safe (`[MovedFrom]`, `[FormerlySerializedAs]`, `AddComponentMenu` paths updated); Rally scenes and prefabs grepped before and after; tests and the sample renamed to match | |
| 4 | Readability pass | 1% | Run the `simplify` skill on the module; long methods split, dead code and stale comments gone, error messages say what, where and how to fix; no behaviour change | `codebase-design` deepening notes for Session 9 |
| 5 | Docs, handoff and next session | 1% | GLOSSARY, README, DESIGN mapping, NAMING.md and AgentContext paths use the new names; handoff; Session 5's OVERVIEW and PROMPT | |

## Done when

DEFINITION-OF-DONE.md holds and PROMPT.md's "Done and verified (this session)" section holds.
