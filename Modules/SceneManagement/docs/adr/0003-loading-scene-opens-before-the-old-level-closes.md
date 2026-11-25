# A loading scene opens before the old level closes

<!-- layout new -->

<!-- decision -->

When a scene group uses a loading scene, the loading scene opens first, and the old level closes behind it before the new one opens.

## The order

1. **Cover the old level**
   1. Hide the screen
   2. Open the loading scene
   3. Show the screen
2. **Swap the levels**
   1. Close the old main scene and shared scenes
   2. Free unused memory
   3. Open the new scenes
3. **Uncover the new level**
   1. Hide the screen
   2. Close the loading scene
   3. Show the screen

## Why

- **Lower peak memory:** the old level leaves memory before the new one loads, which matters on the Steam Deck and in the browser.
- **A live screen:** the player sees a live scene instead of a frozen frame.

## Consequences

- **One loading scene per game:** loading scenes are listed in Project Settings with a default, so a game keeps one loading scene and varies its look inside it.
- **A choice per scene group:** each scene group says whether it uses one, and whether Retry does, so a survivors-like keeps instant restarts. The first names (Off, When changing level, Always) were replaced on 2026-11-21, see below.
- **No one-frame flash:** a loading scene stays on screen for a minimum time from the settings, so a fast load does not flash it for one frame.

## Amended by the 4C grill, 2026-11-21

- **Names:** the per-group choice is "Show the loading scene": Never, When arriving from another scene group, or Every time, Retry included. These replace "use a loading scene" plus the Retry option (Off, When changing level, Always), which read as if Off stopped a Retry.
- **With no screen effect:** the cut is instant: the loading scene opens and covers everything, then the old scenes close. It still opens before the old level closes, so no frame is left with no camera.
- **With a screen effect:** the same effect hides and shows around the loading scene. The player sees the effect cover the old level, uncover the loading scene, the loading scene fill, the effect cover it, then uncover the new level.
- **Timing:** the loading progress view starts its appear animation only when Show screen starts, and Hide screen starts only after its "full" animation has finished, so the effect and the loading scene never animate over each other.
- **Draw order:** the effect always draws above the loading scene.
- **No loading scene:** a switch to a preloaded scene group never shows the loading scene, and neither does start-up (the launch scene covers it).
