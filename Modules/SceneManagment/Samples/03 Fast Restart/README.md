# 03 Fast Restart
**Level 1 · daily code**
You will learn how Retry restarts the open scene group, and how Reset shared scenes gives each run fresh state.

Covers: F26 Retry and Reset shared scenes

## What you need
Unity 6.3 or newer and the scene system set up (Project Settings > Abstract Pixel > Scene System). No extra packages.

## Files
| Folder | What is in it |
|---|---|
| `Scenes/` | `Sample 03 Arena` (main scene, with the Retry buttons) and `Sample 03 Run State` (shared scene, holds the score) |
| `Scene Groups/` | `Sample 03 ArenaGroup`: shared scene `Sample 03 Run State`, Reset shared scenes on, Show the loading scene: Never |
| `Prefabs/` | `Sample 03 Flag`: a pole and a cloth |
| `Materials/` | `Sample 03 Arena` (green) |
| `Scripts/` | `Sample03RunState` (the score) and `Sample03RetryButton` (the buttons) |
| `Tests/` | The PlayMode test: Retry gives Done and a fresh score; twice fast gives Busy |

## Try it
1. Open `Sample 03 Arena`: in the toolbar's scene picker, type "Sample 03 Arena" and click the row.
2. Press Play and wait until the score reaches 3 or more.
3. Click "Retry" (top right).
4. Click "Retry twice fast".
5. Stop Play. Select `Sample 03 ArenaGroup` in `Scene Groups/`, open Advanced in its Inspector and look at "Reset shared scenes" (ticked).

## You should see
- After step 2 the label counts "Score this run: 1, 2, 3 …" once a second.
- After step 3 the screen fades, the label says "Last result: Retry: Done" and the score has started again from 0 (it keeps counting while the screen fades in) and the run number went up by one.
- After step 4 the label says "Retry twice fast: first Done, second Refused (Busy)".
- In step 5 "Reset shared scenes" is ticked, which is why the score scene reopens on every Retry.

## The lines that matter
`Scripts/Sample03RetryButton.cs`: Retry is one call; it returns the same result type as a change.
```csharp
public async Task<SceneChangeResult> RetryOnce()
{
    SceneChangeResult result = await SceneActions.Retry();
    ShowResults("Retry: " + result);
    return result;
}
```

`Scripts/Sample03RunState.cs`: the per-run state lives in the shared scene, so it needs no reset code of its own.
```csharp
private void Update()
{
    secondsThisRun += Time.unscaledDeltaTime;
    score = Mathf.FloorToInt(secondsThisRun);
    ShowScore();
}
```

## Windows this sample shows
- The scene group inspector, Advanced, "Reset shared scenes": select `Sample 03 ArenaGroup`.

## If it goes wrong
- The score does not start over → "Reset shared scenes" is off on the group: tick it.
- "Refused (NothingOpenToRetry)" → no scene group is open yet: wait for start-up after Play.
- "Refused (Busy)" on a single click → the last Retry was still fading.

## Next steps
- [02 Change Level From Code](../02%20Change%20Level%20From%20Code/README.md): the result types in detail.
- [05 Persistent Player And Spawn Points](../05%20Persistent%20Player%20And%20Spawn%20Points/README.md): keep objects across changes instead.
