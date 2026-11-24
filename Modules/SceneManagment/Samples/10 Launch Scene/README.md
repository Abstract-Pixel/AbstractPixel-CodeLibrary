# 10 Launch Scene

<!-- layout new -->

**Level 2 · choices**
You will learn how a launch scene (a splash) holds the first scene group while it opens behind it, and how to hand over with a Timeline.

Covers: F03 Page: Start-up section (look only) · F32 Launch scene hold and its SceneActions calls · F33 Launch scene helpers (Marker, Receiver, TimelineClock, RenderSettingsGuard, StartUpLog)

## What you need
Unity 6.3 or newer and the scene system set up. The Timeline part needs the Timeline package (com.unity.timeline); without it the splash shows "Sample 10's Timeline part needs the Timeline package (com.unity.timeline)." and releases after two seconds.

## Files
| Folder | What is in it |
|---|---|
| `Scenes/` | `Sample 10 Splash` (the launch scene: its own black camera 2000 m up, a logo of cubes, the Splash object) and `Sample 10 Menu` (main scene of the sample group) |
| `Scene Groups/` | `Sample 10 MenuGroup`: "Launch scene (sample only)" is `Sample 10 Splash` |
| `Prefabs/` | `Sample 10 Logo`: two posts, a bar and a dot |
| `Materials/` | `Sample 10 Logo` (gold) and `Sample 10 Menu` (blue) |
| `Timelines/` | `Sample 10 Splash`: 2.5 s, with a LaunchSceneReleaseMarker at 2 s |
| `Scripts/` | `Sample10Splash` (reveal, sting, clock, guard, hand-off) and `Sample10MenuIntro` (waits for the release) |
| `Tests/` | The PlayMode test: without the splash, nothing is held and the intro runs once. The Play-from-splash test and the start-up rule check are in `Shared/Tests` (`Sample10LaunchSceneTests`) |

## Try it
1. Open `Sample 10 Splash`: in the toolbar's scene picker, type "Sample 10 Splash" and click the row.
2. Select `Scene Groups/Sample 10 MenuGroup` and find "Launch scene (sample only)" under "Sample scene group": it names `Sample 10 Splash`.
3. Press Play, with `Sample 10 Splash` as the only open scene. <!-- done-when OnGameStarted -->
4. Wait about two seconds. <!-- done-when OnLaunchSceneReleased -->
5. Stop Play. Open Project Settings > Abstract Pixel > Scene System and read the Start-up section (look only: a real game sets its launch scene and start scene group there; samples never change it).

## You should see
<!-- timeline -->

- After step 3 a black screen with the gold logo growing in, a short tone when it is fully shown, and the splash label. The Menu is already open behind it (Hierarchy), held.
- After step 4 the splash closes and the blue Menu appears; its label says "IsLaunchSceneHeld: false (the intro ran 1 time)".
- The Console shows "Start-up: the launch scene 'Sample 10 Splash' holds the start scene group …", "'Sample 10 Splash' is the sample launch scene of 'Sample 10 MenuGroup' …", the [Startup] milestones "Sample 10 splash awake" and "Sample 10 logo revealed", and "Launch scene released: the start scene group takes over."

## The lines that matter
`Scripts/Sample10MenuIntro.cs`: the Menu's intro waits for the release; with nothing held it runs at once.
```csharp
private void OnEnable()
{
    SceneActions.RunWhenLaunchSceneReleased(StartIntro);
}
```

`Scripts/Sample10Splash.cs`: the guard keeps the Menu's RenderSettings off the splash, and the clock caps the Timeline's step on a load hitch.
```csharp
renderSettingsGuard.BeginGuarding();
LaunchSceneStartUpLog.Milestone("Sample 10 splash awake");
#if ABSTRACTPIXEL_TIMELINE
clock = new LaunchSceneTimelineClock(timeline, MaximumStepSeconds);
```

`Scripts/Sample10Splash.cs`: the LaunchSceneReleaseReceiver's On Released calls this after it released the hold.
```csharp
released = true;
HandOverCount++;
renderSettingsGuard.RestoreMenuSettings();
splashCamera.enabled = false;
```

## Windows this sample shows
- **The scene group inspector, "Launch scene (sample only)":** select `Sample 10 MenuGroup`.
- **The Timeline window with the release marker:** select the `Splash` object, Window > Sequencing > Timeline.
- **LaunchSceneReleaseReceiver and its On Released list:** select the `Splash` object.
- **Start-up section:** Project Settings > Abstract Pixel > Scene System (look only).

## How it's set up
1. Select the menu's scene group: its Launch scene (sample only) names the splash.
   <!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Samples/10 Launch Scene/Scene Groups/Sample 10 MenuGroup.asset" "Select the Menu group" -->
2. Select the splash object, which releases the launch scene hold.
   <!-- action select-in-scene "Packages/com.abstractpixel.library/Modules/SceneManagment/Samples/10 Launch Scene/Scenes/Sample 10 Splash.unity" "Splash" "Select the splash" -->
3. Select `Menu Intro`; it waits with `SceneActions.RunWhenLaunchSceneReleased`.
   <!-- action select-in-scene "Packages/com.abstractpixel.library/Modules/SceneManagment/Samples/10 Launch Scene/Scenes/Sample 10 Menu.unity" "Menu Intro" "Select Menu Intro" -->
4. Open the Start-up section.
   <!-- action open-settings "Start-up" "Open Start-up" -->

## Why it works
- The [launch scene hold](../../Editor/Documentation/Pages/guide/start-up.md#launch-scene) keeps the start scene group back while the splash plays ([design decision 9](../../docs/adr/0009-start-up-opens-the-start-scene-group-behind-a-launch-scene.md)).
- The menu waits with `SceneActions.RunWhenLaunchSceneReleased`, so it starts once, at the hand-off.

## If it goes wrong
- **The Menu opens with no splash:** Play was started from another scene: open only `Sample 10 Splash`.
- **The splash never closes:** the Timeline lost its marker, or the director has no Timeline: check the `Splash` object's PlayableDirector.
- **Two audio listeners warning:** the splash must not add a listener of its own; the Menu's camera has the one listener.

## Next steps
- **[04 Play From Any Scene](../04%20Play%20From%20Any%20Scene/README.md):** what Play opens without a launch scene.
- **[06 Loading Scene](../06%20Loading%20Scene/README.md):** the other screen the scene system can show.
