# Session 3 overview: adapters, conditions, reactors, input source

`plan: gm-commercial, step 3`. Budget: **10%** of the 5-hour window, wrap reserve 2%. Kind: implement.
"Finished" is defined in [../../DEFINITION-OF-DONE.md](../../DEFINITION-OF-DONE.md).

## Goal

Connect Session 2's plain C# machine (`Runtime/Machine/`) to Unity: the real effect sink, the request adapters
(conditions), the state watch and library reactors, the input source seam and the asset migration tool. Rally keeps
using the old system: the old components keep working until Session 4 switches Rally over.

## Starting point (from Session 2)

- `GameStateMachine(settings, clock, sink)` with `Request`, `Leave`, `IsActive`, `Top`, `Stack`, `IsInDelay`,
  `SkipDelay`, `Tick`, `LeaveOnSceneChange`, `Changed` (a `TransitionRecord` for each step, refusals included),
  `Applied` (effects applied) and a `Journal`. Static facade `GameStates`. There is no driver yet: nothing calls `Tick`
  or `LeaveOnSceneChange`.
- Effects are `GameEffects`, where null means "keep". The sink receives only what the stack wants; time eases from its
  value at apply time over `TimeScaleEaseSeconds`.
- New `StateSO` fields: `Parents` and `Effects` (`StateEffectSettings`, each effect `Own` or `SameAsParent`). The
  machine reuses `StateExecutionDelay` and `DisableStateOnSceneChange` and ignores the old priority, sub-state, time,
  cursor and revert fields. `GameStateCatalog` (ordered list) and `GameManagementSettings` (catalog, no-state time
  option, journal size) are new assets, kept in Preloaded Assets by `Editor/GameManagementSettingsAssets.cs`.
- Known Rally bug for the cursor work: after unpausing, the cursor stays visible, because no state is active after
  unpause today. With Pause as a sub-state of Play, the machine re-applies Play's cursor on unpause (a test covers it).
  The Unity sink must write it through `GameCursorController`.

## Tasks

| # | Task | Share | Must-have | Nice-to-have |
|---|---|---|---|---|
| 1 | Unity effect sink and driver | 3% | Sink writes `Time.timeScale` with an unscaled ease from the current value, the cursor through `GameCursorController`, HUD and input maps; a driver calls `Tick` each frame and `LeaveOnSceneChange` on scene changes and gives the facade a machine with the real sink | Ease curve on `StateEffectSettings` |
| 2 | Input source seam | 1.5% | Project-wide actions, a chosen `InputActionAsset`, a custom source; none set means input is left alone (DESIGN.md "Input without a global class") | Map dropdown by ID |
| 3 | Request adapters | 2% | Enter on scene start, input toggle with optional gate, back/cancel, event, poll, "when X leaves go to Y", a requester component for UnityEvent buttons | |
| 4 | State watch and reactors | 1.5% | One composed state-watch helper (Entered, Applied, Left, sync on enable); menu, HUD toggle and audio reactors on it | |
| 5 | Asset migration tool | 1% | Editor tool that fills the new fields from the old ones (priority order to catalog with ties reported, time and cursor fields to effects, sub-states to parents); dry run first; tested on copies, never on Rally's assets | |
| 6 | Handoff and next session | 1% | Everything in DEFINITION-OF-DONE.md's session list | |

## Done when

DEFINITION-OF-DONE.md holds for every task and for the session. Rally behaves as before (parity runs if any old
runtime file is touched).
