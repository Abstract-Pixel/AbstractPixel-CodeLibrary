# 07 Preload The Next Level

<!-- layout new -->

**Level 2 · choices**
You will learn how to load the next scene group in the background, so the change to it only activates what is already loaded.

Covers: F30 Preload and the preloaded switch

## What you need
Unity 6.3 or newer and the scene system set up (Project Settings > Abstract Pixel > Scene System). No extra packages.

## Files
| Folder | What is in it |
|---|---|
| `Scenes/` | `Sample 07 Menu` and `Sample 07 Next Level`, each with the `Sample 07 Preloader` buttons |
| `Scene Groups/` | `Sample 07 MenuGroup` and `Sample 07 Next LevelGroup` |
| `Prefabs/` | `Sample 07 Arch`: two columns, a beam and a keystone |
| `Materials/` | `Sample 07 Menu` (pink) and `Sample 07 Next Level` (green) |
| `Scripts/` | `Sample07Preloader`: Preload, Go, and what happened |
| `Tests/` | The PlayMode test: preload, change, one copy of the next level |

## Try it
1. Open `Sample 07 Menu`: in the toolbar's scene picker, type "Sample 07 Menu" and click the row.
2. Press Play. <!-- done-when OnGameStarted -->
3. Click "Preload Next Level" (top right). <!-- done-when OnSceneGroupPreloaded -->
4. Click "Go to Next Level". <!-- done-when OnPreloadedSceneGroupActivated -->
5. In the Next Level, click "Go to Menu" without preloading, and compare the time. <!-- done-when OnNewSceneGroupTransitionTo -->

## You should see
<!-- timeline -->

- After step 3 the label says "PreloadedSceneGroup: Sample 07 Next LevelGroup" and "OnSceneGroupPreloaded (Sample 07 Next LevelGroup)". The Menu stays on screen.
- After step 4 the screen fades to the green Next Level, the label adds "OnPreloadedSceneGroupActivated (Sample 07 Next LevelGroup)", "PreloadedSceneGroup: none" and "ChangeTo took … ms (Done)".
- After step 5 the Menu appears with no new OnPreloadedSceneGroupActivated line. Here both times are about the same, because these scenes are tiny and the fade takes most of it; with a big level the preloaded change is the short one.

## The lines that matter
`Scripts/Sample07Preloader.cs`: preload is one call; it never touches the screen.
```csharp
public Task<SceneChangeResult> PreloadNext()
{
    return SceneActions.Preload(nextGroup);
}
```

`Scripts/Sample07Preloader.cs`: the change is the usual ChangeTo; the scene system sees the group is preloaded and switches to it.
```csharp
Stopwatch watch = Stopwatch.StartNew();
SceneChangeResult result = await SceneActions.ChangeTo(nextGroup);
watch.Stop();
```

`Scripts/Sample07Preloader.cs`: the two preload messages.
```csharp
SceneEventBus.OnSceneGroupPreloaded += OnPreloaded;
SceneEventBus.OnPreloadedSceneGroupActivated += OnActivated;
```

## Windows this sample shows
- **The preloader's fields (next group, its name, label):** select `Sample 07 Preloader` in the Hierarchy.
- **While a group is preloaded, its main scene shows in the Hierarchy as loading:** look at it after step 3.

## How it's set up
1. Select the object with `Sample07Preloader`.
   <!-- action select-in-scene "Packages/com.abstractpixel.library/Modules/SceneManagment/Samples/07 Preload The Next Level/Scenes/Sample 07 Menu.unity" "Sample 07 Preloader" "Select the preloader" -->
2. Select the script that calls `SceneActions.Preload`.
   <!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Samples/07 Preload The Next Level/Scripts/Sample07Preloader.cs" "Select the script" -->
3. Select the next level's scene group.
   <!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Samples/07 Preload The Next Level/Scene Groups/Sample 07 Next LevelGroup.asset" "Select the Next Level group" -->

## Why it works
- [Preload](../../Editor/Documentation/Pages/guide/preload.md#preload-a-scene-group) opens the next group in the background and holds only its main scene.
- A change to the preloaded group switches to it instead of loading again.

## If it goes wrong
- **"Refused (Busy)" on Preload:** a change is running: preload after it ends.
- **No OnPreloadedSceneGroupActivated:** the change went to a different group than the preloaded one; that preload was closed first.
- **Two copies of a scene:** report it: the preloaded switch opens the held main scene once.

## Next steps
- **[06 Loading Scene](../06%20Loading%20Scene/README.md):** show progress while a group loads.
- **[08 Addressables](../08%20Addressables/README.md):** scenes that load through Addressables.
