# Scene groups and scene sets <!-- level 0 -->

A scene group is one place the player can be; a scene set lists the scenes that come with many groups. Edit both in the Inspector and on the [Scene System page](term:scene-system-page).

## Scene groups <!-- level 0 -->

Select a [scene group](term:scene-group) to see its [main scene](term:main-scene), its scene set and what the set fills in.

**Try it in the sample**

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Samples/01 Button Changes Level/Scene Groups/Sample 01 Room AGroup.asset" "Select a scene group" -->

![The scene group inspector](../../Images/scene-system-scene-group.png)
<!-- marks 1@0.03,0.167 "Main scene"; 2@0.03,0.27 "Its scene status, with a fix button"; 3@0.03,0.325 "Scene set"; 4@0.03,0.362 "Show the loading scene"; 5@0.03,0.465 "Advanced: manager scenes, shared scenes, Reset shared scenes"; 6@0.5,0.575 "Open group" -->

**Or do it yourself**

1. Right-click a scene, Create > Scene Group.
2. Press Open group to open the main scene with everything Play opens with it.

> [!TIP]
> The Hierarchy shows the main scene with its [manager scenes](term:manager-scene) and [shared scenes](term:shared-scene).

<details><summary>If it goes wrong</summary>

- "No main scene" → pick one; a group without it cannot open.
- A scene shows Not in build → press its fix button ("Add to build" or "Make Addressable").
- The same scene listed twice → remove one entry.

</details>

## Scene sets <!-- level 0 -->

A [scene set](term:scene-set) holds the manager scenes, shared scenes and Reset shared scenes for every group that uses it. Change a set and every group using it follows, except its [overrides](term:override).

<!-- action open-settings "Scene sets" "Open Scene sets" -->


- Click + to make "New Scene Set"; its name opens for typing.
- Double-click a name, or press F2, to rename it.
- − deletes the selected set, and is off while a group uses it.

## Overrides <!-- level 1 -->

Edit a part in the group's Inspector and it becomes an [override](term:override): the group keeps its own value. Press Revert to set to follow the set again.

<details><summary>Why it works</summary>

A group names its set and still stores its own lists, which the loader reads, so changing a set only rewrites groups ([design decision 8](../../../../docs/adr/0008-scene-sets-fill-groups-and-overrides-persist.md)). Sets live in Project Settings ([design decision 2](../../../../docs/adr/0002-scene-sets-live-in-project-settings.md)).

</details>

## Reset shared scenes <!-- level 1 -->

Tick [Reset shared scenes](term:reset-shared-scenes) when a shared scene holds per-run state, such as a timer or the player, that must start fresh on each scene change.

## Close previous manager scenes <!-- level 2 -->

Tick [Close previous manager scenes](term:close-previous-manager-scenes) on a group whose game mode needs different managers. It closes manager scenes that are not in this group's scene set.

## Scene status <!-- level 0 -->

Every scene field shows a [scene status](term:scene-status): In build, Addressable, Not in build or Missing. A scene is In build or Addressable, never both. See [Build Settings and Addressables](build-settings-and-addressables.md).
