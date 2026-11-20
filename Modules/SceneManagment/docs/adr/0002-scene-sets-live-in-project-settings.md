# Scene sets live in Project Settings, and each scene group picks one

The manager scenes and shared scenes that come with a level are listed once, in a scene set asset, instead of on every scene group. Project Settings > Abstract Pixel > Scene System holds the scene sets and the default one, and each scene group picks a set from a dropdown that starts on the default. Rally Shift's 13 levels repeated the same two scenes in every group, and a game with modes or minigames needs a few different sets, not one per level.

## Considered options

- One boot profile per game, found through `Resources`. Rejected: one profile cannot describe a game whose modes need different manager scenes, and `Resources` is a hidden place to configure a game.
- Keep the lists on every scene group. Rejected: this was the main source of setup mistakes.

## Consequences

The settings asset is added to Player Settings' Preloaded Assets so a build can read it before any scene opens. The same asset keeps the list of scene groups that "Play from any scene" searches, kept current by an editor script. A scene group's inspector stays short: main scene, scene set, loading scene and the Retry option, with the rest under Advanced.
