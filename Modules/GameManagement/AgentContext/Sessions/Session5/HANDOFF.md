# Session 5 handoff: UI design grill → per-UI sub-sessions

`plan: gm-commercial, step 5`. Written 2026-11-27. Read this file first; it is enough to start Session 6A.

## 1. Decision: one UI piece per sub-session

Session 5 tried to prototype all UI windows at once (10 windows). That was a mistake: no window got finished, and
the owner found layout bugs a "compiles, 0 errors" report never showed. **From now on each UI piece gets its own
sub-session, and the owner finalizes its look there before the next piece starts.** Sessions 6, 7 and 8 of
`PLAN-OVERVIEW.md` are replaced by:

| Sub-session | UI piece | Prototype windows to start from | Spec section in GRILL-RESULTS.md |
|---|---|---|---|
| **6A (first)** | Project Settings > Abstract Pixel > Game Management | 01 Settings Page, 02 Settings Page, No Asset | §1 Settings page, §2 Problem list |
| 6B | State inspector (`StateSO`) and the shared state field drawer | 03, 04, 05 | §3 State Inspector and other components, §2 |
| 6C | Play-mode watchdog (no new UI; from the old Session 6 row) | none | PLAN-OVERVIEW.md Session 6 row |
| 7A | Onboarding window | 06, 07, 08 | §4 Onboarding window |
| 8A | Debugger overlay (in game) | 09 | §5 Runtime debugger |
| 8B | Game State Debugger window | 10 | §5 Runtime debugger |

Sub-session order is the table order. Only 6A has its prompt (section 6 below); each sub-session writes the next
one's prompt in its own handoff.

### Workflow every sub-session follows

1. **Look first.** Start from the prototype window, apply the owner's feedback, and show the owner captured stills.
   Repeat until the owner says the look is final. No tests or architecture before that (owner's standing rule).
2. **Capture-and-verify rule.** Every still shown to the owner was captured with the method in section 3 **and
   opened and read by the agent first**, at three widths: narrow (about 480 px), normal docked (about 760 px) and
   wide (about 1200 px). Check: nothing overlaps or is clipped, text is readable and short, fields line up, controls
   look like controls. "Compiles, 0 errors" is never evidence of a good look. Interactions a still cannot show (hover,
   tab switch, drag-over) are listed for the owner to try by hand.
3. **Then build it for real** in the library (tests first, per the library CLAUDE.md), and capture the real page the
   same way to show it matches the approved look.
4. **End** with stills saved in the sub-session's folder, a handoff that lists what the owner approved, and the next
   sub-session's prompt.

## 2. What exists

| Thing | Where | State |
|---|---|---|
| Grill decisions (Q1-Q25) | `AgentContext/Sessions/Session5/GRILL-RESULTS.md` | Done; the spec for every piece. Owner changes after the grill are in section 4 below and override it. |
| Research: Unity editor UI rules | `AgentContext/Sessions/Session5/research/EDITOR-UI-PRINCIPLES.md` | Done; 25-item checklist |
| Research: scene system's look | `AgentContext/Sessions/Session5/research/SCENE-SYSTEM-UI.md` | Done; the visual language to copy |
| Scene system UI to copy | `Modules/SceneManagement/Editor/Settings/SceneSystemSettingsProvider.cs` (page path `Project/Abstract Pixel/Scene System`), `SceneSystemUI.cs`, `SceneGroupsDebugger.cs` | Existing product code |
| Prototype windows (look only, hard-coded data) | Lab project: `Assets/GameManagementUIPrototype/` (`Editor/` scripts + asmdef `AbstractPixel.GameManagement.UIPrototype.Editor`, `UI/` UXML/USS). Menu: `Tools/External Project Tools/Game Management/UI Prototype/` (01-10, Open all) | **Untracked in the Lab repo, never committed** (throwaway). Window 01 is the only one the owner has reviewed. |
| Window 01 files | `UI/SettingsPage.uxml`, `UI/SettingsPage.uss`, `UI/StateListRow.uxml`, `UI/GameManagementPrototype.uss`, `Editor/GameManagementSettingsPagePrototypeWindow.cs`, `Editor/GameManagementPrototypeRows.cs`, `Editor/GameManagementPrototypeScreen.cs`, `Editor/StateRowSample.cs`, `Editor/GameManagementPrototypeDropBox.cs` | Owner round 1 applied, then agent review fixes (section 4). Not yet approved. |
| Window 01 stills | `AgentContext/Sessions/Session5/stills/01-settings-page-760.png`, `01-settings-page-480.png`, `01-settings-page-760-before-fixes.png` | Latest look, and the broken look for comparison |
| Capture tools | `AgentContext/Sessions/Session5/capture-tools/` (`shot.ps1.txt`, `ProtoShot.cs.txt`, `Dump.cs.txt`) | Saved as `.txt` so Unity does not compile them inside the package |
| Settings data the page edits | `Runtime/Machine/Settings/GameManagementSettings.cs` (`Catalog`, `NoStateTimeScale`, `JournalSize`, `InputSource`, `InputActions`, `DefaultFolder = "Assets/GameManagement"`), `Runtime/Machine/Settings/GameStateCatalog.cs` (`Contains`, `FindEmptySlots`, `FindDuplicates`, `FindParentsMissingFromCatalog`), `Runtime/Configuration/StateSO.cs` | Existing product code |
| Settings asset helper | `Editor/GameManagementSettingsAssets.cs` (`Find`, `Use`, `WithSettingsPreloaded`, a pre-build hook) | Existing product code |
| Settings assets in projects | Library sample: `Samples/01 Minimal Game/States/Minimal Game Settings.asset`. Rally: `Assets/[1]Project/ProjectServices/GameManagment/RallyGameManagementSettings.asset`. The Lab has none in `Assets/`. | Existing |
| Uncommitted library edits from the grill | `AgentContext/DECISIONS.md` (2026-11-27 entry), `GLOSSARY.md` ("Rank") | Not committed yet |

Paths without a project are relative to `Modules/GameManagement/` in the library.

**Not done from Session 5's original PROMPT.md:** the owner never approved windows 02-10, Sessions 6/7/8
OVERVIEW+PROMPT were replaced by this sub-session plan, `Wayfinder/map.md` ticket 02 is not marked resolved, and
nothing is committed. Session 4B's two open checks (Rally EditMode tests and parity scenarios; the Lab sample PlayMode
flow test rerun) are still open and must be cleared before 6A changes product code.

## 3. What was learned: capturing editor windows

- **Never run `capture editor_window` on the Lab** (Unity 7 alpha). It crashes the Editor.
- **Working method:** bring the Unity window to the front from PowerShell (`SetForegroundWindow`), then from a
  pipeline `script run` read the window's screen rectangle back with
  `UnityEditorInternal.InternalEditorUtility.ReadScreenPixel` and save a PNG. If Unity is behind another app, the grab
  shows that app, which is why the foregrounding step is required.
- `capture-tools/shot.ps1.txt -Type <WindowClass> -W <w> -H <h> -Out <png> -Menu "<menu item under UI Prototype/>"`
  closes the window, runs `AssetDatabase.Refresh()` (USS edits made from outside Unity are not picked up otherwise),
  reopens it from its menu, sizes it, foregrounds Unity and grabs it. Two grabs of the same build scored SSIM 1.000, so
  the method is stable. Compare before/after with ffmpeg:
  `ffmpeg -i a.png -i b.png -filter_complex "[0]crop=W:H:0:0[a];[1]crop=W:H:0:0[b];[a][b]ssim" -f null -`.
- `capture-tools/Dump.cs.txt` writes a window's element tree with each element's layout rect and flex values to the
  temp folder. Use it when a layout looks wrong: it found the foldout-toggle bug in one run.
- PowerShell 5.1 strips quotes from native arguments: pass `--args` JSON as `'[\"Name\", 760, 880]'`.

## 4. Window 01: owner feedback and fixes so far

Owner round 1 (settings page), applied:
- One shared label column across all 5 cards so every field starts at the same x.
- States list: **Main states / Sub-states** tabs. Main states shows ranked main states; one with sub-states gets a
  foldout listing them indented. Sub-states is a flat list with "Opens over: …". The "Main state" text column is gone
  (the owner found it confusing); the Main states tab tooltip says "States with no parent. Higher rank wins."
- No Select button: clicking the state field selects the asset. Hover-only add-sub-state button on main rows.
- A **+ / −** footer under the list (+ offers "Add existing state…" / "New state…"); the old Add state… / New state…
  buttons are gone. The whole list is a drop box with a hint row and an "Add to the end of the list" overlay.
- **Rule change:** a sub-state with some parents missing from the list is a **Warning** ("…It still opens over Main
  Menu."); with all parents missing it is an **Error** ("…so Credits can never open."). Overrides the Missing parent
  row in GRILL-RESULTS.md §2.

Agent review fixes (found by capturing and reading stills, not by the build):
- **The page-wide field-stretch rule must not reach foldout toggles.** `.gm-page .unity-base-field { flex-grow: 1 }`
  also matched the TreeView's foldout toggle (a base field), which grew to 144 px and pushed every row toward the
  middle. Fixed with `.gm-page .gm-list .unity-tree-view__item-toggle { flex-grow: 0; }`.
- **Vertical scrolling only.** The page ScrollView was `VerticalAndHorizontal`, so content grew wider than the window
  and the right edge (Move…, Check again, +, help boxes) was clipped. Now `mode="Vertical"`.
- **Tree rows shrink with the window.** `.unity-tree-view__item-content` needs `flex-basis: 0; flex-shrink: 1;
  min-width: 0`, or rows keep their content width and run past the list edge at narrow widths.
- Help boxes: the fix button and Learn more sit inside the box and wrap below the text when narrow.
- Narrow widths: label column `width: 40%` (110-180 px), header status text ellipsizes instead of running under
  Check again, left tree items ellipsize.
- The hover add-sub-state button became a 22 px "+" with the tooltip "Add sub-state: …", because a 70 px hidden text
  button squeezed every main row. **This changes an agreed item; the owner must confirm it.**

Still unchecked by anyone: switching to the Sub-states tab, the hover "+", the + menu and the drag-over overlay.

Smaller fixes made to windows 03-06, 09 and 10 in the same pass are in the prototype files. Those windows are **not
owner-reviewed**; their sub-sessions review them from scratch.

## 5. Session 6A scope: Project Settings > Abstract Pixel > Game Management

Known from the project (GRILL-RESULTS.md §1-2, section 4 above, and the code listed in section 2):
- A Project Settings page at Abstract Pixel > Game Management, built like `SceneSystemSettingsProvider.cs`.
- Five numbered cards: 1 Setup, 2 States, 3 Between scenes, 4 Input, 5 Debugger.
- Setup: the settings asset object field (`GameManagementSettingsAssets.Use` on change), read-only asset path with
  Move…, the no-asset state with Create settings (window 02), and the Preloaded Assets warning with Fix now.
- States: the window 01 design from section 4, with problems from `GameStateCatalog.FindEmptySlots`,
  `FindDuplicates` and `FindParentsMissingFromCatalog`, and the "Check states before a build" toggle.
- Between scenes, Input, Debugger: the fields that exist on `GameManagementSettings` (`NoStateTimeScale`,
  `InputSource` + `InputActions`, `JournalSize`).

**Assumptions for the owner to confirm at the start of 6A:**
1. 6A builds the real page in the library in this session, after the look is final, not only the prototype.
2. The Debugger card's two new fields (show/hide `InputActionReference`, include-in-release toggle) are shown
   disabled or left out until 8A adds them to `GameManagementSettings`.
3. The new problem rules (self-parent or loop, empty parent slot, same parent twice, the warning/error split for
   missing parents) are added to the validator in 6A, since the page shows them.
4. The move of `SceneSystemUI`'s generic helpers to a shared `AbstractPixel.Core.Editor.UI` assembly (GRILL-RESULTS
   "Shared look") happens at the start of 6A.
5. "Set up states…" in the empty state and the Setup card's onboarding toggle stay as inactive buttons until 7A.

## 6. Prompt for Session 6A

> You are working in the AbstractPixelLibrary game management module (library at
> `Abstract_Pixel_Lab/LocalPackages/AbstractPixelLibrary`, branch `main`; prototypes in the Lab project).
> plan: gm-commercial, step 6A. Read `Modules/GameManagement/AgentContext/Sessions/Session5/HANDOFF.md` first, then
> GRILL-RESULTS.md §1-2, `research/EDITOR-UI-PRINCIPLES.md` and `research/SCENE-SYSTEM-UI.md`.
>
> 1. Confirm with the owner the five assumptions in HANDOFF.md section 5. Clear Session 4B's two open checks before
>    changing product code.
> 2. Open prototype window 01 in the Lab, capture it at 480/760/1200 px with `capture-tools/shot.ps1.txt`, read the
>    stills yourself, and show the owner. Iterate on the look until the owner says it is final. Never use
>    `capture editor_window`.
> 3. Build the real Project Settings page in the library, tests first, following the scene system's page and the
>    approved stills. Capture the real page the same way and show it matches.
> 4. Finish: stills in `Sessions/Session6A/stills/`, `Sessions/Session6A/HANDOFF.md` listing what the owner
>    approved, and the prompt for 6B.
>
> Rules: stay inside ProjectRallyShift, Abstract_Pixel_Lab and your scratchpad; one type per script; names follow
> NAMING.md; no strings configure anything (pickers and object fields only); errors say what, where and how to fix;
> commit only with the owner's OK, stage by path, never stage `Modules/InputRebinding/Rebinding UI/Icons/*.png.meta`.
