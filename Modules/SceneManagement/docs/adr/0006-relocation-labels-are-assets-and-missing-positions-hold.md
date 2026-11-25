# Relocation labels are assets, and a relocatable with no spawn point position stays held

<!-- layout new -->

<!-- decision -->

Relocatables and spawn point positions match by a relocation label asset, never by a string or a game enum, so a typo cannot break a match and a new kind of object needs no code change.

## The decision

- **"No label":** an empty label field is a valid label of its own, so a game that relocates only its player needs no asset.
- **Many per label:** many relocatables can share one label. Each spawn point position carries an instance, counted from 1.
- **Instances:** a relocatable either names its instance or takes the next free one in Hierarchy order. A named instance always wins over a free one.
- **No position:** the relocatable stays held where it is, and an error names the object, the label and the scene group, because releasing it could drop the player through the map.

## Considered options

| Option | Why not |
|---|---|
| One position per enum value (Rally Shift's `BootRelocatorService`) | One point per kind, and every new kind edits a shared game enum |
| Every relocatable must name its instance | 100 identical crates would need 100 numbers typed by hand |
| Match one specific object to one position by a stable object id | Replaced by the instance, which needs no cross-scene reference |

## Amended 2026-11-21 (4D grill)

- **Name:** renamed from placement labels to relocation labels.
- **Added:** "No label" and the instance.

## Amended 2026-11-22 (4D stress build)

- **Labels match by an ID:** the label asset's GUID, stamped into it on import, not by object identity.
- **Why:** an Addressables bundle carries its own copy of every asset it uses, so in a build the car's label and the spawn point's label were two objects and Level 3 kept the car held.
- **Rejected, matching by asset name:** a rename, or two labels with one name, would break it.
- **Rejected, keeping labels out of bundles:** it depends on how each game builds its groups.
- **Old labels:** a label with no ID yet (made before this change, not reimported) falls back to object identity.
