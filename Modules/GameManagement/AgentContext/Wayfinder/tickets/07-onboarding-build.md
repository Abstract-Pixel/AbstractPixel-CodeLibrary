# 07 Onboarding window build (7A-build)

Type: implement. Blocked by: 06. Budget: `plan: gm-finish, step 7`.

## Goal

A new game can be set up from the onboarding window: both paths ("Copy the library's menu", "Use my own") and
running it again to add a skipped menu.

## Read first

- Ticket 01's handoff and stills, `../../Sessions/Session5/GRILL-RESULTS.md` §4, `../../UI-STYLE.md`.

## Done and verified

- In a project with no settings asset: run the window, choose each menu with both paths, enter Play and use every
  menu it made with keys, gamepad and mouse. Run it again and add a skipped menu.
- It opens once per Editor session while no settings asset exists; never in batch mode, Play mode or tests.
- Stills at three widths, each read; module tests pass.

## From ticket 05

- Still to make: `SystemPill` in `AbstractPixel.Core.Editor.UI` and the scene setup window's "Scene System" pill (GRILL Q19).
- The page does not show "Set up states..." yet; add it to the empty list, opening at "Which menus?".
- Reuse `GameManagementSettingsAssets.Create(folder)` and `EnsureFolder`.
- `Find` returns any settings asset, including a sample's inside a package (the Lab does). "No settings asset exists" would
  then be false and the window would not open. Settle with the owner; see the 05 handoff, "Open: sample settings are editable".

## Mega run: step S1

Added 2026-11-28 for [../mega-07-09/MEGA-TICKET.md](../mega-07-09/MEGA-TICKET.md). Where this section differs from
the text above, this section wins. Session folder: `../../Sessions/07-onboarding-build/`.

**Also read:** `../../Sessions/7A-look/HANDOFF.md`, UI-STYLE.md "Onboarding window", the 05 handoff "Open: sample
settings are editable". The 7A-look stills and the prototype USS are the look.

**Decided in advance (do not ask; list these under "Decisions made without you")**
- **Package settings:** a settings asset outside `Assets/` is not this project's. "No settings asset exists" means
  none under `Assets/`. The Settings page shows a package one read-only with a warning and **Create settings**; no
  edit, add or Move may write into a package. In the editor a project asset always wins over a package one, and
  samples still run as before.
- **Settings screen:** add one graphics-free uGUI + TextMeshPro Settings menu prefab (title and Back, in the style
  of the other base menus) to `Runtime/MenuPrefabs/`. One Settings sub-state is made, with every menu that ticked
  "Include a Settings screen" as a parent; none ticked means no Settings state. "Use my own" works for it as for others.
- "How to set up each menu" lists exactly the ticked menus, Game Win included.
- Existing files are never overwritten: a name clash gets a new unique name and is reported on the Ready page.
- Keep the library's prefab names, including the trailing space in `Game Over Menu .prefab`.

**Must-have, besides "Done and verified" and "From ticket 05"**
1. Every path in a Lab state with no project settings: all menus by copy, all by "Use my own" (with and without a
   prefab), a mix, none besides Play, cancel halfway, run again to add a skipped menu. Each trial is removed cleanly;
   afterwards the Lab (Preloaded Assets included) is as it started.
2. After each full creation, the Settings page and the state inspector show no problem.
3. Enter Play on what it made: every menu used with keys, gamepad and mouse, each separately.
4. Auto-open: once per Editor session, "Don't show again" per project, re-enabled from the Setup card; never in
   batch mode, Play mode or tests.
5. Long folder path and long prefab names at all widths.
6. Rally: R1, R4 (with the new pill), R5. Never press Finish in Rally.

