# The scene system calls transitions through one interface

Games change scene with one call on `SceneActions`, and the screen effect runs inside that call. The scene module defines `ISceneTransitionExecutor` (`TransitionToHideScreen`, `TransitionToShowScreen`). The transitions module implements it and registers itself at start-up, and the coordinator calls it directly, only after it has accepted the change. The scene module never references transitions code, because the transitions module already depends on it and must stay usable for screen effects that have nothing to do with scenes.

## Considered options

- Keep two public calls (`TransitionActions.*WithEffects` and `SceneActions.*`). Rejected: two busy flags, and the screen went black even when the coordinator refused the change.
- Connect the two modules through events. Rejected: a scene change spread over event handlers is hard to follow in a debugger.

## Consequences

`TransitionActions` keeps status (`IsTransitioning`, in or out) and stand-alone effects. Its two button method names, `TransitionToSceneWithEffects` and `ReloadActiveScenesWithEffects`, stay as forwards to `SceneActions`, because pause, win and game over buttons call them by name through UnityEvents. `SceneActions` returns a scene change result; `MonoSceneActions` returns nothing, because UnityEvents only list void methods.
