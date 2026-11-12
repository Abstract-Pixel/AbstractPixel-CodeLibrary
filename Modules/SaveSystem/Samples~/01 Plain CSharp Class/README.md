# 01 Plain CSharp Class

An `Inventory` class with no MonoBehaviour saves with one line: `SaveSystem.Register`.

## Before you start

- The project needs a save system config named `SaveSystemConfig` in a `Resources` folder (Assets > Create > Utility > SaveSystemConfigSO). It needs these entries:
  - `Game` in `GameProfile`
- To make the "Reload scene" button work, add the **Plain CSharp Class** scene to File > Build Profiles > Scene List.

## Try it

1. Open the **Plain CSharp Class** scene and press Play.
2. Press **Find an item** a few times, then **Save**.
3. Press **Reload scene**, or stop and press Play again.

You should see the same items come back.

## The lines that matter

```csharp
[Savable(SaveCategory.Game)]                         // which save file: Game lives in the game profile
public class Inventory : ISavable<InventoryData>     // CaptureData and RestoreData move the data
SaveSystem.Register(this, "player-inventory");       // the last line of the constructor
SaveSystem.Unregister(Inventory);                    // the owner calls this when the scene ends
```

Register last in the constructor: if saved data is already loaded, it's restored inside that call.
