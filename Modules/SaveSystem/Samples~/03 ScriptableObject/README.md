# 03 ScriptableObject

A `GameProgress` asset that registers itself in `OnEnable`. A later step of the save system makes this automatic.

## Before you start

- The project needs a save system config. Make one with **Tools > External Project Tools > Save System > Create Config**.
- This sample saves into the built-in **GameProfile** save file, so it needs no row of its own in the save file list.
- To make the "Reload scene" button work, add the **ScriptableObject** scene to File > Build Profiles > Scene List.

## Try it

1. Open the **ScriptableObject** scene and press Play.
2. Press **Complete a level** and **Earn a star**, then **Save**.
3. Stop, then press Play again.

You should see the levels and stars come back.

## The lines that matter

```csharp
public class GameProgress : ScriptableObject, ISavable<GameProgressData>
void OnEnable() => RegisterWithSaveSystem();
public void RegisterWithSaveSystem() => SaveActions.Register(this, "game-progress");
```

In the Editor, Unity can load the asset before play mode starts the save system, so `OnEnable` runs too early. `GameProgressScreen.Awake` registers the asset again to cover that; registering twice is harmless. Values you change on an asset in play mode stay in the Editor's memory after you stop, until the save system restores the saved ones on the next Play.
