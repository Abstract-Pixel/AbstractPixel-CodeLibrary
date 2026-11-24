# SceneSet

<!-- layout new -->

The default manager scenes, shared scenes and Reset shared scenes for the scene groups that use it. An asset.

<!-- type-header AbstractPixel.SceneManagement.SceneSet -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/Configuration/SceneSet.cs" "Select the script" -->

Read more in [Scene groups and scene sets](../guide/scene-groups-and-scene-sets.md#scene-sets).

## Start here

Edit scene sets on the Scene System page, not in code. A scene set fills its groups in the Editor (ADR 0008); the loader still reads each group's own lists.

| You want to | Use | Why |
|---|---|---|
| Give many groups the same scenes | A scene set picked in each group's Scene set field | Change the set once and every group follows, except for its overrides. |
| Read a set's scenes in an Editor tool | `ManagerScenes`, `SharedScenes`, `ResetSharedScenes` | The three parts a set fills. |

## In the Inspector

The set's inspector also says how many scene groups use it and has an Open Scene System button.

| Field | What it does for you | Change it when |
|---|---|---|
| Manager scenes | The manager scenes its groups get. | Every group of this set needs another manager scene. |
| Shared scenes | The shared scenes its groups get. | Every group of this set needs another shared scene. |
| Reset shared scenes | Whether its groups reset the shared scenes on each scene change. | A shared scene holds per-run state. |

<!-- members AbstractPixel.SceneManagement.SceneSet -->

## See also

- [Scene groups and scene sets](../guide/scene-groups-and-scene-sets.md)
- [Find a problem](../reference/find-a-problem.md)
