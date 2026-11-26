# Game management

Controls which named situation the game is in (main menu, play, pause, game over, game win) and what each one does to
time, cursor, HUD and input maps. Words: [GLOSSARY.md](GLOSSARY.md). Design and decisions:
[AgentContext/README.md](AgentContext/README.md). A working example: `Samples/01 Minimal Game/`.

Needs the Input System and BroAudio. The runtime assembly compiles only when BroAudio is installed.

## Daily use

From code: `GameStates.Request(state)`, `GameStates.Leave(state)`, `GameStates.IsActive(state)`, `GameStates.Top` and
the `GameStates.Changed` event. From a scene, add components:

| Component | What it is for |
|---|---|
| `SceneStartRequester` | Enters a state when the scene starts (Play in a level) |
| `StateButtonRequester` | UnityEvent buttons: Enter, Leave or Toggle a dragged state |
| `InputToggleRequester`, `BackRequester` | Pause and Back from input actions |
| `EventRequester`, `PollRequester`, `NextStateRequester` | Requests from events, from a condition you write, or after a state leaves |
| `StateMenuReactor`, `HudToggleReactor`, `StateAudioReactor`, `GameStateAudioTrigger` | Menus, HUD and sounds that follow the states |

## Setup

- One `GameManagementSettings` asset per project: the state catalog, what time does with no active state, and the input
  source. The Editor keeps it in Preloaded Assets.
- One `StateSO` asset per state: delay, leave on scene change, parents (a state with parents is a sub-state) and
  effects (time with ease, cursor, HUD, input maps). Each effect can follow the parent.
- The catalog's order is the rank: a state higher in the list outranks a lower one.

`GameCursorController` is the only writer of the cursor. It hides it while a gamepad is in use.

## How Rally Shift uses it

State assets are in `Assets/[1]Project/ProjectServices/GameManagment/States/`, with a persistent setup in
`GameManagment.prefab` and Rally's request adapters for pause, game over and win. The flows are in
[AgentContext/FLOWS.md](AgentContext/FLOWS.md) (written against the old system) and the recordings in
[AgentContext/Parity/README.md](AgentContext/Parity/README.md).

## Tests

`Tests/AbstractPixel.GameManagement.Tests` is the EditMode assembly. The sample has its own PlayMode flow test.
