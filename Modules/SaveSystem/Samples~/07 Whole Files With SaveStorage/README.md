# 07 Whole Files With SaveStorage

A debug screen that reads, copies, pastes and deletes the open game profile's GameProfile save file as JSON text.

`SaveStorage` is for tools and debug buttons: look inside a save, copy a player's save to reproduce a bug, reset one file. Game code saves through `[Save]` fields and `SaveActions` instead.

## Before you start

- Make a save system config with **Tools > External Project Tools > Save System > Create Config**.
- This sample uses the built-in **GameProfile** save file. You don't need to add anything to the config.

## Try it

1. Open the **Whole Files With SaveStorage** scene and press Play.
2. Press **Count a visit**, then **Save**. The file's text shows the visit count under `visits`.
3. Press **Copy the file's text**. Count two more visits.
4. Press **Paste text into the file**. The visits go back to what the copied text says, in the running game.
5. Press **Delete the file** to start this game profile's file over.

Pasting text that isn't a whole save file logs an error and changes nothing.

## The lines that matter

```csharp
SaveStorage.ReadText<SaveFiles.GameProfile>(out string json);     // the open game profile's file
SaveStorage.WriteText<SaveFiles.GameProfile>(json);               // in play mode, restores the objects in it too
SaveStorage.Delete<SaveFiles.GameProfile>();                      // the file and its backup
SaveStorage.GetFilePath<SaveFiles.GameProfile>();                 // where it is on disk
SaveStorage.ReadText<SaveFiles.GameProfile>(otherProfile, out json); // another game profile's file
SaveStorage.ReadText<SaveFiles.Scene>(forestScene, out json);     // a Scene file always needs its scene
```

`SaveStorage` also works in edit mode, so an editor window can read and fix save files without pressing Play.
