# 01 Plain CSharp Class

An `Inventory` class with no MonoBehaviour saves with one line: `SaveActions.Register`.

## Before you start

- The project needs a save system config. Make one with **Tools > External Project Tools > Save System > Create Config**.
- This sample saves into the built-in **GameProfile** save file, so it needs no row of its own in the save file list.
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

No save file class is named, so the data goes into the built-in GameProfile save file. To pick another save file, name its class: `ISavable<InventoryData, SaveFiles.Bag>`.

Register last in the constructor: if saved data is already loaded, it's restored inside that call.
