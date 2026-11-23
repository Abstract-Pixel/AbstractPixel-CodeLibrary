# SceneSystemSettings

The one scene system settings asset per project: the start scene group and launch scene, the scene sets and the default one, where new scenes go, whether builds check scene groups, and the list of scene groups. It can live in any folder; the Editor finds it by type and keeps it in Player Settings' Preloaded Assets.

**Kind:** asset · **Namespace:** `AbstractPixel.SceneManagement` · **File:** `SceneSystemSettings.cs`

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/Configuration/SceneSystemSettings.cs" "Select the script" -->

Read more in [Get started](../guide/get-started.md#run-the-setup).

Edited in Project Settings > Abstract Pixel > Scene System.

## In the Inspector

| Label | Member | What it does |
|---|---|---|
| Start scene group | `startSceneGroup` | — |
| Launch scene | `launchScene` | — |
| Fallback scene group | `fallbackSceneGroup` | If a scene change fails, go to: the scene group the scene system opens when a scene of the target group fails to load. Empty means the start scene group. |
| Scene sets | `sceneSets` | If a scene change fails, go to: the scene group the scene system opens when a scene of the target group fails to load. Empty means the start scene group. |
| Default scene set | `defaultSceneSet` | If a scene change fails, go to: the scene group the scene system opens when a scene of the target group fails to load. Empty means the start scene group. |
| New scenes are addressable | `newScenesAreAddressable` | If a scene change fails, go to: the scene group the scene system opens when a scene of the target group fails to load. Empty means the start scene group. |
| Check scene groups before build | `checkSceneGroupsBeforeBuild` | — |
| Scene groups | `sceneGroups` | — |
| Loading scenes | `loadingScenes` | The loading scenes of this project. A scene group that shows a loading scene picks one of them, or the default. |
| Default loading scene | `defaultLoadingScene` | The loading scene a scene group shows when it picked none. |
| Loading scene minimum seconds | `loadingSceneMinimumSeconds` | Minimum time on screen, in unscaled seconds, so a fast load does not flash the loading scene. Counts from Show screen starting. |
| Ground layers | `groundLayers` | Ground layers: the physics layers that count as ground, for Wait for ground below and for adding spawn point positions in the Editor. Triggers never count. |
| No label steepest ground | `noLabelSteepestGround` | Steepest ground for relocatables with no label, in degrees. A steeper spawn point position is allowed but shown as a warning. |

## Members

<!-- members AbstractPixel.SceneManagement.SceneSystemSettings -->

## See also

- [Get started](../guide/get-started.md)
- [Find a problem](../reference/find-a-problem.md)
