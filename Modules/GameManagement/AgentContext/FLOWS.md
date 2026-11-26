# Rally flows on the current game management system

Written in Session 1 (2026-11-26) from the code, the serialized scenes and prefabs, and the recordings in
[Parity/baseline/](Parity/baseline/). Where this file and the older documents disagree, the recordings are right.

## How to read the tables

- **State names** drop the `StateSO` suffix. "none" means no state is active.
- **Time** is `Time.timeScale`. **Input** is the maps enabled on `GlobalInput.PlayerInputActions`.
- **Cursor.** With a gamepad, `GameCursorController` always hides and locks it (that is what the recordings show,
  because the probes drive a virtual gamepad). **With mouse and keyboard** it follows the top state once that state's
  configurations are applied: "free" = visible, not locked; "locked" = hidden, locked. With no state active it keeps
  the last value. Mouse values below come from the state assets and the code, **not from a recording**.
- **HUD** is the one `GameStateUIToggle` (GameBootScene, content object `Container`).
- **Audio** is "trigger sounds" (`GameStateAudioTrigger`) and "ducking" (`StateAudioVolumeController`:
  while ducked, Music x0.1, SFX, Car and Ambience x0, 0.5 s fade, in Pause, PauseSettings, GameOver and GameWin).
- **Rec** names the scenario that reproduces the row (`bash AgentScripts/Probes/run_scenario.sh <name>`).
  "code" means read from code and assets only.

## Flows

| # | Flow | Trigger | State before > after | Time | Mouse cursor | Menu shown | HUD | Input | Audio | Rec |
|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Boot into main menu | `StartState` component, activate on start (MainMenu scene) | none > Start | 1 | free | Start (`MainMenuRoot`) | no toggle loaded | UI+Player | no ducking | parity_menu |
| 2 | Settings from main menu | `Button_SETTINGS` OnClick > `ActivateState` on StartSettings | Start > StartSettings (Start is unregistered) | 1 | free | StartSettings only | no toggle | UI+Player | no ducking | parity_menu |
| 3 | Back from menu settings | Back button OnClick, or Cancel (`CancelEventCondition`, East / Escape) | StartSettings > Start (events: registered, applied, then restored) | 1 | free | Start | no toggle | UI+Player | no ducking | parity_menu |
| 4 | Main menu to level select | Play button; `MainMenuStateTracker.IsInMainMenu = false` (not a game state) | Start > Start | 1 | free | none of the state menus | no toggle | UI+Player | no ducking | parity_menu |
| 5 | Level select back to main menu | East tapped twice (`UI/GoToMainMenu`, MultiTap) or H, or the back button | Start > Start | 1 | free | Start | no toggle | UI+Player | no ducking | parity_menu |
| 6 | Level select > menu > settings | Rows 5 then 2 | Start > StartSettings | 1 | free | StartSettings | no toggle | UI+Player | no ducking | parity_menu, parity_menu_spam |
| 7 | Start a level (loading screen) | Level select confirm; scene group change | Start > (in the `ManagerBootScene` frame) none > Play. Start stays active under the whole loading screen and leaves when the active scene changes. The `PlayState` in GameManagment.prefab reactivates one frame later; levels 2-4 also have their own | 1 throughout | locked | none | `Container` shown once loaded | UI+Player | no ducking | every level scenario |
| 8 | Play from a lone level in the Editor | `PlayState` activate on start | none > Play | 1 | locked | none | `Container` hidden on the first frames, then shown | UI+Player | no ducking | parity_lone_level |
| 9 | Pause | `Player/Pause` action (Start / Escape) via `RallyPauseCondition`; only when the screen is Visible and no scene group is loading. The action is read from the **project asset reference** in `PauseStateSO`, not from the `GlobalInput` copy | Play > Pause (Play is **evicted**, not suspended) | 0 at once | free | Pause | hidden | UI (Player off) | Pause + Car Blinker sounds; ducked | parity_pause |
| 10 | Unpause: pause button or Resume | Pause action again, or Resume button > `DeactivateState` | Pause > **none**. Nothing reactivates Play | 1 (Pause reverts the saved value) | **keeps the last value** (see finding 1) | none | **`Container` stays hidden** (finding 2) | UI+Player (enabler re-enables on exit) | UnPause sound; ducking off | parity_pause |
| 11 | Settings from pause | Pause menu button > `ActivateState` on PauseSettings | Pause > PauseSettings (Pause is unregistered) | 0 (goes through 1 inside the same call) | free | PauseSettings | hidden | UI | UnPause sound plays (finding 3); stays ducked | parity_pause |
| 12 | Back from pause settings | Back button or Cancel | PauseSettings > Pause (restored) | 0 | free | Pause | hidden | UI | Pause sounds play again | parity_pause |
| 13 | Pause again after settings | Pause action | Pause > none, as row 10 | 1 | keeps last | none | hidden | UI+Player | UnPause; ducking off | parity_pause |
| 14 | Restart from pause menu | `ReloadActiveScenesWithEffects` + `ResetManager` | Pause > none > Play. Pause leaves at the active scene change and **reverts time to 1** while the old level is still loaded | 0, then 1 | keeps, then locked | none | `Container` hidden, shown after the reveal | UI+Player | ducking off | parity_pause_quit |
| 15 | Quit to main menu from pause | `TransitionToSceneWithEffects` + `ResetManager` | Pause > none > Play (one scene, about 6 frames) > Start | 0, then 1 | free | Start | no toggle | UI+Player | ducking off | parity_pause_quit |
| 16 | Game over: bomb runs out | `GameOverCondition` on `Car Prefab` (`BombTimer.OnTimerFinished`) | Play > GameOver, in control at once, effects 1.4 s later | `CarDeathSequence` sets 0.6 on register, then 1 s unscaled ease to 0 after the delay | free after the delay | GameOver, **only once applied** (reveal timing 1) | hidden at once | UI at once | explosion sound; ducked at once | parity_gameover |
| 17 | Game over: death by health | Same condition (`Health.OnDeath`) | as row 16 | as 16 | as 16 | as 16 | as 16 | as 16 | as 16 | code |
| 18 | Death sequence skip | `CarDeathSkipInput`: Submit or Click, 0.5 s or more after death > `SkipExecutionDelay` | GameOver (pending) > GameOver applied | ease starts at once | free at once | GameOver at once | hidden | UI | unchanged | parity_gameover |
| 19 | Pause during game over (delay or menu) | Pause action | refused by priority, silently | unchanged | unchanged | unchanged | unchanged | unchanged | unchanged | parity_gameover |
| 20 | Retry from game over | `ReloadActiveScenesWithEffects` + `ResetManager` | GameOver > none > Play | **stays 0** until Play registers behind the loading screen | free, then locked | none | shown after the reveal | UI+Player | ducking off | parity_gameover |
| 21 | Main menu from game over | `TransitionToSceneWithEffects` + `ResetManager` | GameOver > none > Play > Start | 0, then 1 | free | Start | no toggle | UI+Player | ducking off | parity_gameover |
| 22 | Scene change during the game over delay | any scene change in the first 1.4 s (scenario calls Retry's API) | GameOver (pending) > none > Play; GameOver never applies | **stays about 0.6** during the change, then 1 | keeps, then locked | never shown | shown after the reveal | UI+Player | ducking off | parity_gameover_early_reload |
| 23 | Game win | `GameWinCondition`: `LevelFinishTrigger.OnLevelFinished`, one frame later, ignored while GameOver is active | Play > GameWin | 0 at once | free | GameWin | hidden | **UI+Player** (no enabler for GameWin) | ducked | parity_win |
| 24 | Pause on the win menu | Pause action | refused by priority | 0 | free | GameWin | hidden | UI+Player | unchanged | parity_win |
| 25 | Next level (win to play) | `LoadNextLevel` + effects | GameWin > none > Play | 0 until Play registers behind the loading screen | free, then locked | none | shown after the reveal | UI+Player | ducking off | parity_win |
| 26 | Retry from win | `ReloadActiveScenesWithEffects` + `ResetManager` | GameWin > none > Play | as row 25 | as 25 | none | shown after the reveal | UI+Player | ducking off | parity_win |
| 27 | Play to game over (after a win) | Row 16 on a level reached from the win menu | Play > GameOver | as 16 | as 16 | GameOver | hidden | UI | ducked | parity_win |
| 28 | Pause pressed during loading | Pause action while loading | refused by `RallyPauseCondition` (`canPause`). Presses after the screen is visible do pause | 1 while loading | locked | none | as row 7 | UI+Player | none | parity_loading_pause_spam |
| 29 | Retry or Start spam during a change | repeated input | no extra state change | as row 20 | as 20 | none | as 20 | UI+Player | none | parity_gameover |
| 30 | Controller and mouse switching | `InputDeviceTracker.OnDeviceFamilyChanged`; any mouse move or click while a controller hides the cursor | no state change | unchanged | gamepad: hidden, locked. Mouse picked up: top state's cursor | unchanged | unchanged | unchanged | unchanged | code |
| 31 | Pause blur | `GameStateBlur`: depth of field while any of Pause, PauseSettings, GameOver, GameWin is **active** | follows states | - | - | - | - | - | - | code |
| 32 | Driving rumble, triggers, light bar | `CarRumbleDirector`: silent whenever the top state is not Play. **With no state (after unpause) it is not silenced** | follows states | - | - | - | - | - | - | code |
| 33 | Run timer overlay | `RunTimerUI`: hidden when time is 0 or any of its interrupting states is active | follows states and time | - | - | - | - | - | - | code |
| 34 | Finish gate sounds | `FinishGateSequence`: stops on GameOver | - | - | - | - | - | - | - | code |
| 35 | Level select tablet lighting, main menu panel | `MainMenuStateTracker` (static bool, not a game state) | none | - | - | - | - | - | - | code |

## Transition break scenarios: baseline

Only today's behaviour ("keep the last time scale") exists, so the second "no state active" option cannot be recorded
until Session 4. "Held" means `CarRelease.CarCanMove` is false.

| # | PLAN-OVERVIEW scenario | Recording | Baseline result |
|---|---|---|---|
| 1 | Game Over then Retry | parity_gameover | Time 0 until Play registers, then 1. Bomb stays 0, car held until the reveal. No soft-lock |
| 2 | Game Win, Next Level and Retry | parity_win | Time 0 until Play registers. Old bomb frozen, new bomb full and not ticking while held. No soft-lock |
| 3 | Pause then Quit / Restart | parity_pause_quit | **Time goes to 1 at the active scene change and the old level's bomb ticks 0.02-0.03 s** before it is destroyed |
| 4 | Main menu to a level via loading | parity_pause (first window) | Time 1. New bomb full and not ticking while held |
| 5 | Scene change during Game Over's delay or ease | parity_gameover_early_reload | Time stays about 0.6 through the change, then 1. No soft-lock |
| 6 | Pause spam during loading | parity_loading_pause_spam | Blocked during loading. A press after the reveal pauses |
| 7 | Retry spam during a change | parity_gameover | One reload; Play at the end |
| 8 | Play from a lone level | parity_lone_level | Play active from the first recorded frame |
| 9 | Level select > Main Menu > Settings, during and after the change | parity_menu_spam, parity_menu | Presses during the return raise no state events; ends in Start with the main menu shown. Settings opens normally afterwards (parity_menu) |

In every recorded change the new car was held and the new bomb did not tick until `CarCanMove` turned true.

## Findings: behaviour that looks wrong or surprising today

1. **Unpause leaves no state active** (recorded). `StateBasedActivationTrigger` is not on any scene or prefab, in the
   working tree or in `HEAD`; commit `05cc149e7` last moved it into GameManagment.prefab and it is gone now. DECISIONS
   round 3 item 1 assumes it is still in use. With a gamepad nothing looks wrong. With **mouse and keyboard the cursor
   rule says "keep the last value", which is Pause's free, visible cursor**. This was not tested with a mouse; check it by hand.
2. **The `GameStateUIToggle` content stays hidden after unpause** (recorded) because nothing registers after Pause leaves.
   Two Game View stills (level 2, before pause and after unpause) show no visible difference, so what `Container` draws
   there was not established.
3. **Settings from pause replays pause audio** (code): Pause is unregistered, so `GameStateAudioTrigger` plays UnPause,
   then Pause + Car Blinker again on return.
4. **Quit or Restart from pause lets the old level run for a frame or two** (recorded, break scenario 3).
5. **GameWin leaves the Player map on** (recorded). Pause has a gate and priority, but driving input stays enabled under the win menu.
6. **Two `PlayState` components** are alive in levels 2-4 (the level's and the prefab's), so `applied Play` is raised twice (recorded).
7. **`StateInputActionEnabler` for PauseSettings has re-enable off**, so the Player map comes back only because Pause,
   restored on the way out, re-enables it on its own exit.
8. Refused requests log nothing (code), as ANALYSIS.md says.

With the new design (Pause as a sub-state of Play), findings 1, 2 and 3 change behaviour on purpose. Session 4 should
treat them as "improves", not as parity failures, and say so in its report.

## Rally helpers (tickets 01 and 03)

| Script | What it does with game management | Future (DESIGN.md) |
|---|---|---|
| `CustomGameStateConditions/GameOverCondition` | `EventCondition`: bomb finished or player death requests GameOver | Stays in Rally as a request adapter |
| `CustomGameStateConditions/GameWinCondition` | `EventCondition`: level finished requests GameWin next frame, not during GameOver | Stays in Rally as a request adapter |
| `CustomGameStateConditions/RallyPauseCondition` | `PauseEventCondition` subclass; blocks pause while loading or covered | A gate on the library's input toggle condition |
| `CustomGameStateConditions/StateInputActionEnabler` + `StateInputMode` | Enables or disables the Player map per state | Replaced by the Input effect |
| `ProjectUtilties/StateAudioVolumeController` | Ducks audio channels per state; special-cases sub-states | Library audio reactor (nice-to-have in Session 3); else state watch |
| `MainMenuAssets/GameStateBlur` | Depth of field while listed states are active; also uses `OnStateRestored` | Stays in Rally on the state watch |
| `CarDeath/CarDeathSequence` | On GameOver registered: explosion, **writes `Time.timeScale = deathTimeScale`** | State watch. The time write breaks "one owner of time": make it GameOver's ease start value or an owner-approved request |
| `CarDeath/CarDeathSkipInput` | Finds the GameOver `GameStateComponent` with `FindObjectsByType` and calls `SkipExecutionDelay` | State watch + `GameStates.SkipDelay` |
| `RunTimer/RunTimerUI` | Polls time scale and `IsStateActive` for interrupting states | State watch or `GameStates.IsActive` |
| `ControllerFeedback/CarRumbleDirector` + `CarRumbleProfile` | Silences driving feedback when the top state is not Play; GameOver moment | State watch. Review the "no state means not silenced" rule once Play is never absent |
| `FinishGate/FinishGateSequence` | Stops sounds on GameOver | State watch |

### UnityEvent calls (Session 4 migration must rewrite all of these)

| Source prefab button | Method | Real target (set by prefab-instance override) |
|---|---|---|
| `MainMenuAssets/Button_SETTINGS.prefab` | `ActivateState` | MainMenu.unity > StartSettingsState |
| `ProjectServices/Settings/SettingsMenu.prefab` (back) | `DeactivateState` | MainMenu.unity > StartSettingsState; GameManagment.prefab > SettingsState |
| `GeneralMenuPrefabs/Pause Menu.prefab` (resume) | `DeactivateState` | GameManagment.prefab > PauseState |
| `GeneralMenuPrefabs/Pause Menu.prefab` (settings) | `ActivateState` | GameManagment.prefab > SettingsState |

The source prefabs store these calls with **no target** (`fileID: 0`); only the instance overrides point at a component.
No scene or prefab serializes `SkipExecutionDelay`.

### Module components by file

| File | Components |
|---|---|
| `ProjectServices/GameManagment/GameManagment.prefab` (instanced in ManagerBootScene) | `GameStateComponent` x5 (Play: on start and on scene change; Pause; PauseSettings; GameOver; GameWin), `GameStateMenu` x4, `StateInputActionEnabler` x4, `RallyPauseCondition`, `GameWinCondition`, `CancelEventCondition`, `GameStateAudioTrigger`, `StateAudioVolumeController`, `GameStateBlur` |
| `MainMennu/MainMenu.unity` | `GameStateComponent` x2 (Start on start; StartSettings), `GameStateMenu` x2, `CancelEventCondition` |
| `Level 2`, `level 3`, `level 4` | `GameStateComponent` (Play, on start) |
| `GameBootScene.unity` | `GameStateUIToggle`, `RunTimerUI`, `CarRumbleDirector` |
| `Car Prefab.prefab` | `GameOverCondition`, `CarDeathSequence`, `CarDeathSkipInput` |
| `LevelFinishTrigger.prefab` | `FinishGateSequence` |
| Not used anywhere | `StateBasedActivationTrigger`, `GameStateEventTrigger` |

`AgentScripts/` probes that call the old API (Session 4 updates them): `ControllerHaptics/` (HapticsFixProbes,
LightBarStatesProbe, MenuRumbleProbes, RumbleBuilder, RumbleProbes, StateGatingProbes), `Cursor/CursorTransitionTest`,
`DeathSequence/` (DeathSequenceProbe, DeathStills), `Perf/` (CameraTrace, PerfReport, ResponderCheck),
`Review/HapticsRegressionProbe`, `Splash/SplashSingletonSurvey`, `Probes/Runner/ParityRecorder` and `ConditionEvaluator`.
LSP `findReferences` was not used for this list; it comes from grep over `Assets`, the library and `AgentScripts`.
