# 09 Game State Debugger window build (8B-build)

Type: implement. Blocked by: 08. Budget: `plan: gm-finish, step 9`.

## Read first

- Ticket 03's handoff and stills, `../../Sessions/Session5/GRILL-RESULTS.md` §5.

## Done and verified

- The window matches ticket 03's stills: journal newest first, Refusals only filter, row details with changed-only lines.
- Used in Play mode on Sample 01 through every flow; an empty journal shows "No transitions yet."
- Module tests pass; clean compile in both Editors.

## Mega run: step S3

Added 2026-11-28 for [../mega-07-09/MEGA-TICKET.md](../mega-07-09/MEGA-TICKET.md). Where this section differs from
the text above, this section wins. Session folder: `../../Sessions/09-debugger-window-build/`.

**Also read:** `../../Sessions/8B-look/HANDOFF.md`, UI-STYLE.md "Game State Debugger window" and its open items.

**Decided in advance (do not ask)**
- The approved look is newer than "Refusals only filter" above: build its multi-select outcome filter (Refused
  alone gives refusals only), Newest/Oldest toggle, stable row index and resizable footer.
- The footer shows the selected row's real changed-only data from the effects and stacks before and after. Nothing
  selected, or nothing changed, says so in plain words. No placeholder data remains.
- Click-to-navigate goes to the recorded state asset and requester object. A requester that is destroyed, not an
  object, or from an unloaded scene is greyed with a tooltip saying why. Holding these references must not keep
  objects alive or add per-frame allocations; a release build without the opt-in pays nothing new.
- Long names: one line, cut with "…", full name on hover. Colours match the overlay's.
- The side-by-side effect compare is a nice-to-have.

**Must-have, besides "Done and verified"**
1. The menu entry opens a dockable window. It works opened before, during and after Play, across a recompile, and
   with domain reload off over two Play sessions.
2. A full journal at the configured size (and the minimum, 8) updates live without lag or lost selection.
3. Stills at 480, 760 and 1200 px: empty, full, filtered to nothing, row selected, long names.
4. Runtime files changed: parity scenarios in Rally. Rally: R1, R6, R7, R8 on a real Rally run.
