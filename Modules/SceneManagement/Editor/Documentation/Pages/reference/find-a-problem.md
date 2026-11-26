# Find a problem <!-- level 0 -->

<!-- layout new -->

Find what you see in a table, then follow its link to the fix.

## Scene group problems <!-- level 0 -->

The group check lists these in the scene group's Inspector and in the Scene Groups Debugger.

| What you see | Where to fix it |
|---|---|
| No main scene | [Pick one](../guide/scene-groups-and-scene-sets.md#scene-groups) |
| A scene is Missing | [Scene status](../guide/scene-groups-and-scene-sets.md#scene-status) |
| A scene is Not in build | [Fix a scene status](../guide/build-settings-and-addressables.md#fix-a-scene-status) |
| The same scene is listed twice | [Scene groups](../guide/scene-groups-and-scene-sets.md#scene-groups) |
| A scene is used by a sample scene group and a game scene group | [Sample scene groups](../guide/editor-tools.md#sample-scene-groups) |
| A relocation setup mistake in the group's scenes | [Relocation](../guide/relocation.md#what-happens-in-a-scene-change) |

## Scene status <!-- level 0 -->

Every scene field shows one of four statuses.

| What you see | Where to fix it |
|---|---|
| In build | Nothing to fix; [where scenes load from](../guide/build-settings-and-addressables.md#which-loader-opens-a-scene) |
| Addressable | Nothing to fix; [the Addressables loader](../guide/build-settings-and-addressables.md#which-loader-opens-a-scene) |
| Not in build | [Fix a scene status](../guide/build-settings-and-addressables.md#fix-a-scene-status) |
| Missing | [Fix a scene status](../guide/build-settings-and-addressables.md#fix-a-scene-status) |

## Scene changes <!-- level 1 -->

| What you see | Where to fix it |
|---|---|
| A button does nothing | [Change scene from a button](../guide/change-scene.md#change-scene-from-a-button) |
| Refused: Busy, AlreadyOpen or StartUpRunning | [Read the result](../guide/change-scene.md#read-the-result) |
| Failed, and a different scene group opened | [When a scene change fails](../guide/when-a-scene-change-fails.md#what-happens) |
| "No Location found" for an Addressable scene | [Build Settings and Addressables](../guide/build-settings-and-addressables.md#which-loader-opens-a-scene) |
| No fade | [Use the basic fade](../guide/screen-and-transitions.md#use-the-basic-fade) |
| No loading scene | [Show a loading scene](../guide/loading-scene.md#show-a-loading-scene) |
| A preload did not make the change faster | [Preload a scene group](../guide/preload.md#preload-a-scene-group) |

## Start-up <!-- level 1 -->

| What you see | Where to fix it |
|---|---|
| Play opens the wrong group | [Play from any scene](../guide/start-up.md#play-from-any-scene) |
| The menu never shows after the splash | [Launch scene](../guide/start-up.md#launch-scene) |
| The build skips the splash | [Launch scene](../guide/start-up.md#launch-scene) |

## Relocation <!-- level 1 -->

| What you see | Where to fix it |
|---|---|
| An object stays held with an error | [Add spawn points](../guide/relocation.md#add-spawn-points) |
| An object floats or sinks | [Relocation](../guide/relocation.md#what-happens-in-a-scene-change) |

## Editor <!-- level 0 -->

| What you see | Where to fix it |
|---|---|
| No scene picker on the toolbar | [Scene picker](../guide/editor-tools.md#scene-picker) |
| The setup window did not open | [Run the setup](../guide/get-started.md#run-the-setup) |
