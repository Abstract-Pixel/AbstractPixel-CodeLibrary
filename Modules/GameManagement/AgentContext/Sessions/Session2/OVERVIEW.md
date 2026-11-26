# Session 2 overview: core state machine

`plan: gm-commercial, step 2`. Budget: **10%** of the 5-hour window, wrap reserve 2%. Kind: implement.

Session 1 used well over its 8% (see its handoff). This session needs no Play-mode recording, so 10% is plausible,
but run `budget estimate` with kind `implement` first and raise it if history says so.

## Goal

The whole of DESIGN.md's "one deep module" as plain C#, proven by EditMode tests. Rally keeps using the old system:
this session adds code and changes no existing behaviour, scene or asset.

## Tasks

| # | Task | Share | Must-have | Nice-to-have |
|---|---|---|---|---|
| 1 | Catalog and state data | 2% | State catalog asset (ordered, higher wins); new state fields for parents, effects with "Same as parent", leave on scene change; settings asset with the "no state active" option (keep last = default) and a user-chosen location | Catalog helpers the Session 6 validator can reuse |
| 2 | Machine: requests, order, parents, queue | 4% | `Request`, `Leave`, `IsActive`, `Top`, `Changed`; refusals with reasons; queue for requests made during a transition; sub-states with parents, suspend, close sub-states first | |
| 3 | Effects, delay, journal | 2.5% | Effects worked out from the stack; recording effect sink; fake clock; execution delay and skip; transition journal ring buffer | Diff of effects in the journal record |
| 4 | Static facade | 0.5% | `GameStates`, reset at SubsystemRegistration | |
| 5 | Handoff and next session | 1% | `Sessions/Session2/HANDOFF.html`, map, Session 3 OVERVIEW and PROMPT | |

## Done when

Every row of DESIGN.md's edge-case table and every DECISIONS round 3 rule has a passing EditMode test, both Editors
compile with no new warnings, the old system is untouched, and the handoff exists.

## Status (2026-11-26)

Done. Every must-have and the nice-to-have (catalog helpers) is finished; 40/40 module tests pass on Unity 6000.3 and
the machine's 36 tests pass on Unity 7 (7000.0.0a16). Committed on `game-management-rework` in both repos. See
[HANDOFF.html](HANDOFF.html).
