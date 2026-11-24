# 05 Persistent Player And Spawn Points

<!-- layout new -->

**Basic Code**
You will learn how objects in a shared scene, such as the player, move to the right place in every new scene group.

Covers: F07 Page: Relocation section · F19 Relocatable, Relocation Label, Stamp relocation label IDs · F20 Spawn point inspector and Scene view tools · F28 Relocation: Hold, Relocate, Release, missing position

## What you need
Unity 6.3 or newer and the scene system set up (Project Settings > Abstract Pixel > Scene System). No extra packages.

## Files
| Folder | What is in it |
|---|---|
| `Scenes/` | `Sample 05 Area A` and `Sample 05 Area B` (main scenes with spawn points), `Sample 05 Shared` (the player, 100 crates, 3 drones and a cart, each with a `Relocatable`) |
| `Scene Groups/` | `Sample 05 Area AGroup` and `Sample 05 Area BGroup`: sample scene groups that share `Sample 05 Shared` |
| `Relocation Labels/` | `Cart`, `Crate` and `Drone`: the labels that pair each relocatable with its spawn points |
| `Tests/` | The PlayMode test: in both areas all 105 relocatables are released and none is held |

## Try it
1. Open `Sample 05 Area A` with its scene group: in the toolbar's scene picker, tick "Group" in its footer ("Open with: Group"), type "Sample 05 Area A" and click the row. (In the Project window the scene sits in `Packages/Abstract Pixel Library/Modules/SceneManagment/Samples/05 Persistent Player And Spawn Points/Scenes/`.)
2. Select `Player` in `Sample 05 Shared` (Hierarchy) and look at its `Relocatable` component: its label is "No label", so it goes to the `Player start` spawn point.
3. Select `Crates near the barn` in `Sample 05 Area A` and look at the `Spawn Point` inspector: its label is `Crate`, it lists its positions and the Scene view controls.
4. Hover the Scene view and hold Shift: a ghost shows where a new position would go. Shift + click the ground to add a position, then press Ctrl + Z to take it back.
5. Press Play. Every object in `Sample 05 Shared` jumps to its spawn point and is released once the screen counts as visible. <!-- done-when OnGameStarted -->
6. Stop Play. Select `Cart by the road` in `Sample 05 Area A` and delete it (Delete key). Do not save.
7. Press Play again. The cart has no position left in this area, so it stays held.
8. Stop Play. Open `Sample 05 Area A` again from the scene picker and click "Don't Save" when Unity asks, so `Cart by the road` comes back.
9. Open Project Settings > Abstract Pixel > Scene System and look at the Relocation section (look only: Ground layers and the steepest slope for "No label").
10. Run Tools > Abstract Pixel > Scene System > Stamp relocation label IDs. It stamps an ID on any label that has none; here every label already has one, so nothing changes.

## You should see
<!-- timeline -->

- After step 1, the Hierarchy shows `Sample 05 Area A` and `Sample 05 Shared`; the crates, drones and the cart sit in `Sample 05 Shared`.
- In step 3 the Spawn Point inspector lists its positions and a table of Scene view controls, such as "Shift + click ground: Add a position where you click".
- After step 5 the Game view shows the area from the player's camera, and the Console shows "[Relocation] Sample 05 Area AGroup: 105 released, 0 held".
- After step 7 the Console shows one error: "'Cart' stays held: label 'Cart' has no free spawn point position left in the main scene", and the Relocation line says "104 released, 1 held".
- After step 8 `Cart by the road` is back in the Hierarchy.

## The lines that matter
This sample has no code. The settings that do the work are in the Inspector:
- `Relocatable` on each object in `Sample 05 Shared`: Label (`Cart`, `Crate`, `Drone` or "No label") and Instance.
- `Spawn Point` in each area: the same label and one transform per position (Instance 1 is the spawn point's own transform).
- Each scene group: `Sample 05 Shared` as its shared scene, "Sample scene group" ticked, no scene set.

## Windows this sample shows
- **Relocatable inspector:** select a relocatable in `Sample 05 Shared`.
- **Spawn Point inspector and its Scene view tools:** select a spawn point in an area scene.
- **Relocation section:** Project Settings > Abstract Pixel > Scene System > Relocation.
- **Stamp relocation label IDs:** Tools > Abstract Pixel > Scene System > Stamp relocation label IDs.
- **Relocation Label:** Assets > Create > Abstract Pixel > Scene System > Relocation Label.

## How it's set up
1. Select the Player in the shared scene and look at its `Relocatable`.
   <!-- action select-in-scene "Packages/com.abstractpixel.library/Modules/SceneManagment/Samples/05 Persistent Player And Spawn Points/Scenes/Sample 05 Shared.unity" "Player" "Select the Player" -->
2. Select a spawn point in Area A with the Crate label.
   <!-- action select-in-scene "Packages/com.abstractpixel.library/Modules/SceneManagment/Samples/05 Persistent Player And Spawn Points/Scenes/Sample 05 Area A.unity" "Crates near the barn" "Select a spawn point" -->
3. Select the Crate relocation label.
   <!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Samples/05 Persistent Player And Spawn Points/Relocation Labels/Crate.asset" "Select the Crate label" -->
4. Open the Relocation section for the ground layers.
   <!-- action open-settings "Relocation" "Open Relocation" -->

## Why it works
- The coordinator holds, [relocates and releases](../../Editor/Documentation/Pages/guide/relocation.md#what-happens-in-a-scene-change) every relocatable at fixed moments ([design decision 12](../../docs/adr/0012-the-coordinator-calls-a-relocation-broadcaster-at-fixed-moments.md)).
- A relocatable with no free position stays held, with one error ([design decision 6](../../docs/adr/0006-relocation-labels-are-assets-and-missing-positions-hold.md)).

## If it goes wrong
- **An object stays held with an error:** its label has no free position in the main scene: add one with Shift + click.
- **An object floats or sinks after the jump:** check the Ground layers in the Relocation section.
- **Nothing moves at all:** the scene opened alone without its group: open it with its scene group (scene picker) and press Play again.

## Next steps
- **[03 Fast Restart](../03%20Fast%20Restart/README.md):** Retry resets the shared scene, so relocatables start fresh.
- **[07 Preload The Next Level](../07%20Preload%20The%20Next%20Level/README.md):** preload an area before you go there.
