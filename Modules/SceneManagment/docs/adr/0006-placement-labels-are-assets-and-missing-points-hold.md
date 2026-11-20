# Placement labels are assets, and a placeable with no spawn point stays held

Placeables and spawn points match by a placement label asset, never by a string or a game enum, so a typo cannot break a match and a new kind of object needs no code change. Many placeables can share one label and match spawn points by number. A placeable with no matching spawn point, or one more than the points available, stays held where it is and an error names the object, the label and the level, because releasing it could drop the player through the map.

## Considered options

- One position per enum value (Rally Shift's `BootRelocatorService`). Rejected: one point per kind, and every new kind edits a shared game enum.
- Match one specific object to one point by a stable id. Left for later, until a game needs it.
