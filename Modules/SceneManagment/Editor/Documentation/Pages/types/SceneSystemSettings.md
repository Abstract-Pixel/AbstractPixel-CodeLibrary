# SceneSystemSettings

<!-- layout new -->

The one scene system settings asset per project: the start scene group and launch scene, the scene sets and the default one, where new scenes go, whether builds check scene groups, and the list of scene groups.

<!-- type-header AbstractPixel.SceneManagement.SceneSystemSettings -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/Configuration/SceneSystemSettings.cs" "Select the script" -->

Read more in [Get started](../guide/get-started.md#run-the-setup).

## Start here

You edit this asset on a Project Settings page, not by hand. Code reads it through `SceneSystemSettingsSource`.

| You want to | Use | Why |
|---|---|---|
| Open the asset's page | Project Settings > Abstract Pixel > Scene System | Every field below is edited there. |
| Read the settings from code | `SceneSystemSettingsSource.Find` | Works in a build and in Play. |
| Know where a failed scene change goes | `FallbackSceneGroup` | The fallback scene group, or the start scene group when none is set. |
| Check for a launch scene | `HasLaunchScene` | True when a launch scene is set. |

**Called for you, leave alone**

- **Where it lives:** any folder; the Editor finds it by type.
- **Preloaded Assets:** the Editor keeps it in Player Settings' Preloaded Assets.
- **Scene groups:** `SceneGroups` lists every scene group in the project, kept current by an editor script.

## In the Inspector

| Field | What it does for you | Change it when |
|---|---|---|
| Start scene group | The scene group the game opens at launch. Start-up needs it, except when Play starts in a main scene. | You change the first place of the game. |
| Launch scene | Optional scene, such as a splash, that Unity opens first. Start-up opens the start scene group behind it. | You add or remove a splash. |
| Fallback scene group | If a scene change fails, go to: the scene group the scene system opens when a scene of the target group fails to load. Empty means the start scene group. | A failed change should land somewhere other than the start scene group. |
| Scene sets | The project's scene sets. | You add a scene set. |
| Default scene set | The scene set start-up builds a temporary scene group from when Play starts in a scene that is in no scene group. | You want another set for loose scenes. |
| New scenes are addressable | On: new scenes go to Addressables. Off: they go to Build Settings. | You switch to Addressables. |
| Check scene groups before build | Whether builds check scene groups. On by default. | Rarely. |
| Scene groups | Every scene group in the project, kept current by an editor script. | Never by hand. |
| Loading scenes | The loading scenes of this project. A scene group that shows a loading scene picks one of them, or the default. | You add a loading scene. |
| Default loading scene | The loading scene a scene group shows when it picked none. | You change the usual loading scene. |
| Loading scene minimum seconds | Minimum time on screen, in unscaled seconds, so a fast load does not flash the loading scene. Counts from Show screen starting. Default 1. | The loading scene flashes or stays too long. |
| Ground layers | The physics layers that count as ground, for Wait for ground below and for adding spawn point positions in the Editor. Triggers never count. Default: everything except Ignore Raycast. | Your ground is on its own layers. |
| No label steepest ground | Steepest ground for relocatables with no label, in degrees (0 to 90, default 35). A steeper spawn point position is allowed but shown as a warning. | Your spawn points sit on steeper ground. |

## In code

Each example is closed; open the one you need.

**Example: From the library's relocation labels**

```cs source=../../../../Relocation/Runtime/RelocationLabels.cs
SceneSystemSettings settings = SceneSystemSettingsSource.Find();
```

<!-- members AbstractPixel.SceneManagement.SceneSystemSettings -->

## See also

- [Get started](../guide/get-started.md)
- [Start up](../guide/start-up.md)
- [Find a problem](../reference/find-a-problem.md)
