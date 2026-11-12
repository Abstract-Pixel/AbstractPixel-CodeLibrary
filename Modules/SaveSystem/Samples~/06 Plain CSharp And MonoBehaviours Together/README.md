# 06 Plain CSharp And MonoBehaviours Together

A plain C# quest log and MonoBehaviour pickups saving side by side, each with its own save ID.

## Before you start

- The project needs a save system config named `SaveSystemConfig` in a `Resources` folder (Assets > Create > Utility > SaveSystemConfigSO). It needs these entries:
  - `Game` in `GameProfile`
  - `Position` in `Scene`
- To make the "Reload scene" button work, add the **Plain CSharp And MonoBehaviours Together** scene to File > Build Profiles > Scene List.

## Try it

1. Open the **Plain CSharp And MonoBehaviours Together** scene and press Play.
2. Pick up one or two items. Each one adds a quest to the quest log. Press **Save**.
3. Press **Reload scene**, or stop and press Play again.

You should see the same pickups collected and the same quests in the log. **Reset sample** puts everything back.

## The lines that matter

```csharp
SaveSystem.Register(this, "quest-log");               // QuestLog: plain C#, saved in the game profile
[Savable(SaveCategory.Position)] public class Pickup  // each pickup has its own Savable Bridge and save ID
questLog.Complete($"Found the {displayName}");       // a MonoBehaviour updating the plain C# object
SaveSystem.Unregister(QuestLog);                      // the owner unregisters the plain C# object
```

The two kinds don't need to know about each other's saving. Each has its own save ID, and one Save writes both.
