# 05 Game Profiles

A simple save slot screen: create, open, list and delete game profiles. Each one shows its manifest's name, last saved time and playtime.

## Before you start

- The project needs a save system config named `SaveSystemConfig` in a `Resources` folder (Assets > Create > Utility > SaveSystemConfigSO). It needs these entries:
  - `Game` in `GameProfile`
- To make the "Open" and "Delete" buttons work, add the **Game Profiles** scene to File > Build Profiles > Scene List.

## Try it

1. Open the **Game Profiles** scene and press Play.
2. Press **Add 100 points**, then **Save**. The open game profile (marked with >) shows a last saved time and playtime.
3. Press **Create game profile**, then **Open** on the new one. The points go back to 0.
4. **Open** the first one again. Its points come back.
5. **Delete** a game profile you don't need.

## The lines that matter

```csharp
SaveSystem.GameProfiles.CreateNew("Game Profile 2");   // returns the new ID; doesn't open it
SaveSystem.GameProfiles.Open(id);                      // then reload the scene
SaveSystem.GameProfiles.GetAll();                      // manifests: DisplayName, LastSavedUtc, PlaytimeSeconds
SaveSystem.GameProfiles.Delete(id);
```

A real save slot screen usually sets the config's Startup Policy to **Manual**, so nothing opens until the player picks a slot. With **AutoSetUp**, the last opened game profile reopens at startup.
