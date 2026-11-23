# 06 Loading Scene
**Level 2 · choices**
You will learn how a scene group shows a loading scene, and how a loading bar made of primitives follows the loading progress.

Covers: F06 Page: Loading scenes section (look only) · F29 Loading scene, LoadingProgress, LoadingProgressView, ILoadingProgressDisplay

## What you need
Unity 6.3 or newer and the scene system set up (Project Settings > Abstract Pixel > Scene System). No extra packages.

## Files
| Folder | What is in it |
|---|---|
| `Scenes/` | `Sample 06 Start`, `Sample 06 Destination` and `Sample 06 Loading` (the loading scene: its own camera with an opaque background and a short far plane, 1000 m up, so it sees only its bar of two cubes) |
| `Scene Groups/` | `Sample 06 StartGroup` (Show the loading scene: Never) and `Sample 06 DestinationGroup` (Show the loading scene: When arriving from another scene group, loading scene `Sample 06 Loading`) |
| `Prefabs/` | `Sample 06 Tower`: a cylinder with a cube on top |
| `Materials/` | `Sample 06 Start`, `Sample 06 Destination`, `Sample 06 Bar Frame`, `Sample 06 Bar` |
| `Scripts/` | `Sample06BarDisplay` (the bar, an `ILoadingProgressDisplay`) and `Sample06Buttons` (the button) |
| `Tests/` | The PlayMode test: the loading scene opens, progress rises to 1 without going down, then it closes |

## Try it
1. Open `Sample 06 Start`: in the toolbar's scene picker, type "Sample 06 Start" and click the row.
2. Press Play.
3. Click "Go to Destination" (top right).
4. Click "Go to Start".
5. Stop Play. Select `Sample 06 DestinationGroup` in `Scene Groups/` and read "Show the loading scene" and "Loading scene" in its Inspector.
6. Open Project Settings > Abstract Pixel > Scene System and read the Loading scenes section, with "Minimum time on screen" (look only: the sample does not change it).

## You should see
- After step 3 the screen fades out, the loading scene appears (a dark screen with a grey frame and a green bar), the bar grows from left to right, flashes once when full, the screen fades again, and Destination (light blue) appears.
- After step 4 Start appears with no loading scene: Start's group shows it Never.
- In step 5 Destination's group names `Sample 06 Loading` on its own field; the project's loading scenes list is not touched.

## The lines that matter
`Scripts/Sample06BarDisplay.cs`: the view calls SetProgress every frame with the smoothed progress; the bar grows from the left.
```csharp
public void SetProgress(float _progress)
{
    ShownProgress = _progress;
    float width = Mathf.Max(0.01f, fullWidth * _progress);
    Vector3 scale = bar.localScale;
    scale.x = width;
    bar.localScale = scale;
```

`Scripts/Sample06BarDisplay.cs`: the display is a plain interface, so any look written in code can be one.
```csharp
public class Sample06BarDisplay : MonoBehaviour, ILoadingProgressDisplay
```

## Windows this sample shows
- The scene group inspector: "Show the loading scene" and "Loading scene" (select `Sample 06 DestinationGroup`).
- LoadingProgressView: select `Loading Bar` in `Sample 06 Loading` (its feel settings and four UnityEvents).
- Loading scenes section and Minimum time on screen: Project Settings > Abstract Pixel > Scene System (look only).

## If it goes wrong
- No loading scene appears → the group's "Show the loading scene" is Never: pick "When arriving from another scene group".
- The bar does not move → `Loading Bar` lost its `LoadingProgressView`, or its Bar field is empty.
- The game shows through the loading scene → its camera needs an opaque (Solid Color) background and must not see the other scenes (here: far away, short far plane).

## Next steps
- [07 Preload The Next Level](../07%20Preload%20The%20Next%20Level/README.md): load the next level before you go there.
- [10 Launch Scene](../10%20Launch%20Scene/README.md): the screen Play starts with.
