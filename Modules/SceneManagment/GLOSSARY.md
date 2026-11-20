# Scene Management

The words for opening, closing and switching scenes in a game built on the Abstract Pixel Library. Use these words in code names, docs, inspector labels, tooltips and issues.

Start with the module `README.md`. Decisions and their reasons are in `docs/adr/`.

## Scenes and groups

**Scene group**:
An asset that describes one place the player can be, such as a level, the main menu or a minigame. It names one main scene and picks a scene set. The game changes scene by going to a scene group.
_Avoid_: level, scene list, scene collection

**Main scene**:
The one scene in a scene group that makes it that place, such as "Level 7". It becomes Unity's active scene. A scene is the main scene of at most one scene group.
_Avoid_: level scene, primary scene, active scene (when meaning the asset)

**Scene set**:
An asset that lists the manager scenes and shared scenes that come with a scene group. Many scene groups share one scene set, and Project Settings names the default one.
_Avoid_: boot profile, boot group, boot scenes

**Manager scene**:
A scene that opens once and stays open across scene changes, such as the scene holding audio and save managers.
_Avoid_: managerial scene, boot scene, persistent scene

**Shared scene**:
A scene that several scene groups use next to their main scene, such as the scene holding the player's car. It stays open between two groups that both use it, unless the group resets shared scenes.
_Avoid_: contextual scene, boot scene, additive scene

**Reset shared scenes**:
A scene group option that closes and reopens every shared scene on each scene change, so state kept in them starts fresh.
_Avoid_: force reload, ForceReloadContextualScenes (as a word in docs)

**Close previous manager scenes**:
A scene group option that closes the manager scenes that were open before, when they do not belong to this group's scene set. Used when game modes need different managers.
_Avoid_: discard managers, reset managers

**Loading scene**:
A scene shown while the old place closes and the new one opens. Project Settings lists the loading scenes and names the default; a scene group says whether it uses one and which.
_Avoid_: loading screen (when meaning the scene asset), interstitial

**Scene status**:
Where a scene can be loaded from: In build, Addressable, Not in build, or Missing. A scene is In build or Addressable, never both.
_Avoid_: build state, scene state

## Changing scene

**Scene change**:
Leaving the current scene group and arriving in another one, or the same one again. Only one scene change runs at a time.
_Avoid_: transition (that word belongs to the screen effect), scene load, level load

**Retry**:
A scene change to the scene group that is already open, so the place starts again.
_Avoid_: reload, restart (in code names)

**Preload**:
Opening a scene group's scenes in the background before the scene change to it, keeping its main scene waiting until the change. At most one scene group is preloaded.
_Avoid_: prefetch, background load

**Scene change result**:
What a scene change request tells its caller when it ends: Done, Refused (with the reason), or Failed (with the scene that failed).
_Avoid_: status code, success flag

**Loading progress**:
How far a scene change has got, from 0 to 1, for a loading scene to show.
_Avoid_: percent, load amount

## The screen during a scene change

**Scene transition executor**:
The one thing the scene system asks to hide and show the screen around a scene change. The transitions module provides it; a game without transitions has none, and scenes change without an effect.
_Avoid_: change cover, fade hook, transition callback

**Hide screen**:
The step that covers the screen before scenes close, such as a fade to black.
_Avoid_: transition in, fade in, cover

**Show screen**:
The step that uncovers the screen after the new scenes are ready.
_Avoid_: transition out, fade out, reveal

## Placing objects

**Placeable**:
An object that lives in a manager scene or shared scene and is moved to a spawn point on every scene change, such as the player or the car.
_Avoid_: relocatable, boot object

**Spawn point**:
A marker in a main scene that says where a placeable with the same placement label goes in that place.
_Avoid_: relocator, spawn marker, placement point

**Placement label**:
An asset that names a kind of placeable, such as "Player" or "Enemy". Placeables and spawn points match by label, and by number when many share one label.
_Avoid_: tag (Unity already has tags), spawn type, entity type, string id

**Hold**:
The state of a placeable whose movement and physics are paused during a scene change, or because it has no spawn point.
_Avoid_: freeze, lock, disable

**Place**:
Moving every held placeable to its spawn point once the new scene group is open.
_Avoid_: relocate, teleport, spawn

**Release**:
Ending the hold, once enough of the screen is visible again.
_Avoid_: unlock, unfreeze, enable physics
