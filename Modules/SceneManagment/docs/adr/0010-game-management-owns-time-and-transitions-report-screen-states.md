# Game management owns game time; the screen effect only reports screen states

The basic fade set `Time.timeScale = 1` after covering the screen, so a scene change started from the pause menu quietly unpaused the game behind game management's back. From 4C the transitions module never reads or writes `Time.timeScale`, and neither does the scene system. The transitions module reports a screen state (Visible, Hiding, Hidden, Showing) and an event when it changes; game management, which coordinates the app's states, decides what time does in each of them. Screen effects and loading scenes run on unscaled time, so they play while the game is paused.

## Considered options

- The scene system resets `timeScale` to 1 after every scene change. Rejected: it forces a value the game configured, and a game that wants to start a place paused could not.

## Consequences

A flow that changes scene while paused (pause menu "Main menu" or Retry, win screen, game over) relies on game management to leave the paused state. Each game proves that with a scenario. The game management module does not reference the transitions module; a game connects them in its own code.
