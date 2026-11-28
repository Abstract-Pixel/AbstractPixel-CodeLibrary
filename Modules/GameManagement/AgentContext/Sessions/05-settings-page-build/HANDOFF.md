# Ticket 05 handoff: Project Settings page build

`plan: gm-finish, step 5`. 2026-11-28. **Done (owner closed it with the remaining checks dropped; see ticket 11).**

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
- Drag test (simulated `EditorWindow.SendEvent` drag events, not an OS drag): overlay shows on footer and
  after a rebuild, not for non-State drags, hides on drop, leaving and exit; drop adds A and B, no duplicate,
  non-State ignored, one Undo removes both. Bug fixed: overlay stuck over a row's object field (field stops
  the event; box now listens on TrickleDown).
- Drag pass 2 (same simulated events): overlay shows over a sub-state box; dropping B on the first
  row's field replaced only that row (no overlay, no extra row); overlay hides after Esc + exit. Overlay
  look checked in a still by setting its class (the simulated hover is cleared by the Editor before a
  separate capture call). Catalog on disk after a drop of A+B: B then A, appended after existing entries.
- Pass 3: dropped states land at the catalog start = page bottom (A then B on the page), confirmed after
  the rebuild. Note: delayed rebuilds only run while the Editor ticks; turn on `set_auto_tick` before
  scripted checks or the page looks stale. Handle reorder by simulated mouse events: main row onto main
  row moves it (catalog and page), sub-state onto a main row and release outside do nothing, Undo
  reverts. Sub-state box now starts at the main row's badge (x 209 at 760 and 480, right edge 701/409
  unchanged, no sideways scroll); "no listed parent" group not in the sample, not checked.
- Pass 4: Rally (6.3) with game-management-rework at 370c517: compiles, GameManagement tests 86/86, page
  opens its settings (6 rows, 0 problems), settings and catalog files byte-identical, Rally git status
  unchanged; clone put back on f65c236. The 11 SceneManagement* failures are not from T05 (cause
  analysis, no baseline run): 10 are SaveSystem sample scenes in `LibrarySamplesReferenceOnlyTheLibraryTests`
  and T05 touches no SaveSystem or Samples file; 1 is a 30 s Play-mode hold in
  `Sample10LaunchSceneTests` that uses no changed editor code.
- Standards findings left open: `StateListEdits`, `StateListValidator`, `StateProblem` and
  `GameManagementSettingsProvider` are public though game code does not need them (tests sit in another
  assembly, so making them internal needs `InternalsVisibleTo`). No `var`; files under 400 lines.
- Still not checked: + menu, - button, fix buttons + Undo, no-asset view, problem-row stills, 1200 px
  review, Scene System page look, drops on empty list / Sub-states tab / sub-state group, two-sub-state
  reorder. Ticket 05 stays open; prototype window not removed.
- Pass 5: Rally still at 760 matches the Lab look (only the Editor's own sidebar differs). Fixed: page note no longer says 'the sample's defaults' in a game. StateListEdits, StateListValidator, StateProblem(Kind) and the provider are internal, tests via InternalsVisibleTo. Prototype settings-page and no-asset windows and their 'Open all' calls removed from the Lab. Lab GM tests 86/86, Rally compiles clean on 89a97d7.
- Earlier open item resolved: the page row list read in the same call still showed the old rows (rebuild looks deferred; not
  confirmed by a still), empty list, Sub-states tab, dropped sub-state's group, handle reorder by pointer,
  whether the 11 SceneManagement* failures existed before T05. Baseline for the 11 older SceneManagement failures not compared.
- Still unverified: every real-input interaction (hover +, + menu, drag, drop, fold, fix buttons, Undo),
  edge cases, 1200 px and no-asset view review, Rally compile/data, scene pages' look, coding-standards
  review (`GameManagementSettingsProvider.cs` is 398 lines, at the limit), prototype window removal.

## Budget

7% planned, raised to 9% by the owner.
