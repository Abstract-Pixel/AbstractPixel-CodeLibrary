# Ticket 05 handoff: Project Settings page build

`plan: gm-finish, step 5`. 2026-11-28. **Built; verification pending (Opus, low effort).**

## Built

- `Core/Editor/UI` (`AbstractPixel.Core.Editor.UI`, class `EditorUI`): the generic helpers moved out of `SceneSystemUI`.
  `SystemPill` was not made (ticket 07 needs it).
- `Editor/Problems/`: validator, fixes, state list edits, pre-build check. New setting `CheckStatesBeforeBuild` (on).
- `Editor/Settings/`: the page, state list, rows, handle drag, drop box, + menu, USS.
- `GameManagementSettingsAssets.Create` / `IsPreloaded`.
- The list is plain VisualElements, not a TreeView.
- The catalog is highest priority first; the page lists main states in reverse, so rank 1 is the lowest.

## Checked

| Check | Result |
|---|---|
| Lab compile | clean |
| Lab `AbstractPixel.GameManagement.Tests` EditMode | 86/86 |
| Lab `AbstractPixel.SceneManagement*` EditMode | 1082 pass, 11 fail (10 `LibrarySamplesReferenceOnlyTheLibraryTests` on SaveSystem samples, 1 `Sample10LaunchSceneTests`); not compared with a baseline |
| Captures | 760 and 480 px with the sample's settings, in `stills/`; rank hint no longer clipped |
| Sample assets | unchanged after opening the page |

Validator tests were written before the validator, but a failing run was not recorded.

## Differences from the approved stills

- "?" and "Learn more" open the module README (no in-Editor GM docs yet; ticket 12).
- "+ Add existing state" is a submenu of unlisted states, and the menu opens below the button, not upwards.
- Debugger card's two new fields are shown disabled.
- "Set up states..." is not shown (empty list shows "No states yet."); ticket 07.

## Not checked yet

1200 px, no-asset view, problem rows, every hand interaction, Rally compile and data, scene pages' look,
pre-build check in a real build, 6.7.

## Verification pass (Opus, 6% budget)

Budget allowed only a partial pass; ticket 05 stays open.

- Lab compile clean; `AbstractPixel.GameManagement.Tests` 86/86; `AbstractPixel.SceneManagement.Tests` 229/229.
- 480 px still taken again: rows aligned, no sideways scroll.
- No bugs found, no code changed. Baseline for the 11 older SceneManagement failures not compared.
- Still unverified: every real-input interaction (hover +, + menu, drag, drop, fold, fix buttons, Undo),
  edge cases, 1200 px and no-asset view review, Rally compile/data, scene pages' look, coding-standards
  review (`GameManagementSettingsProvider.cs` is 398 lines, at the limit), prototype window removal.

## Budget

7% planned, raised to 9% by the owner.
