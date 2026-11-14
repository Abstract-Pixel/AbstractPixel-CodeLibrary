# 01 Save A Field

The daily way to save. Put `[Save]` on a field. On a MonoBehaviour that's all. A plain C# class also calls `SaveActions.Register(this)`.

## Before you start

- Make a save system config with **Tools > External Project Tools > Save System > Create Config**.
- This sample uses the built-in **GameProfile** save file. You don't need to add anything to the config.
- To make the "Reload scene" button work, add the **Save A Field** scene to File > Build Profiles > Scene List.

## Try it

1. Open the **Save A Field** scene and press Play.
2. Press **Take 10 damage** a few times and **Find an item** once or twice, then **Save**.
3. Press **Reload scene**, or stop and press Play again.

The health and the backpack come back. "Health read in Start" shows the saved health, not 100.

## The lines that matter

```csharp
[Save] int current = Max;                     // Health, a MonoBehaviour: the Savable Bridge saves it
[Save] List<string> items = new List<string>(); // Backpack, a plain C# class...
SaveActions.Register(this);                   // ...registers itself on the last line of its constructor
SaveActions.Unregister(Backpack);             // its owner unregisters it when the scene ends
```

The **Savable Bridge** on the Player connects `Health` to the save system. The editor adds it for you when a script has a `[Save]` field. The value a field starts with is its new-game value. Saved data is restored after every `Awake` and before any `Start`, so don't reset a saved field in `Start`.

## Next steps

- A pickup that should stay gone: `SaveActions.Forget(gameObject); Destroy(gameObject);`, or the **Save Destroyed** component in sample 02.
- Renaming a saved field: `[Save(Formerly = "hp")] int health;` keeps the old data.
- Renaming a class registered with `Register(this)`: put `[SaveId("Backpack")]` on it first.
- Saving into another file: sample 04. Data that isn't plain fields: sample 06.
