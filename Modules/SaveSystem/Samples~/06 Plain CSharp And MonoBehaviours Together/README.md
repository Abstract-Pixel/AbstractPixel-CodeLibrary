# 06 Plain CSharp And MonoBehaviours Together

A plain C# quest log and MonoBehaviour pickups saving side by side, each with its own save ID.

## Before you start

- Make a save system config with **Tools > External Project Tools > Save System > Create Config**.
- This sample brings one save file, `SamplePickupsSaveFile` in Scene. The editor adds it to the config after the next compile. The quest log uses the built-in **GameProfile** file.
- To make the "Reload scene" button work, add the **Plain CSharp And MonoBehaviours Together** scene to File > Build Profiles > Scene List.

## Try it

1. Open the **Plain CSharp And MonoBehaviours Together** scene and press Play.
2. Pick up one or two items. Each one adds a quest to the quest log. Press **Save**.
3. Press **Reload scene**, or stop and press Play again.

You should see the same pickups collected and the same quests in the log. **Reset sample** puts everything back.

## The lines that matter

```csharp
SaveActions.Register(this, "quest-log");                                     // QuestLog: plain C#, built-in GameProfile file
public class Pickup : MonoBehaviour, ISavable<bool, SamplePickupsSaveFile>  // each pickup has its own Savable Bridge and save ID
questLog.Complete($"Found the {displayName}");                              // a MonoBehaviour updating the plain C# object
SaveActions.Unregister(QuestLog);                                            // the owner unregisters the plain C# object
```

The two kinds don't need to know about each other's saving. Each has its own save ID, and one Save writes both.
