# 05 Project Settings page build (6A-build)

Type: implement. Blocked by: 01, 02, 03 (all looks first, owner 2026-11-27) and 04. Budget: `plan: gm-finish, step 5`.

## Goal

The real Project Settings > Abstract Pixel > Game Management page, matching the approved look, with its problem list.

## Read first

- `../../Sessions/Session6A/HANDOFF.md` (all), its stills, and `../../UI-STYLE.md`.
- `../../Sessions/Session5/GRILL-RESULTS.md` §1 and §2, plus the rule change in `../../Sessions/Session5/HANDOFF.md` §4.
- Build it like `Modules/SceneManagement/Editor/Settings/SceneSystemSettingsProvider.cs`.

## Must-have

- First, move `SceneSystemUI`'s generic helpers to a shared `AbstractPixel.Core.Editor.UI` assembly. The scene system
  keeps working (its tests pass, its pages look the same).
- Validator rules, tests first: self-parent or loop (Error), empty parent slot (Warning), same parent twice (Warning),
  some parents missing from the catalog (Warning), all parents missing (Error).
- The page: five cards, state list with tabs, + / −, drop box, Preloaded Assets warning with Fix now, the no-asset view.
- The rank hint clipped at 480 px is fixed.

## Done and verified

- Module tests pass (count in the handoff); clean compile in the Lab and Rally.
- The real page captured at 480, 760 and 1200 px beside the approved stills; differences fixed or listed.
- Used by hand as a developer: hover +, + menu, drag overlay, drag-to-reorder, group folding, Create settings, Fix now, Move….
- The sample's and Rally's settings assets open on the page with no change to their data.
