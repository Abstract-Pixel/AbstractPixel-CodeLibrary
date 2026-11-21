# A loading scene opens before the old level closes

When a scene group uses a loading scene, the order is: hide the screen, open the loading scene, show the screen, close the old main scene and shared scenes, free unused memory, open the new scenes, hide the screen, close the loading scene, show the screen. The old level leaves memory before the new one loads, which lowers peak memory on the Steam Deck and in the browser, and the player sees a live scene instead of a frozen frame.

## Consequences

Loading scenes are listed in Project Settings with a default, so a game keeps one loading scene and varies its look inside it. Each scene group says whether it uses one, and whether Retry does (Off, When changing level, Always), so a survivors-like keeps instant restarts. A loading scene stays on screen for a minimum time from the settings, so a fast load does not flash it for one frame.

## Amended by the 4C grill, 2026-11-21

- **Names.** The per-group choice is "Show the loading scene": Never, When arriving from another scene group, or Every time, Retry included. These replace "use a loading scene" plus the Retry option (Off, When changing level, Always), which read as if Off stopped a Retry.
- **With no screen effect** the cut is instant: the loading scene opens and covers everything, then the old scenes close. It still opens before the old level closes, so no frame is left with no camera.
- **With a screen effect** the same effect hides and shows around the loading scene, so the player sees: the effect covers the old level, the effect uncovers the loading scene, the loading scene fills, the effect covers it, the effect uncovers the new level. The loading progress view starts its appear animation only when Show screen starts, and Hide screen starts only after its "full" animation has finished, so the effect and the loading scene never animate over each other. The effect always draws above the loading scene.
- **A switch to a preloaded scene group never shows the loading scene**, and neither does start-up (the launch scene covers it).
