# Ready-made assets are cloned into the game at setup

The library ships a working default fade and a default loading scene. The "Set up scene system" button in Project Settings copies them into the game's `Assets/` folder and assigns the copies, together with the settings asset and a default scene set. Games then restyle the copies. A package installed from git is read-only, and editing the library's own prefabs in place (as happened with the Settings module) changes them for every project.

## Consequences

The library's originals are never referenced by a game's settings after setup. Samples have their own visuals, clearly marked as samples, so nobody mistakes sample art for the ready-made defaults.
