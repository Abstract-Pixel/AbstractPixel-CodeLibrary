# Relocation labels are assets, and a relocatable with no spawn point position stays held

Relocatables and spawn point positions match by a relocation label asset, never by a string or a game enum, so a typo cannot break a match and a new kind of object needs no code change. An empty label field is a valid label of its own ("No label"), so a game that relocates only its player needs no asset. Many relocatables can share one label; each spawn point position carries an instance, counted from 1, and a relocatable either names its instance or takes the next free one in Hierarchy order. A relocatable with no matching position stays held where it is, and an error names the object, the label and the scene group, because releasing it could drop the player through the map.

## Considered options

- One position per enum value (Rally Shift's `BootRelocatorService`). Rejected: one point per kind, and every new kind edits a shared game enum.
- Every relocatable must name its instance. Rejected: 100 identical crates would need 100 numbers typed by hand. A named instance always wins over a free one.
- Match one specific object to one position by a stable object id. Replaced by the instance, which needs no cross-scene reference.

Amended 2026-11-21 (4D grill): renamed from placement labels to relocation labels; "No label" and the instance added.
