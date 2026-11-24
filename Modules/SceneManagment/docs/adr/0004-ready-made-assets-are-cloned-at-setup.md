# Ready-made assets are cloned into the game at setup

<!-- layout new -->

<!-- decision -->

The library ships a working default fade and a default loading scene, and setup copies them into the game.

## The decision

- **What is copied:** the "Set up scene system" button in Project Settings copies the fade and the loading scene into the game's `Assets/` folder and assigns the copies, together with the settings asset and a default scene set.
- **Then:** games restyle the copies.
- **Why:** a package installed from git is read-only, and editing the library's own prefabs in place (as happened with the Settings module) changes them for every project.

## Consequences

- **No links back:** the library's originals are never referenced by a game's settings after setup.
- **Samples:** have their own visuals, clearly marked as samples, so nobody mistakes sample art for the ready-made defaults.
