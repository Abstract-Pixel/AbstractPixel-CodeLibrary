# SceneEventBus

<!-- layout new -->

The scene messages: what a scene change tells listeners, and when. Only the scene system raises them.

<!-- type-header AbstractPixel.SceneManagement.SceneEventBus -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagement/Runtime/Core/SceneEventBus.cs" "Select the script" -->

Read more in [Scene messages](../guide/scene-messages.md#the-messages).

## Start here

Subscribe in `OnEnable` and unsubscribe in `OnDisable`. Most games need only the first two.

| You want to | Use | Why |
|---|---|---|
| Set up a new level | `OnNewSceneGroupTransitionTo` | Once per finished change: every scene of the new group is open and it is the active group. |
| Run something once when the game is ready | `OnGameStarted` | Once per session, right after start-up's `OnNewSceneGroupTransitionTo`. |
| React to a broken scene | `OnSceneChangeFailed` | Passes the group asked for and the scene that failed. |
| Show a "ready" button after a preload | `OnSceneGroupPreloaded` | A preload finished. |
| Know a preload was used | `OnPreloadedSceneGroupActivated` | A change to the preloaded group, after `OnNewSceneGroupTransitionTo`. |
| Clean up the old level | `OnSceneGroupUnloaded` | Passes the group that closed. |
| Act when a main scene is active | `OnMainSceneLoaded` | The loader made it Unity's active scene. |
| Leave a launch scene | `OnLaunchSceneReleased` | Once, when `SceneActions.ReleaseLaunchScene` hands over to the start scene group. |

**Called for you, leave alone**

- **Raise… and ResetStatics:** internal; only the scene system raises scene messages.

## In code

Each example is closed; open the one you need.

**Example: From a sample**

```cs source=../../../../Samples/07 Preload The Next Level/Scripts/Sample07Preloader.cs
SceneEventBus.OnSceneGroupPreloaded += OnPreloaded;
SceneEventBus.OnPreloadedSceneGroupActivated += OnActivated;
```

<!-- members AbstractPixel.SceneManagement.SceneEventBus -->
