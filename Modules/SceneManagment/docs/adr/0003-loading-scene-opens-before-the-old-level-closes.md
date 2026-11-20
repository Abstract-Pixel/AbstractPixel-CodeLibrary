# A loading scene opens before the old level closes

When a scene group uses a loading scene, the order is: hide the screen, open the loading scene, show the screen, close the old main scene and shared scenes, free unused memory, open the new scenes, hide the screen, close the loading scene, show the screen. The old level leaves memory before the new one loads, which lowers peak memory on the Steam Deck and in the browser, and the player sees a live scene instead of a frozen frame.

## Consequences

Loading scenes are listed in Project Settings with a default, so a game keeps one loading scene and varies its look inside it. Each scene group says whether it uses one, and whether Retry does (Off, When changing level, Always), so a survivors-like keeps instant restarts. A loading scene stays on screen for a minimum time from the settings, so a fast load does not flash it for one frame.
