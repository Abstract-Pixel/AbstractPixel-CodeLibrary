# 06 Custom Data With ISavable

When `[Save]` fields aren't enough, a class implements `ISavable<TData>` and moves its data in and out itself. This `Inventory` is a plain C# class that saves a copy of its items.

Use it when the saved shape differs from the live fields, when the data needs converting on the way in or out, or when the fields hold things JSON can't store. For everything else, `[Save]` (sample 01) is shorter.

## Before you start

- Make a save system config with **Tools > External Project Tools > Save System > Create Config**.
- This sample uses the built-in **GameProfile** save file. You don't need to add anything to the config.
- To make the "Reload scene" button work, add the **Custom Data With ISavable** scene to File > Build Profiles > Scene List.

## Try it

1. Open the **Custom Data With ISavable** scene and press Play.
2. Press **Find an item** a few times, then **Save**.
3. Press **Reload scene**, or stop and press Play again.

The same items come back.

## The lines that matter

```csharp
public class Inventory : ISavable<InventoryData>      // CaptureData and RestoreData move the data
public InventoryData CaptureData()                     // hand the save system a copy
public void RestoreData(InventoryData _loadedData)     // put the saved data back
SaveActions.Register(this, "player-inventory");       // the last line of the constructor; the ID is optional
SaveActions.Unregister(Inventory);                    // the owner calls this when the scene ends
```

With no save file class named, the data goes into the GameProfile file. To use another file, add its class: `ISavable<InventoryData, SaveFiles.Scene>`. A class uses either `ISavable` or `[Save]` fields, never both; the editor blocks play mode if it finds both.
