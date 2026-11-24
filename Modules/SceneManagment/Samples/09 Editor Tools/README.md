# 09 Editor Tools

<!-- layout new -->

**No Code**
You will learn the Editor tools of the scene system: Create Scene Group, the scene field, the group inspector and its check, the Scene Groups Debugger, the sample scene groups list, migration, the build guard and the scene picker.

Covers: F08 Page: Scene Groups Debugger · F09 Page: Sample scene groups section · F10 Page: Preview migration and Migrate (look only) · F11 Scene group inspector · F12 Group check and fix buttons · F14 Scene field · F15 Create Scene Group · F16 Scene picker · F18 Sample scene group and the build guard

## What you need
Unity 6.3 or newer and the scene system set up (Project Settings > Abstract Pixel > Scene System). No extra packages.

## Files
| Folder | What is in it |
|---|---|
| `Scenes/` | `Sample 09 Workshop` (main scene of a group) and `Sample 09 Loose Scene` (in no group) |
| `Scene Groups/` | `Sample 09 WorkshopGroup` |
| `Prefabs/` | `Sample 09 Bench`: a top, two legs and a vice |
| `Materials/` | `Sample 09 Workshop` (wood) |
| `Tests/` | The PlayMode test: the Workshop group opens. The Editor checks are in `Shared/Tests` (`Sample09EditorToolsTests`) |

## Try it
1. Open `Sample 09 Workshop`: in the toolbar's scene picker, type "Sample 09 Workshop" and click the row. On 6.3 the scene picker is hidden until you show it: right-click an empty part of the main toolbar and tick "Abstract Pixel/Scene picker". On 7.0 it is in the toolbar's overflow list under the same name.
2. In the Project window, right-click `Scenes/Sample 09 Loose Scene` > Create > Scene Group. A group `Sample 09 Loose SceneGroup` appears next to the scene, with "Sample scene group" already ticked. Look at it, then delete it (right-click > Delete).
3. Select `Scene Groups/Sample 09 WorkshopGroup`. In its Inspector find: the Main scene (a scene field with a coloured status: green In build, blue Addressable, grey sample, red needs a fix; right-click it for the move items), the scene set (none for a sample), "Show the loading scene" and "Loading scene", the Retry option, "Sample scene group", Advanced (manager scenes, shared scenes, Reset shared scenes) and "Open group".
4. Read the group check under the main scene: here it finds no problem. A group with a problem shows it in red with a fix button.
5. Open Project Settings > Abstract Pixel > Scene System. Find the Scene Groups Debugger (every group, its main scene and scene set, then the sample scene groups) and the "Sample scene groups" section, which lists `Sample 09 WorkshopGroup`.
6. Look only: click "Preview migration" in the same page and read the plan. Sample scene groups are left out of migration. Do not click Migrate.
7. Look only: the build guard. If a sample scene were in Build Settings (or Addressable), a build would stop and name it. Do not add one.
8. In the scene picker: switch between its two tabs (Scenes and Scene Groups), type in its search field, look at Recent, and hover each row's label, status and folder buttons. Right-click a row for Open, Open additively, Select scene, Select scene group and Copy path. The footer reads "Open with: Group, Default set".

## You should see
<!-- timeline -->

- In step 2 the new group has "Sample scene group" ticked and no scene set, and it disappears again after Delete.
- In step 3 the Main scene field shows "Not in build". That is right for a sample scene, which never goes into a build; the field has no sample state of its own yet (the scene picker shows it grey). Do not use its fix: the scene stays out of Build Settings anyway.
- In step 4 the group check shows no problem.
- In step 5 the Sample scene groups section lists `Sample 09 WorkshopGroup`.
- In step 8 `Sample 09 Workshop` has a grey status chip in the picker.

## The lines that matter
This sample has no code. The settings to look at are on `Sample 09 WorkshopGroup`: "Sample scene group" ticked, main scene `Sample 09 Workshop`, manager scene `Samples Managers`, no scene set.

## Windows this sample shows
- **Create Scene Group:** Project window, right-click a scene > Create > Scene Group (Assets > Create > Scene Group).
- **Scene group inspector and group check:** select a scene group.
- **Scene field:** the Main scene field in the group inspector.
- **Scene Groups Debugger, Sample scene groups, Preview migration:** Project Settings > Abstract Pixel > Scene System.
- **Scene picker:** main toolbar, "Abstract Pixel/Scene picker".

## How it's set up
1. Select the workshop's scene group and read its group check.
   <!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Samples/09 Editor Tools/Scene Groups/Sample 09 WorkshopGroup.asset" "Select the group" -->
2. Ping the loose scene: right-click it, Create > Scene Group.
   <!-- action ping "Packages/com.abstractpixel.library/Modules/SceneManagment/Samples/09 Editor Tools/Scenes/Sample 09 Loose Scene.unity" "Ping the loose scene" -->
3. Open the Scene Groups Debugger.
   <!-- action open-settings "Scene Groups Debugger" "Open the Debugger" -->
4. Open the scene picker.
   <!-- action open-picker "Open the scene picker" -->

## Why it works
- The [Editor tools](../../Editor/Documentation/Pages/guide/editor-tools.md) never change your project on their own; each fix is a button you press.
- [Sample scene groups](../../Editor/Documentation/Pages/guide/editor-tools.md#sample-scene-groups) never reach a build ([design decision 14](../../docs/adr/0014-sample-scene-groups-never-reach-a-build.md)).

## If it goes wrong
- **No scene picker in the toolbar:** on 6.3, right-click the toolbar and tick "Abstract Pixel/Scene picker".
- **Create Scene Group selects an existing group:** that scene is already a main scene: a scene is the main scene of one group at most.
- **A game scene's status is red:** click its fix, or right-click the field for the move items. A sample scene's "Not in build" needs no fix.

## Next steps
- **[04 Play From Any Scene](../04%20Play%20From%20Any%20Scene/README.md):** what Play opens.
- **[08 Addressables](../08%20Addressables/README.md):** the blue status and the moves.
