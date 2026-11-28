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

### Setup window

`Tools > External Project Tools > Game Management > Set Up Game Management` opens by itself once per Editor session while
the project has no settings of its own (never in batch mode, Play mode or tests; "Don't show again" is per project).

- It makes the settings, the state list, the states you tick (Play always) and, for each, either a copy of the library's menu
  (`Runtime/MenuPrefabs`, with a State Menu Reactor, wired buttons and gamepad selection) or, for "Use my own", a reactor
  on your prefab. Nothing existing is written over: a taken name gets a new one and the Ready page says so.
- A settings asset inside a package (a sample's) is not the project's: the Settings page shows it read-only with a warning
  and **Create settings**, and nothing writes into it.
- Run it again from the Settings page ("Set up states...") to add a menu you skipped; the menus you have are locked.
- Copied menus: Restart and Main Menu leave the end state first (Game Over and Game Win outrank Play and Main Menu).
  Buttons that need your scenes (Restart from Pause, Next Scene) are removed. A key to open Pause (Input Toggle Requester)
  and a Back key (Back Requester) are yours to add.

## In-game debugger

A read-only panel, top right: the state stack, the effects in force, the last refusal and the newest journal lines.
It never takes input or the cursor, and works at time scale 0. It starts closed.

- Editor: F8 (change it in Edit > Shortcuts).
- Any build, and the Editor: set **Show or hide debugger** in the Debugger card. Input maps a state switches off do not silence it.
- Builds: Development Builds hold it. A release build holds its code and UI assets only with **Include the debugger in release builds** on.
- A state's colour follows its place in the state list. The first six differ.

## How Rally Shift uses it

State assets are in `Assets/[1]Project/ProjectServices/GameManagment/States/`, with a persistent setup in
`GameManagment.prefab` and Rally's request adapters for pause, game over and win. The flows are in
[AgentContext/FLOWS.md](AgentContext/FLOWS.md) (written against the old system) and the recordings in
[AgentContext/Parity/README.md](AgentContext/Parity/README.md).

## Tests

`Tests/AbstractPixel.GameManagement.Tests` is the EditMode assembly. The sample has its own PlayMode flow test.
