# SceneEventBus

The scene messages: what a scene change tells listeners, and when. Only the scene system raises them.

**Kind:** class · **Namespace:** `AbstractPixel.SceneManagement` · **File:** `SceneEventBus.cs`

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Runtime/Core/SceneEventBus.cs" "Select the script" -->

Read more in [Scene messages](../guide/scene-messages.md#the-messages).

## In code

From a sample:

```cs source=../../../../Samples/07 Preload The Next Level/Scripts/Sample07Preloader.cs
SceneEventBus.OnSceneGroupPreloaded += OnPreloaded;
SceneEventBus.OnPreloadedSceneGroupActivated += OnActivated;
```

## Members

<!-- members AbstractPixel.SceneManagement.SceneEventBus -->

## See also

- [Scene messages](../guide/scene-messages.md)
- [Find a problem](../reference/find-a-problem.md)
