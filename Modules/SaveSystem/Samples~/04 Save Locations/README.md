# 04 Save Locations

One value in each save location: the volume setting in **Global**, coins in **Game Profile**, and chests in **Scene**, across two scenes.

## Before you start

- The project needs a save system config. Make one with **Tools > External Project Tools > Save System > Create Config**.
- This sample ships two save file classes: `SampleSettingsSaveFile` (Global) and `SampleChestsSaveFile` (Scene). After the next compile the editor adds a row for each to the config's save file list. The coins use the built-in **GameProfile** save file.
- To make the "Go to" and "New game profile" buttons work, add the **Save Locations Forest** and **Save Locations Cave** scenes to File > Build Profiles > Scene List.

## Try it

1. Open **Save Locations Forest** and press Play.
2. Press **Volume up**, **Add 5 coins** and **Open Chest 1**, then **Save**.
3. Press **Go to Save Locations Cave**, then go back. Chest 1 is still open: each scene keeps its own chests.
4. Press **New game profile**. The volume stays, the coins go back to 0, and the chests are closed.
5. Press **Next game profile** to go back. The coins and the open chest return.
6. The **Last save file loaded** line comes from `SaveEventBus.OnLoadCompleted`. The screen subscribes after the save files loaded, and is still told, because load events are sticky. A Scene save file reports once per scene.

## The lines that matter

```csharp
public class VolumeSetting : MonoBehaviour, ISavable<int, SampleSettingsSaveFile>  // Global: shared by every game profile
public class PlayerCoins : MonoBehaviour, ISavable<int>                            // GameProfile: one per playthrough
public class Chest : MonoBehaviour, ISavable<bool, SampleChestsSaveFile>           // Scene: one file per scene, per game profile
SaveEventBus.OnLoadCompleted += ShowLoadedSaveFile;                                // sticky: told about files loaded earlier too
profile.Open(); SceneManager.LoadScene(...);                                       // switch game profiles, then reload
```

The save file list in the config decides each save file's save location; a script only names its save file class. In your game you'd name a class from `SaveFiles.cs`, such as `SaveFiles.Settings`; a sample lives in its own assembly, so it ships its own classes the way a library module does.

After opening a different game profile, reload the scene: objects registered under the previous game profile never write their values into the new one.
