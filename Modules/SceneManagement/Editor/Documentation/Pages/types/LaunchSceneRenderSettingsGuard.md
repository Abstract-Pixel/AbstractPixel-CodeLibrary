# LaunchSceneRenderSettingsGuard

<!-- layout new -->

Keeps a launch scene dark while the start scene group opens behind it.

<!-- type-header AbstractPixel.SceneManagement.SceneScreens.LaunchSceneRenderSettingsGuard -->

<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagement/SceneScreens/Runtime/LaunchScene/LaunchSceneRenderSettingsGuard.cs" "Select the script" -->

Read more in [Start up](../guide/start-up.md#launch-scene).

## Start here

Start-up makes the start scene group's main scene active, and the active scene's RenderSettings (ambient, fog, reflections) apply to every scene, so they would light the launch scene. The guard stops that until the hand-off.

| You want to | Use | Why |
|---|---|---|
| Keep the launch scene dark | `BeginGuarding` | Call it when the launch scene wakes. From then on, each active scene change is answered with black settings. |
| Give the start scene group its own look back | `RestoreMenuSettings` | Call it at the hand-off. Safe to call more than once, so also call it on destroy. |

- **On each active scene change:** it remembers the new scene's values and swaps in black ones.
- **At the hand-off:** `RestoreMenuSettings` puts them back, so the start scene group looks as it would without the launch scene.
- **Settings it covers:** ambient mode, ambient light, ambient intensity, ambient probe, reflection intensity and fog.
- **Kind:** a plain C# class, not a component; create it with `new`.

## In code

Each example is closed; open the one you need.

**Example: From sample 10**

```cs source=../../../../Samples/10 Launch Scene/Scripts/Sample10Splash.cs
renderSettingsGuard.BeginGuarding();
```

**Example: From a sample**

```cs source=../../../../Samples/10 Launch Scene/Scripts/Sample10Splash.cs
renderSettingsGuard.RestoreMenuSettings();
```

<!-- members AbstractPixel.SceneManagement.SceneScreens.LaunchSceneRenderSettingsGuard -->
