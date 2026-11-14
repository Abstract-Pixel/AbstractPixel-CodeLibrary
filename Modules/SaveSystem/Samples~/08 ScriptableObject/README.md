# 08 ScriptableObject

A `GameProgress` asset with `[Save]` fields that registers itself in `OnEnable`. A later step of the save system makes the registering automatic.

## Before you start

- Make a save system config with **Tools > External Project Tools > Save System > Create Config**.
- This sample uses the built-in **GameProfile** save file. You don't need to add anything to the config.
- To make the "Reload scene" button work, add the **ScriptableObject** scene to File > Build Profiles > Scene List.

## Try it

1. Open the **ScriptableObject** scene and press Play.
2. Press **Complete a level** and **Earn a star**, then **Save**.
3. Stop, then press Play again.

The levels and stars come back.

## The lines that matter

```csharp
[Save] public int LevelsCompleted;                                  // saved like any other [Save] field
void OnEnable() => RegisterWithSaveSystem();
public void RegisterWithSaveSystem() => SaveActions.Register(this, "game-progress");
```

In the Editor, Unity can load the asset before play mode starts the save system, so `OnEnable` runs too early. `GameProgressScreen.Awake` registers the asset again to cover that. Registering twice is harmless. Values you change on an asset in play mode stay in the Editor's memory after you stop, until the save system restores the saved ones on the next Play.
