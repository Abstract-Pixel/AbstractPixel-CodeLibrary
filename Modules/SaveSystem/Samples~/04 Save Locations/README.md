# 04 Save Locations

One value in each save location: the volume setting in **Global**, coins in **Game Profile**, and chests in **Scene**, across two scenes.

## Before you start

- The project needs a save system config named `SaveSystemConfig` in a `Resources` folder (Assets > Create > Utility > SaveSystemConfigSO). It needs these entries:
  - `Settings` in `Global`
  - `Game` in `GameProfile`
  - `Position` in `Scene`
- To make the "Go to" and "New game profile" buttons work, add the **Save Locations Forest** and **Save Locations Cave** scenes to File > Build Profiles > Scene List.

## Try it

1. Open **Save Locations Forest** and press Play.
2. Press **Volume up**, **Add 5 coins** and **Open Chest 1**, then **Save**.
3. Press **Go to Save Locations Cave**, then go back. Chest 1 is still open: each scene keeps its own chests.
4. Press **New game profile**. The volume stays, the coins go back to 0, and the chests are closed.
5. Press **Next game profile** to go back. The coins and the open chest return.

## The lines that matter

```csharp
[Savable(SaveCategory.Settings)]  public class VolumeSetting   // Global: shared by every game profile
[Savable(SaveCategory.Game)]      public class PlayerCoins     // Game Profile: one per playthrough
[Savable(SaveCategory.Position)]  public class Chest           // Scene: one file per scene, per game profile
SaveSystem.GameProfiles.Open(id); SceneManager.LoadScene(...);  // switch game profiles, then reload
```

The config decides each save file's save location; the scripts only name the save file. After opening a different game profile, reload the scene: objects registered under the previous game profile never write their values into the new one.
