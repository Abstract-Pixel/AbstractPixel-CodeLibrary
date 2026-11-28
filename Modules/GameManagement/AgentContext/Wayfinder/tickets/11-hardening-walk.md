# 11 Hardening walk

Type: implement. Blocked by: 10. Budget: `plan: gm-finish, step 11`.

## Goal

Find and fix what the per-piece checks missed, the way the scene system's hardening did (`SceneSystemHardeningTests`).
Earlier tickets do only a basic real-use check (CODING_STANDARDS.md); deliberate edge-case hunting happens here.

## Work

- Every developer workflow through the real buttons and menus (settings page, inspector, drawer, onboarding, debugger),
  never the method behind them.
- Every player flow in Sample 01 and Rally with keys, gamepad and mouse, and all nine transition break scenarios in
  `../../PLAN-OVERVIEW.md`.
- Unity 6.3 (Rally), Unity 6.7 compile, Unity 7 (Lab): clean compile and full tests.
- Domain reload off: statics reset, and a second Play session behaves like the first.

## Done and verified

- Every bug found gets a failing test first, then the fix.
- The handoff lists what was walked and the result of each.

## From ticket 04

- Unity 6.7.0b4 compiles the library with 0 errors but 25 warnings, none in GameManagement: UAC1002 x10, UAC1009 x2, UAC1015 x4 (serialization rules; check saved data before changing Settings types), CS0618 x8 (deprecated Find calls; keep 6.3 compiling), CS0414 x1. Clear them or record why not.
- Without BroAudio the GameManagement and Settings assemblies are skipped whole, and their samples and tests then fail with CS0246. The library CLAUDE.md asks for a clean compile plus one error naming the missing package.
- Detail: `../../Sessions/04-stability-gate/HANDOFF.md`.

## From ticket 05

- Not verified (owner dropped them): + menu (both entries, cancel on save dialog), - button, hover + sub-state, each problem kind's fix button with Undo/redo, no-asset view and Create settings, Fix now, Move..., input source warnings, pre-build check in a real build, stills at 480/1200 with problem rows, the 'no listed parent' group and its indent, long names, many states, read-only package asset, Scene System page look after the EditorUI move, drops on empty list / Sub-states tab / sub-state group, reorder of two sub-states in one group, flicker sweep, Unity 6.7.
- The 11 SceneManagement* failures were judged pre-existing by cause only; no baseline run.
- Added after the owner's two fixes: New state through the real + menu and the hover + (only the save window's folder was checked, by calling the code); the empty-row rank wording on the page; settings inside a package being editable (owner undecided, see the 05 handoff).
- Scripted UI checks: turn on `set_auto_tick` first, or delayed page rebuilds never run.
- Detail: `../../Sessions/05-settings-page-build/HANDOFF.md`.

## From ticket 06

By mouse: drag-reorder parents and input actions, drag and drop onto fields, the object picker. Also: the table after
renaming a state or input map; Project Settings keeping focus while typing with its new asset watcher; "Leave on scene
change" is cut below about 375 px. Detail: `../../Sessions/06-state-inspector-build/HANDOFF.md`.

## From ticket 06 (S0 close-out)

"Leave on scene change" now wraps (fixed in S0, still seen at 275 px only). Not done, no verifier, owner skipped: Project
Settings focus while typing under real input (the page has no editable text field, so the risk looks nil); narrow-table
"Locked Hidden" wrap re-capture in the Lab; drawer stills re-captured in both Editors; Unity 6.7; the table after a real
rename in the Project window (state rename checked by script, input map rename only showed "All off" because the probe's
JSON edit broke the reference).

## From the mega run (tickets 06-09)

Checks no unattended run can do (physical mouse drag and drop and object picker, physical gamepad, rumble, real
Steam, hearing audio, Unity 6.7) land here. Each step lists its own under "Not done / not verified" in
`../../Sessions/<ticket id>/HANDOFF.md`; the final list is `../../Sessions/mega-07-09/FINAL-REPORT.md`. Step S0 is
told to fix the 375 px cut above and to check the table after renames and Settings focus: read its handoff for the
result. Plan: [../mega-07-09/MEGA-TICKET.md](../mega-07-09/MEGA-TICKET.md).

