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
One playthrough of the game, such as "Game Profile 1", and the save location for data about that playthrough, such as the player's health and inventory. Game Profile is the default save location.
_Avoid_: game, profile, save slot, slot

**Scene**:
The save location for data about one scene within one game profile, such as which chests in the Forest scene are open. It always sits inside a game profile.
_Avoid_: scene-specific, level data

**Save file**:
A named file of saved data that lives in exactly one save location. Every location has a built-in save file with the same name as the location, and designers can add more.
_Avoid_: category, save category

**Game profile manifest**:
The summary of one game profile that a save slot screen shows, such as its name, last saved time, playtime and last scene.
_Avoid_: slot details, profile metadata

**Save system metadata**:
Facts about the save system as a whole, kept in Global, such as which game profile was opened most recently so the game can reopen it at startup.
_Avoid_: last profile, system metadata

## What gets saved

**Saved object**:
Anything whose data the save system captures and restores: a MonoBehaviour, a plain C# class or a ScriptableObject.
_Avoid_: savable, participant, save target

**Save ID**:
The name that identifies one saved object's data inside a save file. No two saved objects in the same save file share one.
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
Putting saved data back into a saved object.
_Avoid_: deserialize, load into, apply
