# SceneSystemSettingsSource

<!-- layout new -->

Finds the scene system settings at runtime, in a build and in Play.

<!-- type-header AbstractPixel.SceneManagement.SceneSystemSettingsSource -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/StartUp/SceneSystemSettingsSource.cs" "Select the script" -->

Read more in [Start up](../guide/start-up.md#pick-the-start-scene-group).

## Start here

One call: `Find` returns the project's `SceneSystemSettings`, or null when there are none.

| You want to | Use | Why |
|---|---|---|
| Read the settings from code | `Find` | The same lookup start-up uses. Check for null. |

**Called for you, leave alone**

- **In a build:** Player Settings' Preloaded Assets loads the settings before the first scene.
- **In the Editor:** the Editor does not load Preloaded Assets for Play, so the scene system's editor code registers a lookup that finds the asset by type.

## In code

From the library's relocation labels:

```cs source=../../../../Relocation/Runtime/RelocationLabels.cs
SceneSystemSettings settings = SceneSystemSettingsSource.Find();
```

<!-- members AbstractPixel.SceneManagement.SceneSystemSettingsSource -->

## See also

- [Start up](../guide/start-up.md)
- [Find a problem](../reference/find-a-problem.md)
