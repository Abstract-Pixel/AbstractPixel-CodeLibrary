# Preload <!-- level 2 -->

[Preload](term:preload) opens the next [scene group](term:scene-group) in the background, so the [scene change](term:scene-change) to it is fast. The scene system never preloads by itself.

## Preload a scene group <!-- level 2 -->

Call `SceneActions.Preload(group)`, then later `SceneActions.ChangeTo` the same group.

**Try it in the sample**

<!-- action open-sample 07 "Open sample 07" -->

1. Press Play. <!-- done-when OnGameStarted -->
2. Press "Preload Next Level". <!-- done-when OnSceneGroupPreloaded -->
3. Press "Go to Next Level". <!-- done-when OnPreloadedSceneGroupActivated -->

**Or do it yourself**

```cs source=lib:Modules/SceneManagment/Samples/07 Preload The Next Level/Scripts/Sample07Preloader.cs
public Task<SceneChangeResult> PreloadNext()
{
    return SceneActions.Preload(nextGroup);
}
```

> [!TIP]
> `OnSceneGroupPreloaded`, then a fast change and `OnPreloadedSceneGroupActivated`.

> [!WARNING]
> While a preloaded main scene waits, Unity holds every later scene load. A change to another group closes the preload first.

- At most one scene group is preloaded; a new preload closes the old one.
- Preload never touches the screen and never shows a loading scene.
- A failed preload closes what it opened and goes nowhere.

<details><summary>Why it works</summary>

Only the main scene waits (held at 90%); its manager and shared scenes open fully, so Unity's hold blocks as little as possible.

</details>

<details><summary>If it goes wrong</summary>

- The change is not faster → it went to a different group than the one preloaded.
- Refused → the group is open already, start-up is running, or a change is running.

</details>
