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
