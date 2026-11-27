# Session 6A handoff: Project Settings page look approved

`plan: gm-commercial, step 6A`. Written 2026-11-27. The owner approved the look of the Project Settings >
Abstract Pixel > Game Management page after 8 rounds. **The real page is not built yet.**

## 1. What the owner decided

- The five assumptions in Session 5 HANDOFF.md §5: **all yes.**
  1. The real page is built in the library after the look is final.
  2. The Debugger card's two new fields are shown disabled or left out until 8A.
  3. The new problem rules (self-parent or loop, empty parent slot, same parent twice, warning/error split) go into
     the validator with the page build.
  4. `SceneSystemUI`'s generic helpers move to a shared `AbstractPixel.Core.Editor.UI` assembly at the start of the build.
  5. "Set up states…" and the onboarding toggle stay inactive until 7A.
- The approved look is written down as rules in **`AgentContext/UI-STYLE.md`**. Every later UI piece follows it.
- The hover "add sub-state" button is a small 22 px "+" (tooltip "Add sub-state: …"). Kept through all rounds;
  the owner did not object.

## 2. What exists

| Thing | Where | State |
|---|---|---|
| Approved stills | `Sessions/Session6A/stills/` — window 01 at 480/760/1200 + Sub-states tab at 760 (round 8); window 02 at 480/760/1200 | Approved |
| Style rules | `AgentContext/UI-STYLE.md` | New, approved look |
| Prototype windows 01, 02 | Lab: `Assets/GameManagementUIPrototype/` (`UI/SettingsPage.uxml`, `SettingsNoAsset.uxml`, `StateListRow.uxml`, `SettingsPage.uss`; `Editor/GameManagementPrototypeRows.cs`, `StateRowSample.cs`) | **Untracked in the Lab, never committed. Don't delete:** the USS is the source of truth for exact values. |
| Capture tools | `Sessions/Session5/capture-tools/`. 6A added helpers in its scratchpad copy of `ProtoShot.cs`: `SubTab` (switch to the Sub-states tab) and `CloseAll` (close a window type before grabbing). Add them to the `.txt` if you need them. | Working |

## 3. What changed in the prototype during 6A (rounds 1-8)

The final result is in UI-STYLE.md. In short: window 02 scrolls only up and down; the state list is one shaded
container with tabs, a footer (drag hint on the left, + / − on the right) and 6 px padding; rows are 18 px and line up
with the foldout arrow (2 px above and below each row); main rows have grey rank badges; sub-states sit in one teal
outline per parent with a "Sub-states" header, teal `2.1` badges and a muted "Opens over" line; no per-row warning
icon; help boxes start at the badge; "Check states before a build" sits above a full-width Catalog; notes start with `NOTE:`.

## 4. Not done / open

- **Build the real page** (Session 5 HANDOFF §5, now a sub-session of its own: **6A-build**). Before changing
  product code, clear Session 4B's two open checks (Rally EditMode tests and parity scenarios; Lab sample PlayMode
  flow test rerun). Neither was run in 6A.
- At 480 px the rank hint in the tab bar is cut off behind Unity's tab scroll arrow. Fix when building (for example:
  hint on its own line under the tabs when narrow).
- The prototype's Sub-states tab lists only Settings and Credits, not the sample's Retry Prompt / Share Score.
- Never checked by anyone: the hover "+", the + menu, the drag overlay, drag-to-reorder, folding a group. Check them in the real page.
- Captures: a leftover window 02 or the Windows Start menu can cover a grab. Close them first and read every still.

## 5. Next sub-session

The owner wants the next prototype look finalized before any more building: **6B, the State inspector (`StateSO`) and
the shared state field drawer** (prototype windows 03, 04, 05; GRILL-RESULTS.md §3 and §2). The settings-page build
(6A-build) runs after it or alongside it, as the owner chooses.

## 6. Prompt for Session 6B

> You are working in the AbstractPixelLibrary game management module (library at
> `Abstract_Pixel_Lab/LocalPackages/AbstractPixelLibrary`, branch `main`; prototypes in the Lab project at
> `Assets/GameManagementUIPrototype/`). plan: gm-commercial, step 6B. Read
> `Modules/GameManagement/AgentContext/Sessions/Session6A/HANDOFF.md` first, then `AgentContext/UI-STYLE.md` (the
> approved look, which overrides older notes), then Session 5's `GRILL-RESULTS.md` §2-3 and `HANDOFF.md` §1 and §3.
>
> 1. Open prototype windows 03, 04 and 05 (State inspector main state, sub-state, and the shared state field drawer;
>    menu `Tools/External Project Tools/Game Management/UI Prototype/`). Restyle them to UI-STYLE.md first (badges,
>    18 px rows, teal sub-state look, help boxes, NOTE labels).
> 2. Capture each at 480, 760 and 1200 px with `Session5/capture-tools/shot.ps1.txt` (copy to your scratchpad, drop
>    `.txt`; never use `capture editor_window`). Close other windows that could cover the grab. Read every still,
>    measure alignment in pixels when it matters, fix what is unreadable, overlapping or clipped.
> 3. Send the owner every still with SendUserFile (status "proactive", display "render") and a one-line caption. List
>    interactions a still can't show. Iterate until the owner says the look is final.
> 4. Add any new approved rules to `AgentContext/UI-STYLE.md`. Finish with stills in `Sessions/Session6B/stills/`,
>    `Sessions/Session6B/HANDOFF.md` listing what the owner approved, and the next prompt.
>
> Look only: no product code in 6B unless the owner says so. Rules: stay inside ProjectRallyShift, Abstract_Pixel_Lab
> and your scratchpad; prototypes stay untracked in the Lab; library commits stage by path, one commit per task with
> the task ID first (`S6B-T1: …`), never push, never stage `Modules/InputRebinding/Rebinding UI/Icons/*.png.meta`,
> `Core/Runtime/SuperLogger/DebugDataBase.asset` or `CODING_STANDARDS.md.meta`.
