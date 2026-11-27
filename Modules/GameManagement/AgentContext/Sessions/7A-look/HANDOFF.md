# Session 7A-look handoff: onboarding window look, owner-approved

`plan: gm-finish, step 1`. Written 2026-11-28. Read this file first to start ticket 02 (8A-look).

## 1. What happened, and how this session differed from the usual workflow

Ticket 01 asked to combine prototype windows 06-08 (Which menus?, Setup paths, Ready) into one navigable window and
get the look called final. This ran as a plain chat session, not the full look-first grill: the owner reviewed the
window live in the Unity Editor (not from captured stills shown in a diff round), and iterated with short, direct
feedback across two follow-up passes rather than numbered grill questions. The agent still captured and read its own
stills before and after each change, same as the formal workflow; it just didn't post them to the owner. The owner
then said the session is finalized in chat, which stands in for "the owner says final" for this ticket.

Net effect: **the look is owner-approved and is now the spec** (recorded in `UI-STYLE.md`, "Onboarding window"
section), but it was never put through the Session5-style numbered Q&A, so there is no GRILL-RESULTS-style record of
*why* each choice was made beyond what is in this handoff and the UI-STYLE.md entry. If a future session needs that
level of detail, ask the owner rather than assuming more was decided.

## 2. What exists

| Thing | Where | State |
|---|---|---|
| Combined onboarding window | Lab project: `Assets/GameManagementUIPrototype/Editor/GameManagementOnboardingCombinedPrototypeWindow.cs`. Menu: `Tools/External Project Tools/Game Management/UI Prototype/00 Onboarding, Combined` | Owner-approved look, all 7 pages, Back/Next working |
| Per-page UXML/USS | `Assets/GameManagementUIPrototype/UI/Onboarding*.uxml` (7 files) and `Onboarding.uss` | Owner-approved; see UI-STYLE.md for the exact rules |
| In-memory onboarding state | `Assets/GameManagementUIPrototype/Editor/GameManagementOnboardingPrototypeState.cs` | Prototype-only choice tracking (which menus, which path, which have Settings, folder path, input source) so the Ready/All-set pages reflect earlier choices. Not real data; resets each time the window opens. |
| Pill icon | `Assets/GameManagementUIPrototype/Editor/GameManagementOnboardingPillIcon.cs` | Procedurally-drawn 18×18 gear texture, cached. No image asset. |
| Approved look | `../../UI-STYLE.md`, "Onboarding window" section | Source of truth for exact values |
| Stills | `Sessions/7A-look/stills/` (one per page, 480-1000 px mixed, each already read by the agent) | Final round only; earlier rounds' stills were not kept (chat session, not committed to the Lab anyway) |
| Original 06-08 standalone windows | Still exist, unchanged, under the same `UI Prototype/` menu, `06`/`07`/`08` | Untouched; the combined window is additive |
| Prototype folder | Lab `Assets/GameManagementUIPrototype/` | **Untracked, never commit, never delete** (per ticket 01 and the library's own rule) |

Paths without a project are relative to `Modules/GameManagement/` in the library.

## 3. The owner's choices, in order (for ticket 07 to build from)

1. **7 pages, not 3.** Welcome, Settings folder, Which menus?, How to set up each menu, Input, Ready, All set — the
   full GRILL-RESULTS §4 list, not just the three prototype windows the ticket named to start from.
2. **A new per-menu choice**: "Include a Settings screen" (Main Menu, Pause), on the "How to set up each menu" page,
   not "Which menus?" — the owner's own call once asked to choose, reasoning: that page already holds every other
   per-menu setup decision.
3. Full-width, dimmed-white (not solid) separators under the pill and above the footer buttons, breaking out of the
   640 px content cap that everything else stays inside.
4. Welcome page deliberately oversized (logo, pill, header) relative to every other page.
5. Numbered box indexing everywhere **except** a page with only one box (Settings folder, Input) and the Ready page
   (numbered points instead of a numbered card).
6. `NOTE:` in bold caps before any "settings" subtext, scoped to the "How to set up each menu" page only — the owner
   confirmed this scope when asked.
7. All set page needed to visually read as "things left to do", not "things already done" — green tick by the title,
   amber "TO DO" tag per remaining menu.
8. Pill icon: gear only. A first pass drew a gear overlapping a small controller glyph; the owner rejected it as
   noise at 18 px and asked for the gear alone.

## 4. GRILL-RESULTS Q21, resolved

The library already ships base menu prefabs to copy: `Modules/GameManagement/Runtime/MenuPrefabs/StartMenu.prefab`,
`Pause Menu.prefab`, `Game Over Menu .prefab` (trailing space in the asset name — pre-existing, not introduced here),
`Game Win Menu.prefab`. **There is no Settings menu prefab.** Since item 2 above makes "Settings screen" a real,
ownerable choice, ticket 07 needs one of: add a Settings menu prefab to that folder, or decide the Settings sub-state
always starts from "Use my own" until one exists.

## 5. Not done / left for ticket 07

- No real `GameManagementSettings`/`GameStateCatalog` data is touched anywhere in this prototype; Ready and All set
  are built from the prototype's own in-memory `GameManagementOnboardingPrototypeState`, not the real validator.
- The Setup Paths page's three rows (Main Menu, Pause, Game Over) are still hardcoded; they do not yet filter to only
  the menus ticked on the Which-menus page (Game Win was never added as a fourth row either). Flagged, not fixed —
  out of this ticket's asked-for scope.
- "Running it again" (opens at Which menus? with existing menus ticked and locked) is not prototyped.
- Long-name handling (per UI-STYLE.md's state-inspector rules) was not tested on any onboarding page.

## 6. Prompt for ticket 02 (8A-look)

> You are working in the AbstractPixelLibrary game management module (library at
> `Abstract_Pixel_Lab/LocalPackages/AbstractPixelLibrary`, branch `main`; prototypes in the Lab project).
> plan: gm-finish, step 2. Read `Modules/GameManagement/AgentContext/Wayfinder/tickets/02-debugger-overlay-look.md`
> first, then `Sessions/Session5/GRILL-RESULTS.md` §5 and `UI-STYLE.md`.
>
> 1. Open prototype window 09 (the in-game debugger overlay) in the Lab, capture it at a few realistic Game view
>    sizes with a long stack and long state names, read the stills yourself, and show the owner (in whatever way
>    they want to review it — Session 7A-look showed it live in the Editor rather than posting stills, and that
>    worked fine).
> 2. Iterate on the look until the owner says it's final.
> 3. Add the approved look to `UI-STYLE.md` as a new "Debugger overlay" section, following the existing sections'
>    format (what Session 7A-look's "Onboarding window" section looks like is a reasonable template).
> 4. Finish: stills in `Sessions/8A-look/stills/`, handoff in `Sessions/8A-look/HANDOFF.md` with ticket 03's prompt.
>
> Rules: stay inside Abstract_Pixel_Lab and your scratchpad; the Lab prototype folder is untracked, never commit,
> never delete; never use `capture editor_window` on the Lab (crashes the Unity 7 alpha Editor) — use
> `Sessions/Session5/capture-tools/` instead.

## 7. Not yet committed

Nothing from this session has been committed. `UI-STYLE.md` and this handoff are library files and would normally be
staged and committed per the library's own rule ("one commit per task... commit only with the owner's OK"); that OK
was not asked for or given this session, so they are left as uncommitted working-tree changes.
