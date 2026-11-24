# SceneSetPart

<!-- layout new -->

An enum: the three parts of a scene group that its scene set fills, and that can each be an override. You pass it to `SceneGroup.IsOverride` to ask whether one part keeps the group's own value.

<!-- type-header AbstractPixel.SceneManagement.SceneSetPart -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/Configuration/SceneSetPart.cs" "Select the script" -->

Read more in [Scene groups and scene sets](../guide/scene-groups-and-scene-sets.md#overrides).

## Start here

Pass a value to `SceneGroup.IsOverride` to ask whether that part keeps the group's own value.

| You want to | Use | Why |
|---|---|---|
| Ask about manager scenes | `ManagerScenes` | The group's Manager scenes. |
| Ask about shared scenes | `SharedScenes` | The group's Shared scenes. |
| Ask about the reset choice | `ResetSharedScenes` | The group's Reset shared scenes. |

## In code

Each example is closed; open the one you need.

**Example: Ask whether one part of a group keeps its own value**

```cs
bool ownShared = sceneGroup.IsOverride(SceneSetPart.SharedScenes);
if (ownShared)
{
    // This group does not follow its scene set's shared scenes.
}
```

<!-- members AbstractPixel.SceneManagement.SceneSetPart -->
