# Game management

Controls which named situation the game is in (main menu, play, pause, game over, game win) and what each one does to
time, cursor, menus and HUD. This is the system Rally Shift ships on Steam. A redesign is planned; read
[AgentContext/README.md](AgentContext/README.md) before changing behaviour. Words: [GLOSSARY.md](GLOSSARY.md).

Needs the Input System and BroAudio. The runtime assembly compiles only when BroAudio is installed.

## Pieces

| Piece | What it is for |
|---|---|
| `StateSO` (and `PauseStateSO`) | One asset per state: priority, sub-state flag, delay, time scale and ease, cursor, HUD, leave on scene change |
| `GameStateComponent` | One per state in a scene. Enters and leaves the state and writes `Time.timeScale` |
| `BaseCondition`, `EventCondition`, `PollCondition` | Scene scripts that ask for a state. `CancelEventCondition`, `PauseEventCondition` and `StateBasedActivationTrigger` are ready made |
| `GameStateMenu`, `GameStateUIToggle` | Show a menu, or the game HUD, as states change |
| `GameStateEventTrigger`, `GameStateAudioTrigger` | UnityEvents and BroAudio sounds on enter and leave |
| `GameCursorController` | Static. Sets the cursor from the top state, and hides it while a gamepad is in use |
| `GameStateRegistry` | Static. `IsStateActive`, `GetCurrentHighestState`, and the registered, unregistered, restored and applied events |

## Rules that surprise people

- A higher priority state **removes** lower ones. Leaving it does not bring them back. A sub-state is the exception:
  it restores the one state that was on top.
- A request with a lower priority than the active state is refused without a log.
- With no active state, time and cursor keep their last values.
- A state needs a `GameStateComponent` in a loaded scene, or conditions that target it do nothing.

## How Rally Shift uses it

Seven state assets in the game (`Assets/[1]Project/ProjectServices/GameManagment/States/`), a persistent setup in
`GameManagment.prefab`, and Rally conditions for pause, game over and win. Every flow with its expected values is in
[AgentContext/FLOWS.md](AgentContext/FLOWS.md); the recordings are in
[AgentContext/Parity/README.md](AgentContext/Parity/README.md).

The library ships no state assets of its own at the moment. Create them with **Assets > Create > Utility > States**.

## Tests

`Tests/AbstractPixel.GameManagement.Tests` (EditMode) is the module's only test assembly.
