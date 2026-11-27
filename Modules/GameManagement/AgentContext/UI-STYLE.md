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

## State inspector (`StateSO`)

Approved in Session 6B (stills: `Sessions/Session6B/stills/`). Exact values: the Lab prototype's
`Assets/GameManagementUIPrototype/UI/StateInspector.uss` and `.uxml`.

- It sits under Unity's own ScriptableObject header. No custom header. A sub-state has a 4 px teal line
  (`rgb(70,140,150)`) as the first thing under that header, edge to edge.
- Order: general info, then **Problems**, **Software Effects**, **Advanced**. All foldouts start collapsed.
- General info: a type tag (teal "Sub-state" or grey "Main state") with an **Open in Project Settings** button at the
  top right; "Opens over" (small muted caption, then one teal-outlined chip per parent, or muted "nothing" /
  "no parent yet"); the "Is sub-state" checkbox; then, for sub-states only, two panels 8 px apart: **Parents** and
  **What this state does over each parent**.
- Problems, Software Effects, Advanced and the two panels use component-style header bars with a foldout. Panel
  contents have 12 px above and below.
- Parent rows: handle, index pill joined to the object field (grey, as main rows), 18 px tall, 6 px apart.
- Over-each-parent panel: a `NOTE:` line (NOTE in bold capitals), then a bordered table with a shaded head,
  alternating rows, 1 px column separators and numbered pills joined to the parent names.
- A table value is a teal tag only when what this state uses differs from what that parent uses. Being set to Own is
  not enough. Compare the real values, never the displayed text.
- Until the developer has chosen an Own value, switching to Own starts from parent 1's value.
- Each effect's value shows only on Own (Input: its "Input actions" foldout). On "Same as parent" (main states:
  "Keep last") only the muted inherited line shows.
- Input action rows carry a chip with the action map's name.
- Sub-states use teal for selected and clicked colours. Main states keep Unity's own.

## Long names (required in the build)

Real names can be far longer than the prototype's samples. **The real state inspector must handle them everywhere in
the view.** Added at the owner's request when the 6B design was approved. The owner asked for the requirement; the
handling below was written by the agent, and **no screen has been checked with long names**. The prototype was left
unchanged on purpose.

General rules:

1. From 275 px to 1200 px wide: no sideways scrolling, no overlap, and no button, checkbox, picker, handle, pill or
   count field pushed off, covered or squeezed.
2. A **name** stays on one line and is cut at the end with "…". Whenever it can be cut, hovering shows all of it.
3. A **sentence** (notes, inherited lines, problem messages, section and panel titles) wraps. It is never cut.
4. Layout never changes data: compare, save and match on object references and real values, never on shown text.

| Place | Handling |
|---|---|
| "Opens over" chips | Chips wrap onto new lines. One chip is never wider than the line; its name is cut, with a tooltip. |
| Parent rows (object fields) | The field shrinks. Handle, pill and picker button keep their size. Tooltip: the full state name. |
| Table, Parent column | The widest column. The name is cut with a tooltip; the pill never shrinks. |
| Table, value cells | Cut inside the cell, teal tag included. Column lines stay aligned. Tooltip: the full value. A changed cell's tooltip gives both full values (this state's and the parent's) and the parent's full name. |
| Table, Input column | Map names joined with ", ", each map once, cut at the end. The tooltip lists every map, one per line. Teal is decided by which maps are on, so name length and order make no difference. |
| Input action rows | The map chip takes at most about a third of the row, is cut, and has a tooltip. The object field keeps the rest, with its picker button; its tooltip gives the full action name. |
| Inherited lines, `NOTE:` line | Wrap. With many parents they grow downward. |
| Section and panel titles | Wrap. A title never runs under the Parents count field. |
| Problem box | The message wraps. A fix button with a long name ("Add …") is cut with a tooltip and moves to its own line when narrow. "Learn more" stays visible. |

Test before calling the build done, at 275, 480, 760 and 1200 px, reading every still: a parent name of 60 or more
characters with spaces; one of 40 or more without spaces; an input action map name of 40 or more; three or more maps
in one table cell; six or more parents; and a problem message and fix button that name a long state.

## Onboarding window

Approved 2026-11-28 in Session 7A-look, owner review in the live combined prototype window (not a formal
stills-and-grill round; see that session's handoff for what this means for ticket 07). Exact values: the Lab
prototype's `Assets/GameManagementUIPrototype/UI/Onboarding.uss` and the seven `Onboarding*.uxml` files.

- **Seven pages**, Back/Next, matching GRILL-RESULTS §4 exactly: Welcome, Settings folder, Which menus?, How to set
  up each menu, Input, Ready, All set. Step label reads "Step N of 7" on every page, including the two that carry no
  box index (see below).
- **Page body is capped at 640 px and centered**, with 6% side padding inside that cap. This never changes with
  window width; only the two separators (below) break out of it.
- **Both separators (the one under the pill, and the footer's top border line) span the full window width**, however
  wide the window is resized, in a dimmed white (`rgba(255,255,255,0.35)`, not solid white or the default border
  colour). The footer's own buttons row stays capped at 640 px inside that full-width line — implemented as an outer
  full-width `.gm-footer` (border + padding) holding an inner capped `.gm-footer-inner` (the actual row).
- **Footer**: `padding-top: 22px` above the button row, so the buttons never crowd the line above them.
- **Welcome page reads much larger than the rest**: logo 325×75 (vs the normal 160×36), pill padding 5px 20/14px with
  16 px text (vs the normal 10 px), header 30 px (vs 20 px). The four terms (state, sub-state, Requester, Reactor)
  sit in a **Foldout titled "Terminology", collapsed by default**.
- **The "Game Management" pill**: a capsule with a 1 px tinted-blue outline, a tinted-blue fill, tinted-blue bold
  text, and a small flat white gear icon on the left. The icon is drawn procedurally at runtime
  (`GameManagementOnboardingPillIcon.cs`, an 18×18 texture, cached) — there is no image asset. An earlier round tried
  combining the gear with a tiny controller glyph; the owner asked for the gear alone, since the controller reads as
  noise at that size.
- **Box indexing**: every choice card gets a small numbered badge in its top-left corner (`gm-box-index`, absolute
  position, the card's padding-left makes room for it) — *except* a page with only one box on it (Settings folder,
  Input), which carries no index and keeps the card's normal padding (`gm-card-flat`). The Ready page also carries no
  per-card index; its points are numbered individually instead (see below).
- **Settings folder page**: one un-indexed card, a `Folder` text field next to a **Choose...** button
  (`EditorUtility.OpenFolderPanel`, converted to an Assets-relative path), and a live green/red status row ("Looks
  good." / "Must be inside Assets/.") that re-checks as the field is edited or browsed to.
- **How to set up each menu**: each menu's card keeps its existing Copy/Use-my-own radio row, and the Main Menu and
  Pause cards additionally carry an **"Include a Settings screen" toggle** with a note hint under it. This is new
  since the original GRILL spec (which had Pause always bring Settings with no choice) — the owner asked for it as a
  real per-menu choice, decided it belongs on this page (not "Which menus?") because this is already where every
  other per-menu setup decision lives. A subtext under any settings control on this page is prefixed **`NOTE:`** in
  bold, then the sentence — the only page this convention has been applied to so far.
- **Ready page**: numbered points (`1.`, `2.`, ...) in bold green instead of a per-row dot, roomier spacing between
  points (9 px) than within a point's own wrapped text, generated from the owner's actual choices (which menus,
  which path, which have a Settings screen) rather than static sample text.
- **All set page**: a green circle with a check mark sits beside the "All set" header. Each menu still needing setup
  (the ones set to "Use my own") gets its own card with a 3 px amber left border and a small bold amber "TO DO" tag
  above its name — so the page reads as a checklist of what is still left, not a report of what is already done. If
  every chosen menu is "copy the library's menu", one plain card says so instead.

**Open items for ticket 07 (the real build) to settle, not decided here:**
- Whether the per-menu "Include a Settings screen" toggle changes the Settings-page validator (a shared Settings
  sub-state opening over whichever menus asked for it) — this prototype only shows the resulting state list and menu
  line, never the real catalog or validator.
- The library ships `StartMenu.prefab`, `Pause Menu.prefab`, `Game Over Menu .prefab` (note the trailing space in
  the asset name) and `Game Win Menu.prefab` in `Modules/GameManagement/Runtime/MenuPrefabs/` — GRILL-RESULTS Q21's
  open fact is answered: **yes**, base prefabs already exist for every menu named in "Which menus?". There is **no**
  Settings menu prefab yet; a menu set to "Include a Settings screen" has nothing to copy for that sub-screen, so
  ticket 07 needs either a Settings menu prefab added to that folder or a decision that Settings always starts from
  "Use my own".

## Debugger overlay

Approved 2026-11-28 in Session 8A-look, owner review in chat on captured stills (not a formal grill round, like
7A-look). Exact values: the Lab prototype's `Assets/GameManagementUIPrototype/UI/DebuggerOverlay.uxml` and
`Debugger.uss`.

- Fixed-width panel, top-right, semi-opaque dark background, independent of Game view size: `GAME STATE`, `EFFECTS`,
  `LAST REFUSAL` (shown only when there is one), `JOURNAL`, matching GRILL-RESULTS §5.
- **Per-state colour dot**: a small 7 px dot, shared between the stack row and the journal, so the same state reads
  the same colour everywhere on the overlay. The colour comes from a small fixed palette (`gm-state-dot-0` through
  `gm-state-dot-5`); nothing about the palette or the mechanism names a game or a specific state. A real build picks
  the class deterministically from the state's identity, so any game's states get stable, consistent colours with no
  per-game configuration.
- **Journal, grouped and indexed**: lines that share one timestamp (for example Entered + Applied from the same
  request) collapse into one numbered group instead of repeating the time on separate rows. Each group has:
  - a small numbered index badge (1 = most recent, counting down), grey, in the same style as the state-list row
    badges;
  - the time, shown once;
  - one line per state involved in that transition, each with its colour dot, the state name, and its event icon(s).
  - A `newest first` label sits at the top right of the `JOURNAL` title, matching the Game State Debugger window's
    own wording, since both order newest first.
- **Event icons**, shared with the Game State Debugger window (`gm-event-icon` plus `-entered` / `-applied` /
  `-left` / `-refused`): ▶ entered (green), ◆ applied (blue), ◀ left (grey), ! refused (yellow, bold). A refusal in
  the journal shows only the icon and "Refused"; its full reason stays in the `LAST REFUSAL` block above it, so it is
  never shown twice.
- Unchanged from GRILL-RESULTS §5: the stack with suspended states dimmed, `EFFECTS` as stacked key/value rows,
  `LAST REFUSAL` as an icon plus one sentence, the `F8 hide` key hint, unscaled time, top-right placement.

## How a look gets approved

Capture the window at 480, 760 and 1200 px with `Sessions/Session5/capture-tools/` (never `capture editor_window`),
read every still, measure alignment in pixels when it matters, close other windows that could cover the grab
(window 02, the Start menu), then show the owner. Interactions a still cannot show are listed for the owner to try.
