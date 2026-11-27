# 10 Play-mode watchdog (was 6C)

Type: implement. Blocked by: 09 (after every UI build, owner 2026-11-27). Budget: `plan: gm-finish, step 10`.

## Goal

Warn, never change anything, when the applied effects differ from the top state's wanted effects, or when no state
is active and time stays 0 outside a scene change. See `../../DESIGN.md` "Play-mode watchdog" and
`../../DECISIONS.md` "No state active".

## Settle with the owner first

How long "a few seconds" is; how close two time scales count as equal; how eases, execution delays and scripts that
set time on purpose are treated. A watchdog that warns during normal play is worse than none.

## Done and verified

- Tests first for both rules, and for staying silent during eases, delays and scene changes.
- Play mode, real input, Sample 01 and Rally normal flows: no warning. Each problem caused on purpose gives exactly one
  warning that starts with `[Game management]` and says what, where and how to fix it. Resets with domain reload off.
