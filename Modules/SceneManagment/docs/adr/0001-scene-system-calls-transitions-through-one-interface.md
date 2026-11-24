# The scene system calls transitions through one interface

<!-- layout new -->

<!-- decision -->

Games change scene with one call on `SceneActions`, and the screen effect runs inside that call.

## The decision

- **The interface:** the scene module defines `ISceneTransitionExecutor` (`TransitionToHideScreen`, `TransitionToShowScreen`).
- **Who implements it:** the transitions module, which registers itself at start-up.
- **When it is called:** the coordinator calls it directly, only after it has accepted the change.
- **No reference back:** the scene module never references transitions code. The transitions module already depends on it and must stay usable for screen effects that have nothing to do with scenes.

## Considered options

| Option | Why not |
|---|---|
| Keep two public calls (`TransitionActions.*WithEffects` and `SceneActions.*`) | Two busy flags, and the screen went black even when the coordinator refused the change |
| Connect the two modules through events | A scene change spread over event handlers is hard to follow in a debugger |

## Consequences

- **`TransitionActions`:** keeps status (`IsTransitioning`, in or out) and stand-alone effects.
- **Button names kept:** `TransitionToSceneWithEffects` and `ReloadActiveScenesWithEffects` stay as forwards to `SceneActions`, because pause, win and game over buttons call them by name through UnityEvents.
- **Results:** `SceneActions` returns a scene change result; `MonoSceneActions` returns nothing, because UnityEvents only list void methods.

## Amended by the 4C grill, 2026-11-21

- **Status:** the status `TransitionActions` keeps is the screen state (Visible, Hiding, Hidden, Showing) with one change event, replacing `IsTransitioning` and the in/out events (design decision 10).
- **Scene calls:** `SceneActions.ChangeTo`, `Retry` and `Preload`; the two button names forward to them.
