# LaunchSceneStartUpLog

<!-- layout new -->

Writes start-up milestones of a launch scene to the log (Player.log in builds).

<!-- type-header AbstractPixel.SceneManagement.SceneScreens.LaunchSceneStartUpLog -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/SceneScreens/Runtime/LaunchScene/LaunchSceneStartUpLog.cs" "Select the script" -->

Read more in [Start up](../guide/start-up.md#launch-scene).

## Start here

Use it to measure how long start-up takes in a build.

| You want to | Use | Why |
|---|---|---|
| Mark a moment of start-up | `Milestone` | Writes `[Startup] name t=seconds utc=ticks`. |
| Report a start-up problem | `Warning` | Writes a warning starting `[Startup]`. |

- **t:** `Time.realtimeSinceStartup`, in seconds.
- **utc:** UTC ticks, so a launcher script can measure from process launch.

## In code

Each example is closed; open the one you need.

**Example: From sample 10**

```cs source=../../../../Samples/10 Launch Scene/Scripts/Sample10Splash.cs
LaunchSceneStartUpLog.Milestone("Sample 10 splash awake");
```

<!-- members AbstractPixel.SceneManagement.SceneScreens.LaunchSceneStartUpLog -->

## See also

- [Start up](../guide/start-up.md)
- [Find a problem](../reference/find-a-problem.md)
