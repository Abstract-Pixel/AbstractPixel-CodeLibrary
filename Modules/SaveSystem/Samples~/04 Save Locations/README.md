# 04 Save Locations

One value in each save location: the volume setting in **Global**, coins in **Game Profile**, and chests in **Scene**, across two scenes.

## Before you start

- Make a save system config with **Tools > External Project Tools > Save System > Create Config**.
- This sample brings two save files, `SampleSettingsSaveFile` in Global and `SampleChestsSaveFile` in Scene. The editor adds them to the config after the next compile. The coins use the built-in **GameProfile** file.
- To make the "Go to" and "New game profile" buttons work, add the **Save Locations Forest** and **Save Locations Cave** scenes to File > Build Profiles > Scene List.

## Try it

1. Open **Save Locations Forest** and press Play.
2. Press **Volume up**, **Add 5 coins** and **Open Chest 1**, then **Save**.
3. Press **Go to Save Locations Cave**, then go back. Chest 1 is still open: each scene keeps its own chests.
4. Press **New game profile**. The volume stays, the coins go back to 0, and the chests are closed.
5. Press **Next game profile** to go back. The coins and the open chest return.
6. The **Last save file loaded** line comes from `SaveEventBus.OnLoadCompleted`. The screen subscribes after the files load and still hears about them, because load events are sticky.

## The lines that matter

```csharp
public class VolumeSetting : MonoBehaviour, ISavable<int, SampleSettingsSaveFile>  // Global: shared by every game profile
public class PlayerCoins : MonoBehaviour, ISavable<int>                            // GameProfile: one per playthrough
public class Chest : MonoBehaviour, ISavable<bool, SampleChestsSaveFile>           // Scene: one file per scene, per game profile
SaveEventBus.OnLoadCompleted += ShowLoadedSaveFile;                                // sticky: told about files loaded earlier too
profile.Open(); SceneManager.LoadScene(...);                                       // switch game profiles, then reload
```

The config decides where each save file lives. A script only names the file's class. In your game you'd use a class from `SaveFiles.cs`, such as `SaveFiles.Settings`. A sample can't see that file, so it brings its own classes.

Reload the scene after opening another game profile. Objects from the old profile never write into the new one.
