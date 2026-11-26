# Session 5 prompt: UI design grill

You are an autonomous agent working in ProjectRallyShift on the AbstractPixelLibrary submodule's game management
module. plan: gm-commercial, step 5. Budget 4% of the 5-hour window.

## What "finished" means

`LocalPackages/AbstractPixelLibrary/Modules/GameManagement/AgentContext/DEFINITION-OF-DONE.md` is the general
guideline; read it first. This session is finished only when it holds **and** the section below holds. Anything cut
goes in the handoff's "Not done" table.

## Done and verified (this session)

- **Settings page, problem list, onboarding and debugger each have an approved prototype** (the `prototype` skill;
  stills shown to the owner, approved in one round) covering layout, the scene system's visual language
  (`SceneGroupsDebugger` reference stills included), and how each screen behaves (what a click does, what a problem
  looks like, what the two onboarding paths show).
- **Sessions 6, 7 and 8 each have their own `OVERVIEW.md` and `PROMPT.md`**, written from this session's approved
  prototypes: concrete end state (which screens, which fields, which interactions), their own "Done and verified"
  section, linking DEFINITION-OF-DONE.md, and the one-type-per-script rule repeated in their Rules.
- **Open questions resolved, not deferred again:** multi-parent conflict display (DECISIONS.md round 1 Q4 + round 2
  Q2), the settings-asset-location picker's exact field (DECISIONS.md round 3 Q9), and the debugger's before/after
  effect diff view (named a nice-to-have for Session 8 if not designed now).
- No runtime or editor code changes in this session (it is a grill: prototypes only, in the scratchpad or a throwaway
  scene, never committed as product code).

## Starting point

Session 4B is done: every public type in `Runtime/`/`Editor/` passes NAMING.md's read-alone test; folders are under
the 8-script limit. Read "Starting point" in `OVERVIEW.md` for what that means for this session's prototypes
(the exact type and field names to show on screen) and what Session 4B could not verify (confirm those are clear
before Sessions 6-8 start changing code; this grill session itself does not depend on them).

## Read first

`AgentContext/PLAN-OVERVIEW.md` (rules), `DEFINITION-OF-DONE.md`, `Sessions/Session4B/HANDOFF.html`, this session's
`OVERVIEW.md`, the library's and Rally's `CLAUDE.md`, `Wayfinder/issues/02-ui-design-grill.md` (the question this
session answers), `DECISIONS.md` (rounds 1-3 and the 2026-11-26 entries, for every design call already made).

## Start

- First tool call: `budget start` with `plan: "gm-commercial"`, `step: 5`, `kind: "grill"`, `budgetPct: 4`,
  `wrapReservePct: 1`. If the scope check fires, do the must-haves first and spend what is left on the rest.
- Use the `grilling` skill to drive the question in Wayfinder/issues/02, and the `prototype` skill to answer it with
  stills rather than a written spec alone.
- Capture stills of the scene system's `SceneGroupsDebugger` for reference before designing anything, so the new
  screens are checked against it, not designed in isolation.

## Check

- Each of the four screens (settings page, problem list, onboarding, debugger) has at least one approved still and
  one line on its behaviour the owner has seen and not pushed back on.

## Finish

`Sessions/Session5/HANDOFF.html`, update `Wayfinder/map.md` (resolve ticket 02), write `Sessions/Session6/OVERVIEW.md`
+ `PROMPT.md`, `Sessions/Session7/OVERVIEW.md` + `PROMPT.md`, `Sessions/Session8/OVERVIEW.md` + `PROMPT.md`,
`budget end`. Commit and push by default (PLAN-OVERVIEW.md); the handoff lists the commits. A grill session's commits
are prototypes/docs only, never product code.

## Rules

- Stay inside ProjectRallyShift, `../Abstract_Pixel_Lab` and the session scratchpad; put this rule in any subagent
  brief.
- One type per script: carried forward for any throwaway prototype code that is not deleted before the session ends.
- Names follow `NAMING.md` even in a prototype, so approved names do not need re-deciding in Session 6-8.
- No strings configure anything (DECISIONS.md, 2026-11-26): prototype the picker/reference fields accordingly, not
  as text fields with string IDs.
- If the budget runs low: finish the must-haves (at least rough prototypes and the three next sessions' prompts) and
  list the rest in the handoff.
