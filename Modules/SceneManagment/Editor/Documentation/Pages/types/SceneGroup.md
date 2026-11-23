# SceneGroup

One place the player can be. Its identity is the asset itself: two groups are equal only when they are the same object, even if they name the same main scene (session 4C).

**Kind:** asset · **Namespace:** `AbstractPixel.SceneManagement` · **File:** `SceneGroup.cs`

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/Configuration/SceneGroup.cs" "Select the script" -->

Read more in [Scene groups and scene sets](../guide/scene-groups-and-scene-sets.md#scene-groups).

A runtime group such as a temporary group equals only itself.

## In the Inspector

| Label | Member | What it does |
|---|---|---|
| Main scene | `MainScene` | — |
| Show loading scene | `showLoadingScene` | Show the loading scene: Never; When arriving from another scene group (a Retry goes straight back in without it); or Every time, Retry included. |
| Loading scene | `loadingScene` | The loading scene this group shows. Empty uses the default loading scene from the scene system settings. |
| Is sample | `isSample` | Sample scene group: its scenes open in the Editor but never go into a build (ADR 0014). |
| Sample launch scene | `sampleLaunchScene` | Editor only. Play started from this scene runs the launch scene hold with this sample scene group as the start scene group. |
| Scene set | `sceneSet` | — |
| Manager scenes override | `managerScenesOverride` | — |
| Shared scenes override | `sharedScenesOverride` | — |
| Reset shared scenes override | `resetSharedScenesOverride` | — |

## In code

From a sample:

```cs source=../../../../Samples/07 Preload The Next Level/Scripts/Sample07Preloader.cs
[SerializeField] private SceneGroup nextGroup;
```

## Members

<!-- members AbstractPixel.SceneManagement.SceneGroup -->

## See also

- [Scene groups and scene sets](../guide/scene-groups-and-scene-sets.md)
- [Find a problem](../reference/find-a-problem.md)
