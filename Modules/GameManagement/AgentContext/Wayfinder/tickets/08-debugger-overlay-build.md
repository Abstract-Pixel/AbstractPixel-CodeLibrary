# 08 Debugger overlay build (8A-build)

Type: implement. Blocked by: 07. Budget: `plan: gm-finish, step 8`.

## Read first

- Ticket 02's handoff and stills, `../../Sessions/Session5/GRILL-RESULTS.md` §5.

## Done and verified

- The overlay matches ticket 02's stills, works at time 0, and never takes input or the cursor.
- Shown and hidden by the Debugger card's InputActionReference, and by F8 in the Editor.
- Editor and Development Builds only; nothing is built into a release without the opt-in (checked in a real build).
- Sample 01 and Rally played through with it on: no errors, parity unchanged.

## Mega run: step S2

Added 2026-11-28 for [../mega-07-09/MEGA-TICKET.md](../mega-07-09/MEGA-TICKET.md). Where this section differs from
the text above, this section wins. Session folder: `../../Sessions/08-debugger-overlay-build/`.

**Also read:** `../../Sessions/8A-look/HANDOFF.md`, UI-STYLE.md "Debugger overlay".

**Decided in advance (do not ask)**
- **State colour:** deterministic from the real state, the same across Play sessions and builds, and the same in the
  stack, the journal and (ticket 09) the window. With up to six states in a catalog, all six differ; past six,
  colours repeat. No per-game setting and no string typed by a developer. Tests first.
- **Long names:** one line, cut with "…"; the panel keeps its fixed width; icons stay visible. No tooltip (the
  overlay takes no input).
- "Built into a release only with the opt-in" covers code and UI assets.
- The two Debugger card fields, shown disabled today, become live.

**Must-have, besides "Done and verified"**
1. The overlay takes no input: with it shown, mouse clicks, gamepad navigation and key presses behave exactly as with
   it hidden, in every state.
2. The toggle action works in every state, including ones whose input effect disables other maps, and at time 0.
3. Empty journal, no settings, no state active, a scene change, domain reload off with two Play sessions: no error,
   never two overlays.
4. Sample 01 (Lab) and Rally played through with it on; all parity scenarios in Rally; Sample 01's flow test in the Lab.
5. **The Rally dogfood:** [../mega-07-09/RALLY-DOGFOOD.md](../mega-07-09/RALLY-DOGFOOD.md), "One-level Mono player
   builds": dev build flows P1-P8 and release checks B1-B2, plus R1, R6, R7. This is how "checked in a real build"
   is met. The verifier drives the built dev player itself (at least P1, P4, P6, P7) and repeats the B1-B2 searches
   on the built files; it does not rebuild unless a later fix changed runtime code or the build probe.
