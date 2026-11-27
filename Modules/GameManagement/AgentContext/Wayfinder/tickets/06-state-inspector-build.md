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
