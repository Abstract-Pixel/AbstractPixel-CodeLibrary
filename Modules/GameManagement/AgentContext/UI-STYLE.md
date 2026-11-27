# Game Management UI style

The look the owner approved for Game Management editor UI. Every later UI piece (6B onward) follows it unless the
owner changes it. Approved 2026-11-27 in Session 6A on the Project Settings page (stills:
`Sessions/Session6A/stills/`). The source of truth for exact values is the prototype USS in the Lab project:
`Assets/GameManagementUIPrototype/UI/SettingsPage.uss` and `GameManagementPrototype.uss`.

## Page

- Numbered cards (`1. Setup`, `2. States`…), each with a `?` help button on the right of its title.
- One shared label column on the whole page: label width 40%, 110-180 px. Every field starts at the same x.
- The page scrolls only up and down (`ScrollView mode="Vertical"`). Nothing may scroll sideways at 480 px.
- The page-wide field rule (`.gm-page .unity-base-field { margin-top: 2px; flex-grow: 1 }`) must not reach
  in-row fields or foldout toggles. Override with a selector at least as specific (`.gm-page .gm-state-field`).
- Notes under a card start with `NOTE:` in capitals, then the sentence as written.
- Order in the States card: "Check states before a build", then a full-width Catalog field, then 8 px, then the list.

## Tabs

- Tabs keep their own width. Each tab header has a light background (`rgba(255,255,255,0.1)`, top corners 3 px).
  A 4 px page-colour gap separates them. No border line between them.
- A short hint may follow the tabs in the tab bar (muted 11 px text, ellipsized).
  Known issue: at 480 px the tab bar has no room and Unity shows a scroll arrow over the hint.

## List container

- One container, no double border: a shaded box (`rgba(0,0,0,0.12)`) right under the tabs, and the list itself
  has no border.
- 6 px of space above the first row and below the last.
- The list is as tall as its rows (no empty band, no inner scroll). A collapsed group takes one line.
- Footer: shaded like the tab bar (`rgba(255,255,255,0.1)`). The drop hint "Drag State assets here to add them" is on
  the left; `+` and `−` are on the right. The `+` menu ("Add existing state…", "New state…") opens up from the right edge.
- Dragging assets over the list shows a blue overlay, "Add to the end of the list".

## Rows

- Each row is: reorder handle `≡` (12 px) → index badge → object field → hover `+` (main rows only).
- The handle, index badge, field and hover `+` are all 18 px tall and share one top edge.
  The badge touches the field with no gap: badge corners rounded on the left, field corners square on the left.
- 4 px between rows, split as 2 px above and 2 px below. This keeps the tree's foldout arrow on the same vertical
  centre as the row content (measured within 0.5 px). A gap only below the row puts the arrow off-centre.
- Main-row badge: grey (`rgba(255,255,255,0.08)`), the rank number.
- Tree indent is 0. Sub-state boxes line up with the main row's handle on the left, and on the right with the end of
  the main-row field (26 px kept for the hover `+`).
- Group header rows (for example "Sub-states with no listed parent"): bold muted 11 px text, 18 px line, centred
  on the foldout arrow.

## Sub-states (in the Main states tree)

- The sub-states of one parent sit in **one** teal outline (`rgba(70,140,150,0.7)`, 1 px, 3 px corners, faint teal
  fill, 3 px padding). With several sub-states, only the first draws the top edge and only the last the bottom
  edge, so they read as one box.
- A muted bold "Sub-states" header sits inside the box, at the top.
- Sub-state badge: teal (`rgba(70,140,150,0.55)`, white text), numbered `<parent rank>.<n>` (`2.1`, `3.2`).
  A sub-state with no listed parent shows `?.1`.
- Under the field: a calm muted 10 px line, "Opens over: Main Menu, Pause". No fill, no capitals.
- In the Sub-states tab the rows are plain: no outline, no fill, no group header.

## Problems (warnings and errors)

- No severity icon on the row. The problem shows once, in a help box under the row.
- The help box starts at the index badge's left edge (13 px in from the handle) and ends 4 px inside the box's right edge.
- The fix button ("Add Pause") and "Learn more" sit inside the help box and wrap below the text when narrow.
- Warning vs error: a sub-state with some parents missing is a Warning ("…It still opens over Main Menu."); with all
  parents missing it is an Error ("…so Credits can never open.").

## How a look gets approved

Capture the window at 480, 760 and 1200 px with `Sessions/Session5/capture-tools/` (never `capture editor_window`),
read every still, measure alignment in pixels when it matters, close other windows that could cover the grab
(window 02, the Start menu), then show the owner. Interactions a still cannot show are listed for the owner to try.
