# 03 Save Slots And Continue

A simple save slot screen with a Continue button. Create, open, list and delete game profiles. Each one shows its name, last saved time and playtime.

## Before you start

- Make a save system config with **Tools > External Project Tools > Save System > Create Config**.
- This sample uses the built-in **GameProfile** save file. You don't need to add anything to the config.
- To make the "Open", "Continue" and "Delete" buttons work, add the **Save Slots And Continue** scene to File > Build Profiles > Scene List.

## Try it

1. Open the **Save Slots And Continue** scene and press Play.
2. Press **Add 100 points**, then **Save**. The open game profile (marked with >) shows a last saved time and playtime.
3. Press **Create game profile**, then **Open** on the new one. The points go back to 0.
4. **Open** the first one again. Its points come back.
5. Press **Back to the main menu**. Nothing is open now. **Continue** opens the game profile you just left.
6. **Delete** a game profile you don't need.

## The lines that matter

```csharp
[Save] int points;                                            // Score: each game profile has its own
GameProfile created = SaveActions.GameProfiles.CreateNew();   // named "Game Profile 3", not opened yet
GameProfile slot = SaveActions.GameProfiles.Get(slotNumber);  // by the slot number the screen shows
slot.Open();                                                  // then reload the scene
SaveActions.GameProfiles.LastOpened                           // Continue; null on a first launch
SaveActions.GameProfiles.Count                                // grey out Load when it's 0
SaveActions.GameProfiles.CloseCurrent();                      // back to the main menu
foreach (GameProfile profile in SaveActions.GameProfiles.All) // SlotNumber, DisplayName, LastSaved, Playtime
slot.Delete();
```

You never type a game profile's ID. You hold a `GameProfile` and call its methods. Slot numbers never change.

A real main menu usually sets the config's Startup Policy to **Manual**, so nothing opens until the player picks Continue, Load or New Game. **AutoSetUp** opens the last game profile at startup.

Designers can wire the same buttons with no code: **Open Slot**, **New Game** and the **On Game Profile Opened** event on the **Mono Save Actions** component (sample 02).
