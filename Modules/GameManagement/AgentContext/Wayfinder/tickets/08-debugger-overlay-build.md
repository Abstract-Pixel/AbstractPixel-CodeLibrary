# 08 Debugger overlay build (8A-build)

Type: implement. Blocked by: 07. Budget: `plan: gm-finish, step 8`.

## Read first

- Ticket 02's handoff and stills, `../../Sessions/Session5/GRILL-RESULTS.md` §5.

## Done and verified

- The overlay matches ticket 02's stills, works at time 0, and never takes input or the cursor.
- Shown and hidden by the Debugger card's InputActionReference, and by F8 in the Editor.
- Editor and Development Builds only; nothing is built into a release without the opt-in (checked in a real build).
- Sample 01 and Rally played through with it on: no errors, parity unchanged.
