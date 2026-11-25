# 04 Play From Any Scene

<!-- layout new -->

**No Code**
You will learn that Play from any scene of a scene group opens the whole group, and how the Editor opens a group's scenes together.

Covers: F01 Setup window · F02 Page: Setup section (look only) · F04 Page: Scene sets section (look only) · F13 Scene set inspector, overrides, Revert to set (look only) · F17 Scene opening preferences · F27 Start-up and Play from any scene

## What you need
Unity 6.3 or newer and the scene system set up (Project Settings > Abstract Pixel > Scene System). No extra packages.

## Files
| Folder | What is in it |
|---|---|
| `Scenes/` | `Sample 04 Level` (main scene, three crates) and `Sample 04 Shared` (shared scene, lists what is open) |
| `Scene Groups/` | `Sample 04 LevelGroup`: main scene `Sample 04 Level`, shared scene `Sample 04 Shared` |
| `Prefabs/` | `Sample 04 Crate`: a box with a lid |
| `Materials/` | `Sample 04 Level` (brick red) |
| `Scripts/` | `Sample04WhatLoaded`: shows the open scenes and the active scene group |
| `Tests/` | The PlayMode test: the list shows both scenes and the group. The start-up rule check is in `Shared/Tests` |

## Try it
1. Open `Sample 04 Shared`: in the Project window, go to `Packages/Abstract Pixel Library/Modules/SceneManagement/Samples/04 Play From Any Scene/Scenes/` and double-click it.
2. Open Edit > Preferences > Abstract Pixel > Scenes and read the two scene opening preferences, "Open a main scene with its scene group" and "Open a scene in no group with the default scene set". Tick the first one.
3. Double-click `Sample 04 Shared` again.
4. Press Play. Stop Play. <!-- done-when OnGameStarted -->
5. Close every scene but `Sample 04 Level` (right-click `Sample 04 Shared` in the Hierarchy > Remove Scene), press Play, then stop Play.
6. Open Tools > External Project Tools > Scene System > Set Up Scene System. In a project that is already set up it shows the Opening Scenes step. Click Done.
7. Look only, change nothing: open Project Settings > Abstract Pixel > Scene System and read the Setup section and the Scene sets section (the cards, Make default, the name you can rename, + and −). Select the project's default scene set in the Project window and look at its Inspector; then select a game scene group that uses it and find the "Override" and "Revert to set" buttons next to its parts. Do not save any change.
8. Put the first preference back the way you found it in step 2.

## You should see
<!-- timeline -->

- After step 1 only `Sample 04 Shared` is in the Hierarchy if the first preference is off.
- After step 3 the Hierarchy shows `Sample 04 Shared`, `Sample 04 Level` and `Samples Managers`: the sample scene group opened with its main, shared and manager scenes.
- In step 4 and step 5 the label lists "Open scenes: …Sample 04 Level, Sample 04 Shared…" and "Active scene group: Sample 04 LevelGroup" both times.
- In step 6 the setup window shows "Opening Scenes" with the two preferences.

## The lines that matter
This sample has no code that you need to copy; start-up does the work. The settings that matter:
- `Sample 04 LevelGroup`: "Sample scene group" ticked, main scene `Sample 04 Level`, shared scene `Sample 04 Shared`. Play from either scene opens both.
- Preferences > Abstract Pixel > Scenes: the two scene opening preferences act in edit mode only; Play always loads what the group needs.

## Windows this sample shows
- **Scene opening preferences:** Edit > Preferences > Abstract Pixel > Scenes (also the scene picker's footer, "Open with: Group, Default set").
- **Setup window:** Tools > External Project Tools > Scene System > Set Up Scene System.
- **Setup section and Scene sets section:** Project Settings > Abstract Pixel > Scene System (look only).
- **Scene set inspector, Override and Revert to set:** select a scene set, then a scene group that uses it (look only).

## How it's set up
1. Select the level's scene group; Play from its main scene opens it.
   <!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagement/Samples/04 Play From Any Scene/Scene Groups/Sample 04 LevelGroup.asset" "Select the Level group" -->
2. Select `What Loaded` in the shared scene; it shows what start-up opened.
   <!-- action select-in-scene "Packages/com.abstractpixel.library/Modules/SceneManagement/Samples/04 Play From Any Scene/Scenes/Sample 04 Shared.unity" "What Loaded" "Select What Loaded" -->
3. Open the scene opening preferences.
   <!-- action open-preferences "Open Preferences" -->

## Why it works
- [Start-up](../../Editor/Documentation/Pages/guide/start-up.md#play-from-any-scene) opens the group of the scene Unity opened, so no start-up object is needed ([design decision 9](../../docs/adr/0009-start-up-opens-the-start-scene-group-behind-a-launch-scene.md)).
- Opening a scene in the Editor also opens what Play loads with it ([editor tools](../../Editor/Documentation/Pages/guide/editor-tools.md#scene-opening-preferences)).

## If it goes wrong
- **Step 3 opens only the shared scene:** the first preference is off: tick it in Preferences > Abstract Pixel > Scenes.
- **Play opens a different group:** another main scene was still open: close it before Play.
- **The label is empty:** `Sample 04 Shared` did not load: check the group's shared scenes list.

## Next steps
- **[09 Editor Tools](../09%20Editor%20Tools/README.md):** the scene picker, the group inspector and the Debugger.
- **[10 Launch Scene](../10%20Launch%20Scene/README.md):** Play from a launch scene.
