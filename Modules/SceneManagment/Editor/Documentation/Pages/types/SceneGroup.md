# SceneGroup

<!-- layout new -->

One place the player can be: a main scene with its manager scenes, shared scenes and loading scene choice. An asset.

<!-- type-header AbstractPixel.SceneManagement.SceneGroup -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/Configuration/SceneGroup.cs" "Select the script" -->

Read more in [Scene groups and scene sets](../guide/scene-groups-and-scene-sets.md#scene-groups).

## Start here

You make scene groups in the Project window and pass them to `SceneActions.ChangeTo`. Most of a group is set in its Inspector, not in code.

| You want to | Use | Why |
|---|---|---|
| Name a place in your script | A `SceneGroup` field | Drag the asset in, then pass it to `SceneActions.ChangeTo` or `SceneActions.Preload`. |
| Pick when the loading scene shows | `ShowLoadingScene` | Same as the Show the loading scene field. See `ShowLoadingScene` for the values. |
| Pick which loading scene shows | `LoadingScene` | Empty uses the default loading scene from the scene system settings. |
| Know if a part follows the scene set | `IsOverride` | True when that part keeps the group's own value. |
| Check a group has no scenes | `IsEmpty` | True when it has no main scene, no manager scenes and no shared scenes. |

**Called for you, leave alone**

- **Initialize:** for groups made at run time, such as a `TemporarySceneGroup` or one read back from `SceneGroupData`.
- **SetOverride and SceneSet:** the Inspector sets them when you edit a part or pick a scene set.

**Identity**

- **Equal when:** the two are the same asset, even if two groups name the same main scene (session 4C).
- **Runtime group:** a group made at run time, such as a temporary group, equals only itself.

## In the Inspector

What you see when you select a scene group asset.

| Field | What it does for you | Change it when |
|---|---|---|
| Main scene | The scene the group is about. A group without one cannot open. | Always: every group needs one. |
| Scene set | The scene set that fills the three Advanced parts. | Many groups should share the same manager and shared scenes. |
| Show the loading scene | Never; When arriving from another scene group (a Retry goes straight back in without it); or Every time, Retry included. | The change into this group needs a loading scene. |
| Loading scene | The loading scene this group shows. Default uses the default loading scene from the scene system settings. Shown when Show the loading scene is not Never. | This group needs a different loading scene. |
| Sample scene group | Its scenes open in the Editor but never go into a build (ADR 0014). | Only for library samples. |
| Launch scene (sample only) | Editor only. Play started from this scene runs the launch scene hold with this sample scene group as the start scene group. Shown only while Sample scene group is ticked. | A sample has a launch scene. |
| **Advanced** | | |
| Manager scenes | Stored as `ManagerialBootScenesList`. Filled by the scene set unless it is an override. | This group needs other manager scenes than its set. |
| Shared scenes | Stored as `ContextualBootScenesList`. Filled by the scene set unless it is an override. | This group needs other shared scenes than its set. |
| Reset shared scenes | Stored as `ForceReloadContextualScenes`. Filled by the scene set unless it is an override. | A shared scene holds per-run state that must start fresh. |

**Overrides**

- **Stored as:** `managerScenesOverride`, `sharedScenesOverride` and `resetSharedScenesOverride`, one per `SceneSetPart`.
- **Shown as:** each Advanced part's header says From set, Override with Revert to set, or Own value when the group has no scene set.
- **Loader:** reads the group's own lists, not the scene set or these flags; the Editor keeps the lists in step with the set (ADR 0008).

## In code

Each example is closed; open the one you need.

**Example: From a sample**

```cs source=../../../../Samples/07 Preload The Next Level/Scripts/Sample07Preloader.cs
[SerializeField] private SceneGroup nextGroup;
```

<!-- members AbstractPixel.SceneManagement.SceneGroup -->
