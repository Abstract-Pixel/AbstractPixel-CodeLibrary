# 06 State inspector and state field drawer build (6B-build)

Type: implement. Blocked by: 05 (shared UI helpers). Budget: `plan: gm-finish, step 6`.

## Goal

The real `StateSO` inspector (main state and sub-state) and the shared state field drawer.

## Read first

- `../../Sessions/Session6B/HANDOFF.md` (all), its stills, and `../../UI-STYLE.md` "State inspector" and "Long names".

## Must-have

- **Long names everywhere in this view** (parent names, input action map names, table cells): one line, cut with "…",
  full name on hover; sentences wrap; nothing overlaps, scrolls sideways or pushes a control off from 275 to 1200 px.
- **A real Input comparison** (the prototype shows sample data): maps from each action, each parent's maps, compared
  as sets (order and repeats ignored). `GameEffects.SameMaps` compares in order, so it can't be reused as is.
- Parent values resolved from the real assets, as `StateEffectResolver` does; "keeps the last value" shown.
- Settle with the owner before code: the three questions in the 6B handoff §4 ("Is sub-state" field, the unsaved
  "Own not chosen yet" marker, what Own starts from), and how to show a parent that is itself a sub-state.
- The drawer: a normal object field plus a red icon and tooltip when the state isn't in the catalog. Show the owner stills.

## Done and verified

- Tests first for the Input comparison and value resolution; module tests pass.
- Stills at 275, 480, 760 and 1200 px with the long test names from UI-STYLE.md, each read.
- Used by hand: drag-reorder parents, Unity's object picker, the Own switch, Open in Project Settings.

## From ticket 05

- Shared look: `AbstractPixel.Core.Editor.UI.EditorUI`. Don't add scene-module references to Game Management.
- `Editor/Problems/StateListValidator` is the one source of problems: the inspector's Problems block reads it (GRILL Q15).
  It does not yet report "State not in the list" (GRILL §2, Inspector and drawer only); add it there, tests first.
- Its `Learn more` currently opens the module README (`GameManagementSettingsProvider.OpenReadMe`).
- New State assets are made by `NewStateMenu`; a settings asset may live in a package, so never assume its folder is under `Assets`.
- Detail: `../../Sessions/05-settings-page-build/HANDOFF.md` ("Findings for later sessions" covers a stale-compile trap).

## Mega run: step S0, close this ticket

Added 2026-11-28 for [../mega-07-09/MEGA-TICKET.md](../mega-07-09/MEGA-TICKET.md). Where this section differs from
the text above, this section wins. Session folder: `../../Sessions/06-state-inspector-build/` (rewrite the top of
`HANDOFF.md` with the brief's headings; keep the old text below under `## History`).

The code is built and uncommitted. Read that HANDOFF.md (the third round wins). Do not rebuild it or rerun checks it
passed.

**State to start from** (check it): LIB on `main`, `game-management-rework` at `89a97d7`; uncommitted ticket 05
owner fixes, ticket 06 code, and docs (earlier look docs and the mega run files). Rally LIB at `89a97d7`.

**Must-have**
1. Commit without losing anything: docs on `main` first, by path; then carry the code to
   `game-management-rework`, commit it there (05 fixes and 06 apart, new `.meta` files included), merge into `main`.
   Never commit `Core/Runtime/SuperLogger/DebugDataBase.asset` or the Sample 01 catalog (check its diff: it showed as
   modified before ticket 06). Leave the Lab's `Assets/GameManagement/States/New State.asset` alone.
2. Fix, tests first where it is logic: "Leave on scene change" is cut below about 375 px. The missing-parent
   problem names the long state once in its sentence (the fix button may name it again).
3. Check what was still unverified and needs no physical mouse: the table after renaming a state and an input map;
   Project Settings keeps focus while typing, with the asset watcher on; the narrow-table wrap in the Lab; the
   no-settings and no-catalog messages; the state field drawer in both Editors, with stills saved for the owner.
4. Rally gets the commit and keeps it: R1, R2, R3 and R4 of
   [../mega-07-09/RALLY-DOGFOOD.md](../mega-07-09/RALLY-DOGFOOD.md) (R4 without the pill, which ticket 07 adds).
5. Verifier PASS. Skip list for round 1: the handoff's passed checks whose code this step did not change.

**Decided in advance (do not ask)**
- Real-mouse drag-reorder, drag and drop and the object picker stay in ticket 11, as the owner did for ticket 05.
  This replaces "Used by hand" above.
- The drawer keeps the red icon and tooltip (GRILL Q18). "Show the owner stills" means: save them and list the paths.
- The empty table header under "Add a parent" and Unity's `(State SO)` suffix stay as built.
- "Settle with the owner before code" above is done: see the handoff's owner decisions.
