# 04 Pick A Save File

Each saved script picks its save file with `ISaveIn<TFile>`. The volume goes in a Global file every game profile shares, the coins in the GameProfile file, and the chests in the Scene file, across two scenes.

## Before you start

- Make a save system config with **Tools > External Project Tools > Save System > Create Config**.
- The coins use the built-in **GameProfile** file and the chests the built-in **Scene** file. This sample also brings one file of its own, `SampleSettingsSaveFile` in Global, the way a library module does. The editor adds its row to the config after the next compile.
- To make the "Go to" and "New game profile" buttons work, add the **Pick A Save File Forest** and **Pick A Save File Cave** scenes to File > Build Profiles > Scene List.

## Try it

1. Open **Pick A Save File Forest** and press Play.
2. Press **Volume up**, **Add 5 coins** and **Open Chest 1**, then **Save everything**.
3. Press **Go to Pick A Save File Cave**, then go back. Chest 1 is still open: each scene keeps its own chests.
4. Press **Add 5 coins** and **Volume up**, then **Save only the settings**, then **Undo unsaved coins**. The coins go back; the volume stays.
5. Press **New game profile**. The volume stays, the coins go back to 0, and the chests are closed.
6. Press **Next game profile** to go back. The coins and the open chest return.

## The lines that matter

```csharp
public class Chest : MonoBehaviour, ISaveIn<SaveFiles.Scene>                 // one file per scene, per game profile
public class PlayerCoins : MonoBehaviour                                     // no ISaveIn: the GameProfile file
public class VolumeSetting : MonoBehaviour, ISaveIn<SampleSettingsSaveFile>  // Global: shared by every game profile
SaveActions.Save<SampleSettingsSaveFile>();                                  // write one file
SaveActions.Reload<SaveFiles.GameProfile>();                                 // throw away unsaved changes to one file
SaveEventBus.AddLoadListener<SaveFiles.Scene>(CountSceneFileLoad);           // sticky: hears files that loaded earlier
```

The config decides where each save file lives. A script only names the file's class. In your game, use a built-in class (`SaveFiles.GameProfile`, `Scene`, `Settings`, `Input`, `Achievements`) or one of your own rows from `GameSaveFiles`.

Reload the scene after opening another game profile. Objects from the old profile never write into the new one.
