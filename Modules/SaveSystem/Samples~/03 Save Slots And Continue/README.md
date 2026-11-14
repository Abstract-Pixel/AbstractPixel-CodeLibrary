# 05 Game Profiles

A simple save slot screen: create, open, list and delete game profiles. Each one shows its manifest's name, last saved time and playtime.

## Before you start

- Make a save system config with **Tools > External Project Tools > Save System > Create Config**.
- This sample uses the built-in **GameProfile** save file. You don't need to add anything to the config.
- To make the "Open" and "Delete" buttons work, add the **Game Profiles** scene to File > Build Profiles > Scene List.

## Try it

1. Open the **Game Profiles** scene and press Play.
2. Press **Add 100 points**, then **Save**. The open game profile (marked with >) shows a last saved time and playtime.
3. Press **Create game profile**, then **Open** on the new one. The points go back to 0.
4. **Open** the first one again. Its points come back.
5. **Delete** a game profile you don't need.

## The lines that matter

```csharp
GameProfile created = SaveActions.GameProfiles.CreateNew();   // named "Game Profile 3", not opened yet
GameProfile slot = SaveActions.GameProfiles.Get(slotNumber);  // by the slot number the screen shows
slot.Open();                                                  // then reload the scene
foreach (GameProfile profile in SaveActions.GameProfiles.All) // SlotNumber, DisplayName, LastSaved, Playtime
slot.Delete();
```

You never type a game profile's ID. You hold a `GameProfile` and call its methods. Slot numbers never change.

A real save slot screen usually sets the config's Startup Policy to **Manual**, so nothing opens until the player picks a slot. **AutoSetUp** opens the last game profile at startup.
