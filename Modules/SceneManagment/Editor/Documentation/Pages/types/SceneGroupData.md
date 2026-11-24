# SceneGroupData

<!-- layout new -->

A save-file copy of a scene group: the names of its scenes, used to remember a scene group in saved data.

<!-- type-header AbstractPixel.SceneManagement.SceneGroupData -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/Configuration/SceneGroupData.cs" "Select the script" -->

Read more in [Scene groups and scene sets](../guide/scene-groups-and-scene-sets.md#scene-groups).

## Start here

Use it only to save a scene group and read it back. It converts to and from `SceneGroup` on its own.

| You want to | Use | Why |
|---|---|---|
| Save a scene group | Assign the `SceneGroup` to a `SceneGroupData` | It converts for you and keeps the scene names and Reset shared scenes. |
| Load it back | Assign the `SceneGroupData` to a `SceneGroup` | Calls `ToSceneGroup` and makes a new group at run time. |

**What it keeps**

- **Kept:** main scene, manager scenes and shared scenes by name, and `ForceReloadContextualScenes`.
- **Not kept:** the scene set, overrides, loading scene choice and sample settings.
- **Loaded group:** a new object, so it is not equal to the saved asset (see `SceneGroup`).
- **Empty main scene:** loading logs an error that the saved group's main scene is empty.

## In code

Each example is closed; open the one you need.

**Example: Turn a scene group into plain data to save it, and back**

```cs
SceneGroupData data = sceneGroup;
string json = JsonUtility.ToJson(data);

SceneGroupData loaded = JsonUtility.FromJson<SceneGroupData>(json);
SceneGroup restored = loaded;
```

<!-- members AbstractPixel.SceneManagement.SceneGroupData -->

## See also

- [Scene groups and scene sets](../guide/scene-groups-and-scene-sets.md)
- [Find a problem](../reference/find-a-problem.md)
