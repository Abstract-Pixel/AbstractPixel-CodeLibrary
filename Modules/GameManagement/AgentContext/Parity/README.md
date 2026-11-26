# Parity recordings

`baseline/` holds how Rally behaved on the **old** game management system (Session 1, 2026-11-26, Unity 6000.3.15f1,
virtual gamepad, throwaway save, Steam writes blocked). Session 4 records the new system and compares.

## Record

From the Rally project folder, with the Editor open and no other agent driving it:

```bash
bash AgentScripts/Probes/run_scenario.sh parity_menu
```

Scenarios: `parity_menu`, `parity_menu_spam`, `parity_pause`, `parity_pause_quit`, `parity_gameover`,
`parity_gameover_early_reload`, `parity_loading_pause_spam`, `parity_win`, `parity_lone_level`.
Each run writes `Temp/ProbeRuns/parity/<name>.json`. Copy it over `baseline/` only to replace the old-system baseline.
The recorder is `AgentScripts/Probes/Runner/ParityRecorder.cs`; a `parityStart` step turns it on in any scenario.
It only reads the game.

After a batch, check `git status`: Play mode can rewrite `GameManagment.prefab`, font assets and
`SaveSystemConfigSO.asset`. **Check before running `revert_probe_churn.sh`**: it runs `git checkout` on files that may
hold the owner's uncommitted work.

Two probe-only shortcuts: `parity_gameover` and `parity_win` set time scale 6 to shorten the wait for the bomb (back
to 1 before it ends), and `parity_gameover_early_reload` uses the `reloadLevel` step, which calls Retry's API.

## File shape

- `snapshots`: one per scenario step. Active states, top state and whether it is applied, time scale, cursor, visible
  state menus, level select or main menu panel, HUD content, enabled input maps, ducking state, bomb, car.
- `events`: every registered, unregistered, restored and applied event, with frame and time scale.
- `frames`: every rendered frame while a scene group loads, the screen is covered, a state waits for its delay, or time
  eases. Identical consecutive frames share one entry (`frameCount`).

## Compare

There is no compare script yet. Both lists have one entry per line, so compare by hand: remove the values that depend
on frame rate (`frame`, `firstFrame`, `frameCount`, `bomb`, `car`), drop repeated lines, then diff.

```bash
strip() { sed -E 's/"(frame|firstFrame|frameCount|bomb)": [-0-9.]+,? ?//g; s/"car": \{[^}]*\}, //' "$1" | uniq; }
diff <(strip "LocalPackages/AbstractPixelLibrary/Modules/GameManagement/AgentContext/Parity/baseline/parity_pause.json") <(strip Temp/ProbeRuns/parity/parity_pause.json)
```

In `frames`, also check the pass rule: while `carCanMove` is `no`, the new level's bomb does not count down and the car
does not move.

The new system merges the two Settings states and keeps parents suspended, so Session 4 expects those differences (see
the findings in [../FLOWS.md](../FLOWS.md)). The recorder reads the old API in `ReadSnapshot`, `ReadActiveStates` and
the four event handlers; Session 4 points those at `GameStates` and keeps the JSON shape.

## Not covered

Mouse and keyboard cursor values (the probes use a gamepad), audio output, death by health, switching between
controller and mouse, and the "set time to 1" no-state option (it does not exist yet).
