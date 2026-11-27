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

In `Sessions/Session6B/OVERVIEW.md` and `Sessions/Session6B/PROMPT.md`. They point back to this handoff and to
`AgentContext/UI-STYLE.md`.
