# 01 Plain CSharp Class

An `Inventory` class with no MonoBehaviour saves with one line: `SaveActions.Register`.

## Before you start

- Make a save system config with **Tools > External Project Tools > Save System > Create Config**.
- This sample uses the built-in **GameProfile** save file. You don't need to add anything to the config.
- To make the "Reload scene" button work, add the **Plain CSharp Class** scene to File > Build Profiles > Scene List.

## Try it

1. Open the **Plain CSharp Class** scene and press Play.
2. Press **Find an item** a few times, then **Save**.
3. Press **Reload scene**, or stop and press Play again.

You should see the same items come back.

## The lines that matter

```csharp
public class Inventory : ISavable<InventoryData>     // CaptureData and RestoreData move the data
SaveActions.Register(this, "player-inventory");       // the last line of the constructor
SaveActions.Unregister(Inventory);                    // the owner calls this when the scene ends
```

With no save file class named, the data goes into the GameProfile file. To use another file, add its class, such as `ISavable<InventoryData, SaveFiles.Bag>`.

Register on the last line of the constructor. Saved data may arrive inside that call.
