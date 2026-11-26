# Game management today: analysis

Read on 2026-11-25. 18 C# files, about 1,400 lines, plus 7 state assets and 4 uGUI menu prefabs. No README, glossary,
tests, editor tools or samples.

## How it works

| Piece | File | Role |
|---|---|---|
| `StateSO` | Runtime/Configuration/StateSO.cs | Settings asset for one state: int `Priority`, `IsSubState`, execution delay, three time-scale fields plus ease, two cursor bools, scene-change and revert flags, two HUD flags. |
| `GameStateComponent` | Runtime/State/GameStateComponent.cs (305 lines) | One per state per scene. Finds its conditions through `StateConditionRegistry`, asks `GameStateRegistry` for permission, saves `Time.timeScale` and restores it later, runs the delay and ease coroutines, reactivates on scene change. |
| `GameStateRegistry` | Runtime/Registries/GameStateRegistry.cs | Static. Active-state set, sub-state history stack, priority eviction, a busy flag. Events: Registered, Unregistered, Restored, ConfigurationsApplied. |
| `StateConditionRegistry` | Runtime/Registries/StateConditionRegistry.cs | Static map from a `StateSO` to its conditions, so a condition in one scene reaches the component in another. |
| Conditions | Runtime/Conditions/*, GenericStates/PauseState/* | `BaseCondition` (TargetState + activate/deactivate bool), `EventCondition`, `PollCondition`, `CancelEventCondition`, `PauseEventCondition` (+ `PauseStateSO` that only holds an input action), `StateBasedActivationTrigger`. |
| Reactors | Runtime/UI/*, Audio/*, State/GameStateEventTrigger.cs, State/GameCursorController.cs | Menu toggle, HUD toggle, UnityEvent trigger, BroAudio trigger, the static cursor controller. |

Rally adds, in `Assets/[1]Project/ProjectServices/GameManagment/CustomGameStateConditions/`: `GameOverCondition`,
`GameWinCondition`, `RallyPauseCondition`, `StateInputActionEnabler`, `StateInputMode`. Other Rally scripts that
listen to the registry: `StateAudioVolumeController`, `GameStateBlur`, `CarDeathSequence`, `CarDeathSkipInput`,
`RunTimerUI`, `CarRumbleDirector`, `FinishGateSequence`. Scenes using it: MainMenu, levels 2-4, and
`ProjectServices/GameManagment/GameManagment.prefab` (the persistent setup).

Rally's state assets (live in the library's `Runtime/StateConfigurations/`, which is game data in the library):

| State | Priority | Sub-state | Time | Delay | Cursor | HUD on enter |
|---|---|---|---|---|---|---|
| Play | 0 | no | 1 | 0 | locked, hidden | yes |
| Pause | 1 | no | 0 | 0 | free, visible | no |
| Start (main menu) | 2 | no | 1 | 0 | free, visible | yes |
| Start Settings | 3 | yes | 1 (no revert) | 0 | free, visible | no |
| Pause Settings | 3 | yes | 0 | 0 | free, visible | no |
| Game Over | 10 | no | eases to 0 over 1 s | 1.4 s | free, visible | no (HUD on leave) |
| Game Win | 15 | no | 0 (no revert) | 0 | free, visible | no |

## Strengths

- Data-driven and game-agnostic: states are assets, conditions are small scripts, reactors only listen.
- States link across scenes by asset identity, so a condition in a level can drive a state set up in the boot scene.
- `GameCursorController` works the cursor out from the top state every time instead of saving and restoring it.
  It cannot go stale. This is the pattern the redesign spreads to everything.
- Statics reset at SubsystemRegistration (works with domain reload off).
- Shipped on Steam in Rally Shift; the user's most successful of more than 10 game management systems.

## Weaknesses (with evidence)

1. **Time has many owners.** Every `GameStateComponent` saves `Time.timeScale` when it activates and writes it back
   when it leaves. With overlapping states the saved value goes stale. `HandleStateUnregistered` (eviction) always
   restores, ignoring `RevertConfigurationsOnDeactivation`. Game Over and Game Win do not revert, so time only returns
   to 1 because Play happens to reactivate after the scene change. This is the soft-lock class the user reports.
   It also means that today time likely stays 0 during the scene change after Game Over or Win, which keeps the shared
   scenes (bomb, car) still while the next level loads. Session 1's baseline must confirm this per frame; the redesign
   keeps that behaviour by default (DECISIONS round 3, item 7).
2. **Priority ties are undefined.** States with equal priority can both be active, and `GetCurrentHighestState` picks
   whichever a `HashSet` returns first. Start Settings and Pause Settings both use 3. Priorities are magic numbers
   across 7 assets with no overview.
3. **Sub-states evict their parent.** Opening Settings removes Pause (menus close, audio stops, HUD reacts), then
   "restores" it on close. Rally's audio fix commits ("Fixed Audio repeated reducing on sub states to main state") come
   from this churn. `IsSubState` only works if the sub-state's priority number is higher than its parent's.
   A non-sub-state activation clears the whole history stack.
4. **Requests get lost.** `TryRegisterAsActiveState` returns false while `isProcessingStateChange` is set.
   `StateBasedActivationTrigger` needs a `LateUpdate` workaround because of it.
5. **Mistakes are silent.** A condition whose state has no `GameStateComponent` in any loaded scene does nothing and
   logs nothing. Two components for the same state are not reported. `GameStateUIToggle` reacts to any registered
   state, even one that is not on top.
6. **Repeated code.** Menu, HUD toggle, event trigger, audio trigger, `StateInputActionEnabler`, `GameStateBlur` and
   others each repeat subscribe, filter by state, and first-frame sync.
7. **Field soup.** Three fields express one time scale (`IsTimeZeroOnExecution`, `OverrideExecutionTimeScale`,
   `ExecutionTimeScale`). Two bools express one cursor mode. `DisableStateOnSceneChange` has a wrong tooltip.
   `PauseStateSO` exists only to hold one input action; `PauseEventCondition.canPause` is a protected field a subclass
   mutates.
8. **Debugging is hard.** No record of what changed, who asked, or why a request was refused.
9. **Not shippable as a product.** No README, glossary, tests, validation, onboarding or in-Editor docs. Folder name typo.
