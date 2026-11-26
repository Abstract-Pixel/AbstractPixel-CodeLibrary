# Target design

Uses the codebase-design vocabulary (module, interface, seam, adapter, depth). Decisions behind it: [DECISIONS.md](DECISIONS.md).

## One deep module

Today one transition passes through four shallow pieces (condition, condition registry, component, state registry),
and time has many owners. The redesign puts all decisions in one module with a small interface.

**Game state machine** (plain C#, no MonoBehaviour, unit-testable):

- Daily interface: `Request(state)`, `Leave(state)`, `IsActive(state)`, `Top`, and one `Changed` event carrying a
  transition record (what was asked, by whom, accepted or refused and why, the stack before and after, the effects before
  and after).
- Hidden inside: a request queue processed in order (no lost requests), ordering from the state catalog, the parent and
  sub-state stack, rejection reasons, execution delay through a clock.
- Static facade `GameStates` keeps call sites as simple as today and resets at SubsystemRegistration. Tests build the
  machine directly with a test catalog, a fake clock and a recording effect sink.

## Patterns

- **Stack ordered by the catalog.** The catalog is one ordered list of states, top to bottom. Higher in the list wins.
  Ties cannot exist.
- **Sub-states with parents.** A sub-state lists its allowed parents (dragged asset references, no strings). It opens only
  while one of its parents is on top. The parent is suspended, not evicted, so reactors never see it leave and come back.
  Closing returns to whichever parent opened it. Nesting is allowed.
- **Effects are worked out, never restored.** Time scale, cursor, HUD and input maps are a pure function of the stack:
  `stack -> wanted effects`. A reconciler applies the difference. Nothing is saved for later, so nothing goes stale.
  With no state active, effects keep their last values (a setting can make time 1 instead; see DECISIONS round 3,
  item 7). Each sub-state effect defaults to "Same as parent" and can be overridden.
- **Observers** for reactors, through one composed state-watch helper (not a base class). It reports Entered (state
  took control), Applied (delay over, effects applied) and Left, and syncs once when enabled.

## Rules the machine follows (edge cases)

| Situation | Result |
|---|---|
| Request for a main state lower in the catalog than the active main state | Refused, reason logged ("Pause refused: Game Over is higher"). |
| Request for a main state higher or equal to the active one | Accepted; the old main state and all its sub-states leave (sub-states first, top down). |
| Request for a sub-state whose parents are not on top | Refused with the reason. |
| Request while another transition runs | Queued and processed in order, never dropped. |
| Leave a parent with open sub-states | Sub-states close first, top down, then the parent. Each step is journaled. |
| Leave a sub-state | Returns control to the parent that opened it; the parent never left, so reactors see no Left/Entered. |
| Request for an already active state | No change, no event. |
| State in its execution delay | In control (others refused, "on enter" menus show); effects wait for the delay; `SkipDelay` applies at once. |
| Scene change | States with "leave on scene change" leave; the new scene's "enter on scene start" conditions request. |
| No state active | Effects keep their last values (or time 1 if that option is set). |
| Rally example | Main Menu (main) + Settings (sub, parents Main Menu and Pause); Play (main) + Pause (sub of Play) + Settings; Game Over and Game Win (main, high in the catalog). |

## Seams and adapters

| Seam | Adapters | Why it is real |
|---|---|---|
| Effect sink | Unity sink (writes `Time.timeScale` with ease, cursor via `GameCursorController`, HUD, input maps); recording sink for tests | Two adapters: production and test |
| Clock | Realtime clock; fake clock in tests | Delay and ease are testable without waiting |
| Input source | Project-wide actions (`InputSystem.actions`); a chosen `InputActionAsset`; a custom source the game writes | Games own their input differently (see below) |
| Request adapters (conditions) | Enter on scene start, input toggle with optional gate, back/cancel, event, poll, "when X leaves go to Y", a requester component for UnityEvent buttons | They only call `Request` and `Leave` |

### Input without a global class

Each state has an Input effect: the action maps it turns on. Maps are picked by dragging an `InputActionReference` to any
action in the map (that action's map is turned on), never typed or stored as a string (DECISIONS.md, 2026-11-26). Where the live asset comes from is chosen once in
Project Settings > Abstract Pixel > Game Management:

1. Project-wide actions (Unity 6 `InputSystem.actions`), the default.
2. A specific `InputActionAsset`.
3. A custom input source: a small script the game writes with one method that returns the live `InputActionAsset`.
   Rally's returns `GlobalInput.PlayerInputActions.asset`. This is needed because a generated C# input class makes its
   own copy of the asset, so enabling maps on the asset file would not reach it.

With no source set, game management leaves input alone. Because input follows the same "worked out from the top state"
rule, a map cannot stay disabled after a state leaves.

## Editor surface (UI Toolkit; menus stay uGUI)

- **Project Settings > Abstract Pixel > Game Management.** Has an object field for the settings asset, so the user
  chooses where it lives (default location from the scene system's convention). Same path style as `SceneSystemSettingsProvider.PagePath`
  ("Project/Abstract Pixel/Scene System"). Modelled on `SceneGroupsDebugger` and `SceneGroupProblemsView` in
  `Modules/SceneManagement/Editor/Settings` and `Editor/Checks`. Shows the catalog order, how states, conditions and
  menus connect across scenes and prefabs, the input source, and every problem with its fixes. It does not edit
  per-state details; those stay in each state asset's inspector.
- **State inspector** with status dots: a problem shows one line (what, where) and one or two Fix buttons. Two levels
  only, error and warning. Nothing shows when all is fine. Same look as the scene system.
- **Catalog validator**: a pure function from catalog plus scene scan to diagnostics. The settings page, inspectors and
  tests all show the same list. It covers multi-parent sub-states (for example a sub-state whose parents disagree in a
  way an override must settle, a parent below its sub-state in the catalog, a sub-state with no way back).
- **Play-mode watchdog**: warns when the applied effects differ from the wanted ones (for example time is 0 but the top
  state wants 1).
- **Onboarding window**: first page chooses which states and menus the game needs (an infinite runner may skip Win).
  For each menu: duplicate the library's base menu into a chosen folder, or point at the game's own menu and let the
  module set itself up on it. Base menus are graphics-free uGUI with TextMeshPro text. Start and Pause menus keep a
  Settings button. Menus get back/cancel. Safe to run twice;
  never overwrites edited copies. Settings itself is out of scope.
- **Runtime debugger**: UI Toolkit overlay reading the transition journal (ring buffer). On in the Editor and
  Development Builds; release builds opt in through a setting.

## Words (seed for the module GLOSSARY.md)

| Word | Meaning |
|---|---|
| State | One named situation of the game (Main Menu, Play, Pause, Game Over). An asset. |
| State catalog | The ordered list of every state. Higher in the list wins. |
| Top state | The state currently in control. Its effects apply. |
| Sub-state | A state that opens over one of its parents and returns to it on close. |
| Parent | A state a sub-state may open over. A sub-state may have several. |
| Suspended | A parent under an open sub-state: still active, not in control. |
| Effect | Something a state controls: time scale, cursor, HUD, input maps. |
| Request / Leave | Asking the machine to enter or exit a state. |
| Refusal | A request the machine turned down, with a reason. |
| Condition | A scene script that makes requests when something happens. |
| Reactor | A scene script that responds to state changes (menu, audio, HUD). |
| Transition journal | The recorded list of transitions the debugger reads. |
| Problem | A setup mistake the validator found, with its fixes. |

## Old to new

| Old | New |
|---|---|
| `GameStateComponent` (activate on start, reactivate on scene change, owns time) | Deleted after the parity gate. "Enter on scene start" condition; time owned by the effect sink. Fallback if setup gets harder: a thin optional scene starter. |
| `GameStateRegistry`, `StateConditionRegistry` | Inside the state machine. |
| `StateSO.Priority` | Position in the state catalog (migrated in priority order; ties reported for a human choice). |
| `IsSubState` | Parents list. |
| Three time fields | One time-scale value (+ ease, + "Same as parent" on sub-states). |
| Two cursor bools | One cursor mode. |
| `RevertConfigurationsOnDeactivation`, `ShowGameUIOnDeactivation` | Cut: effects follow the top state. |
| `PauseStateSO`, `PauseEventCondition`, `canPause` | Generic input toggle condition with an optional gate. `RallyPauseCondition` becomes a gate. |
| `StateBasedActivationTrigger` | "When X leaves go to Y" condition, no `LateUpdate` workaround. Rally's Pause-to-Play use disappears (Pause is a sub-state of Play). |
| Rally's state assets in the library's `Runtime/StateConfigurations/` | Moved to Rally in Session 1; the library ships starter templates. |
| `StateInputActionEnabler` (Rally) | Input effect. |
| `StateAudioVolumeController` (Rally) | Library reactor beside `GameStateAudioTrigger`, behind the BroAudio define. |
| `ActivateState`/`DeactivateState` called from UnityEvent buttons | Requester component; migration rewrites persistent calls. |
| Start Settings + Pause Settings (Rally) | One Settings sub-state with two parents. |

Serialized data survives: `FormerlySerializedAs`, an editor migration that upgrades existing assets and scenes, and
`git mv` with `.meta` files.
