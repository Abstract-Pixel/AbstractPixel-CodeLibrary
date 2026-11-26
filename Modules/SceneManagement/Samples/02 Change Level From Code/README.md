# 02 Change Level From Code

<!-- layout new -->

**Basic Code**
You will learn how to change the scene group from a script, read the result it returns, and follow the scene messages in order.

Covers: F23 SceneActions.ChangeTo and SceneChangeResult · F24 Scene messages and ScreenState · F25 Failed change and the fallback scene group (look only)

## What you need
Unity 6.3 or newer and the scene system set up (Project Settings > Abstract Pixel > Scene System). No extra packages.

## Files
| Folder | What is in it |
|---|---|
| `Scenes/` | `Sample 02 Hub` and `Sample 02 Arena`: two areas, each with the `Sample 02` object (level changer and message log) |
| `Scene Groups/` | `Sample 02 HubGroup` and `Sample 02 ArenaGroup`: sample scene groups with `Samples Managers` as manager scene |
| `Prefabs/` | `Sample 02 Lamp Post`: a cylinder, a sphere and a cube |
| `Materials/` | `Sample 02 Hub` (blue) and `Sample 02 Arena` (yellow) |
| `Scripts/` | `Sample02LevelChanger` (the buttons) and `Sample02MessageLog` (the message list) |
| `Tests/` | The PlayMode test: the message order of one change, Busy and AlreadyOpen |

## Try it
1. Open `Sample 02 Hub`: in the toolbar's scene picker, type "Sample 02 Hub" and click the row.
2. Press Play. <!-- done-when OnGameStarted -->
3. Click "Change to Arena" (top right of the Game view). <!-- done-when OnNewSceneGroupTransitionTo -->
4. Click "Change twice fast". <!-- done-when OnNewSceneGroupTransitionTo -->
5. Click "Change to the open group".
6. Stop Play. Open Project Settings > Abstract Pixel > Scene System and find "If a scene change fails, go to" (look only: do not change it).

## You should see
<!-- timeline -->

- After step 2 the Hub is blue and the label's log lists "OnNewSceneGroupTransitionTo (Sample 02 HubGroup)" and "OnGameStarted (Sample 02 HubGroup)".
- After step 3 the screen fades out and in, the Arena is yellow, the label says "Last result: Change to Arena: Done" and the log adds "Screen Hiding", "Screen Hidden", "OnMainSceneLoaded (Sample 02 Arena)", "OnNewSceneGroupTransitionTo (Sample 02 ArenaGroup)", "Screen Showing", "Screen Visible", each with its frame number.
- After step 4 you are back in the Hub and the label says "Twice fast: first Done, second Refused (Busy)".
- After step 5 nothing changes and the label says "Change to the open group: Refused (AlreadyOpen)".

The log has no "OnSceneGroupUnloaded": it is sent after the old main scene closed and before the new one opened, when no
`Sample02MessageLog` exists. A listener in a manager or shared scene hears it.

**When a change fails** (look only). If a scene of the group cannot load, the result is `Failed` with that scene,
`OnSceneChangeFailed` is sent once, and the scene system opens the **fallback scene group** ("If a scene change fails, go to"
on the Scene System page; empty means the start scene group) with the screen still covered. Why: [ADR 0011](../../docs/adr/0011-a-failed-scene-change-goes-to-the-fallback-scene-group.md).

## The lines that matter
`Scripts/Sample02LevelChanger.cs`: one change, awaited, with its result.
```csharp source=Scripts/Sample02LevelChanger.cs
public async Task<SceneChangeResult> ChangeToOther()
{
    SceneChangeResult result = await SceneActions.ChangeTo(otherGroup);
    ShowResults("Change to " + otherGroupName + ": " + result);
    return result;
}
```

`Scripts/Sample02LevelChanger.cs`: two changes in one frame; only one runs at a time, so the second is refused.
```csharp source=Scripts/Sample02LevelChanger.cs
Task<SceneChangeResult> first = SceneActions.ChangeTo(otherGroup);
Task<SceneChangeResult> second = SceneActions.ChangeTo(otherGroup);
SceneChangeResult[] results = await Task.WhenAll(first, second);
```

`Scripts/Sample02MessageLog.cs`: listen in OnEnable, and stop in OnDisable.
```csharp source=Scripts/Sample02MessageLog.cs
private void OnEnable()
{
    SceneEventBus.OnNewSceneGroupTransitionTo += OnNewSceneGroupTransitionTo;
    SceneEventBus.OnMainSceneLoaded += OnMainSceneLoaded;
    SceneEventBus.OnSceneGroupUnloaded += OnSceneGroupUnloaded;
```

## Windows this sample shows
- **The level changer's fields (other group, its name, label):** select `Sample 02` in the Hierarchy.
- **"If a scene change fails, go to":** Project Settings > Abstract Pixel > Scene System (look only).
- **The full message list and their order:** the module README, "The messages" and "Order, per entry point".

## How it's set up
1. Select the object with `Sample02LevelChanger`; its Other Group field names the Arena's group.
   <!-- action select-in-scene "Packages/com.abstractpixel.library/Modules/SceneManagement/Samples/02 Change Level From Code/Scenes/Sample 02 Hub.unity" "Sample 02" "Select the changer" -->
2. Select the script that calls `SceneActions.ChangeTo`.
   <!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagement/Samples/02 Change Level From Code/Scripts/Sample02LevelChanger.cs" "Select the script" -->
3. Select the Arena's scene group.
   <!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagement/Samples/02 Change Level From Code/Scene Groups/Sample 02 ArenaGroup.asset" "Select the Arena group" -->

## Why it works
- `SceneActions.ChangeTo` returns a [scene change result](../../Editor/Documentation/Pages/guide/change-scene.md#read-the-result) and never throws.
- Busy is set before the first await, so a second change in the same frame is refused.

## If it goes wrong
- **A button logs nothing:** the `Sample 02` object lost its label: drag `Sample Label` into both of its Label fields.
- **"Refused (Busy)" on a single click:** the last change was still fading: wait for it to end.
- **"Refused (StartUpRunning)":** start-up has not finished yet: wait a moment after Play.

## Next steps
- **[03 Fast Restart](../03%20Fast%20Restart/README.md):** Retry the open scene group.
- For level order, see the LevelManager module.
