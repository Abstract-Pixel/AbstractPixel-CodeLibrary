# Save System

The words the team uses when talking about saving and loading player data. Use these words in code names, docs, tooltips and issues.

## Where data lives

**Save location**:
One of exactly three places saved data can live: Global, Game Profile or Scene. There are never more than three, and developers can't add new ones.
_Avoid_: scope, directory scope, save path

**Global**:
The save location for data that belongs to the player's machine rather than to any playthrough, such as settings and input bindings. Every game profile shares it.
_Avoid_: system data, shared data

**Game profile**:
One playthrough of the game, such as "Game Profile 1", and the save location for data about that playthrough, such as the player's health and inventory. Game Profile is the default save location. In code, a game profile is a `GameProfile` you hold and call `Open()` or `Delete()` on; you never type its ID.
_Avoid_: game, profile, save slot

**Slot number**:
The number a game profile shows on the save slot screen: 1, 2, 3. It never changes. Deleting slot 2 leaves slot 3 as slot 3, and the next new game profile takes the lowest free number.
_Avoid_: index, profile index

**Default game profile name**:
The name the config gives every new game profile, followed by its slot number, such as "Game Profile 2". A custom name is optional.
_Avoid_: profile name template

**Scene**:
The save location for data about one scene within one game profile, such as which chests in the Forest scene are open. It always sits inside a game profile.
_Avoid_: scene-specific, level data

**Save file**:
A named file of saved data that lives in exactly one save location. Every location has a built-in save file with the same name as the location, and designers can add more.
_Avoid_: category, save category

**Save file list**:
The list of save files in the save system config. Each row has a name, a save location and a Backup checkbox. The editor writes `SaveFiles.cs` from it.
_Avoid_: category list, category definitions

**Save file class**:
The class that stands for one save file in code, such as `SaveFiles.Ghosts`. You write it wherever you mean that file, so a typo doesn't compile. Game files come from `SaveFiles.cs`; a library module like Settings ships its own, such as `SettingsSaveFile`.
_Avoid_: save file name, category enum, file key

**Backup**:
The previous good copy of a save file, kept in the `Backups` folder. Only save files with Backup ticked have one, and each has exactly one. If a save file is corrupt, its backup loads instead.
_Avoid_: bak file, fallback file

**Game profile manifest**:
The summary of one game profile that a save slot screen shows, such as its name, slot number, last saved time, playtime and last scene.
_Avoid_: slot details, profile metadata

**Save system metadata**:
Facts about the save system as a whole, kept in Global, such as which game profile was opened most recently so the game can reopen it at startup.
_Avoid_: last profile, system metadata

**Save system config**:
The one settings asset for the save system, at `Assets/Resources/SaveSystemConfig.asset`. It is the source of truth for folders, file type, the save file list and game profile names. The menu item Create Config makes it.
_Avoid_: save settings, save manager config

## What gets saved

**Saved object**:
Anything whose data the save system captures and restores: a MonoBehaviour, a plain C# class or a ScriptableObject.
_Avoid_: savable, participant, save target

**Saved field**:
A field marked `[Save]`. The save system captures and restores it with no other code.
_Avoid_: persistent field, serialized field

**Save ID**:
The name that identifies one saved object's data inside a save file. No two saved objects in the same save file share one. A class with only one instance uses its class name; a scene object uses its Savable Bridge's ID; a spawned copy gets one from `SetSaveId`.
_Avoid_: unique ID, GUID, composite key

**Spawned copy**:
A saved object created from a prefab while the game runs. It needs its own save ID because every copy of a prefab starts with the same one.
_Avoid_: runtime instance, clone

**Savable Bridge**:
The script on a GameObject that connects the saved scripts on that GameObject to the save system. The editor adds it.
_Avoid_: save bridge, save component

## What happens

**Capture**:
Reading the current data out of a saved object so it can be written to a save file.
_Avoid_: serialize, collect

**Restore**:
Putting saved data back into a saved object. It happens as soon as both the object and its data exist, whichever comes second: normally right after every Awake and before any Start.
_Avoid_: deserialize, load into, apply

**Reload**:
Reading one save file from disk again and restoring every saved object in it, throwing away changes that weren't saved.
_Avoid_: load, refresh

**Forget**:
Removing one saved object's data from its save file on purpose, such as a coin that was picked up.
_Avoid_: delete entry, clear data

**Load event**:
An event the save system raises when saved data is ready: one save file restored, or everything restored. Load events are sticky: a script that subscribes after the event already happened is called right away. A Scene save file raises its load event once per scene, one frame after that scene's scripts start.
_Avoid_: callback, data ready signal

## Code names

| Word | Class |
|---|---|
| Saving, reloading, forgetting, game profiles | `SaveActions` |
| Load and save events | `SaveEventBus` |
| Reading or writing a save file directly, in play mode or editor tools | `SaveStorage` |
| Game profile | `GameProfile`, listed by `SaveActions.GameProfiles` |
| Save file class | `ISaveFile` |
| Save system config | `SaveSystemConfigSO` |
