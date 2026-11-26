# Session 5 overview: UI design grill

`plan: gm-commercial, step 5`. Budget: **4%** of the 5-hour window (PLAN-OVERVIEW.md). Kind: grill.
"Finished" in general: [../../DEFINITION-OF-DONE.md](../../DEFINITION-OF-DONE.md). This session's own definition is
in PROMPT.md's "Done and verified (this session)".

## Goal

Resolve [Wayfinder/issues/02-ui-design-grill.md](../../Wayfinder/issues/02-ui-design-grill.md): how the
Project Settings > Abstract Pixel > Game Management page, the state inspector's problem list, the onboarding
window's pages and the runtime debugger overlay should look and behave, in the scene system's visual language.
This session is the **precursor** for Sessions 6 (settings page, validator UI, watchdog), 7 (onboarding window and
menus) and 8 (runtime debugger): none of them starts until this session has written their own detailed `OVERVIEW.md`
and `PROMPT.md`.

## Why here

Session 4B just finished naming and folders, so the UI grill names things on screen with the module's final words
(`Requester`, `Reactor`, state, sub-state, parent, problem) instead of placeholders. Before Sessions 6-8, so they
build from approved prototypes rather than guessing the look.

## Starting point (Session 4B's end state)

- Module is on `GameStates`, `Runtime/Requesters/` (+ `Requesters/Shared/`), `Runtime/Machine/`, `Runtime/Reactors/`,
  `Runtime/Unity/`; every public type passes NAMING.md's read-alone test. See
  [Sessions/Session4B/HANDOFF.html](../Session4B/HANDOFF.html) for the full rename table and what is not yet
  verified.
- **Before touching anything else**, confirm Session 4B's two open checks: Rally's own EditMode tests and the
  transition-break parity scenarios (its Editor was stuck on a dialog that needs one manual click), and a clean
  rerun of the Lab's sample PlayMode flow test (failed only on cursor-lock state, suspected environment flakiness,
  not confirmed). A grill session does not touch runtime code, so this does not block starting the grill itself, but
  it must be clear before Session 6 starts changing code.
- `GameManagementSettings` (the one per-project asset), `StateSO` (state, with `Parents` and `Effects`),
  `GameStateCatalog` (ordered list) are the data the Settings page edits. `GameStateCatalog.FindEmptySlots`,
  `FindDuplicates`, `FindParentsMissingFromCatalog` already exist as the validator's helpers (Session 2/6 prep) —
  design the problem list around what they already report, not a new shape.
- Reference for visual language: the scene system's `SceneGroupsDebugger` (stills; DECISIONS.md round 2 Q4 says the
  Settings page should feel unified with it) and its onboarding-equivalent if one exists.
- `GhostSystem`/`InputRebinding` style note: library editor tooling uses UI Toolkit; runtime menus (onboarding
  output) are uGUI with TextMeshPro, per DECISIONS.md round 3 Q3.

## Tasks

| # | Task | Must-have |
|---|---|---|
| 1 | Settings page layout | Prototype (prototype skill) of Project Settings > Abstract Pixel > Game Management: catalog editor, the no-state-time-scale/input-source fields, where the settings asset location picker lives |
| 2 | Problem list | How a problem (empty slot, duplicate, missing parent, multi-parent conflict) reads: text, severity, and the fix action, "elegant, non-overwhelming" (DECISIONS.md round 1 Q4) |
| 3 | Onboarding window | Pages for choosing which menus a game needs, and the two setup paths (duplicate the library's base menu, or attach to the game's own) — DECISIONS.md round 2 Q5 |
| 4 | Runtime debugger overlay | UI Toolkit overlay reading `GameStateMachine.Journal`/`TransitionRecord`; Editor and Development Builds only, release opt-in |
| 5 | Write Sessions 6, 7, 8's `OVERVIEW.md` and `PROMPT.md` | Each links DEFINITION-OF-DONE.md, has its own "Done and verified" section and repeats the one-type-per-script rule, per PLAN-OVERVIEW.md |

## Done when

DEFINITION-OF-DONE.md holds and PROMPT.md's "Done and verified (this session)" section holds.
