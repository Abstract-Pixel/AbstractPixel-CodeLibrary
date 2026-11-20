# Start-up opens the start scene group behind an optional launch scene

The scene system settings name a start scene group and, optionally, a launch scene such as a splash. When Unity opens the launch scene, start-up opens the start scene group behind it and announces once that the game has started; the launch scene closes itself when its own sequence ends. Most games have a splash or intro, and Rally Shift's splash loaded the main menu and its manager scenes by itself, outside the scene system, so the scene system could not know when the game had started.

## Considered options

- Start-up does nothing unless build index 0 is a group's main scene, and the launch scene calls a "start the game" method. Rejected: every game with a splash would write that call, and Play from the splash would differ from Play from a level.
- Start-up opens the start scene group at once and ignores the splash. Rejected: the splash and start-up would load the main menu twice.

## Consequences

Start-up opens the main scene first and makes it active as soon as it loads, so objects created while manager scenes wake land in the main scene and not in the launch scene that closes.

The start scene group loads and wakes behind the launch scene, so it must not show itself before the hand-off. Start-up holds the launch scene hold before any scene's Awake, only when Unity opens the launch scene. Scenes in the start scene group that show something wait with `SceneActions.RunWhenLaunchSceneReleased` (or read `SceneActions.IsLaunchSceneHeld`), and the launch scene calls `SceneActions.ReleaseLaunchScene()` at its hand-off, also after a failed start-up. Play from a level or the start scene group never holds, so those scenes start at once (session 4B-2).
