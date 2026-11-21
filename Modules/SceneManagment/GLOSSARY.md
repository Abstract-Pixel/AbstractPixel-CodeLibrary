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
An asset that holds the default manager scenes, shared scenes and Reset shared scenes choice for the scene groups that use it. Many scene groups share one scene set, and Project Settings names the default one.
_Avoid_: boot profile, boot group, boot scenes

**Override**:
A part of a scene group (its manager scenes, its shared scenes, or its Reset shared scenes choice) that keeps the group's own value instead of following its scene set. An override stays until the designer reverts it.
_Avoid_: custom value, local change, exception

**Scene system settings**:
The one asset per project that holds the scene sets, the default scene set, where new scenes go, whether builds check scene groups, and the list of scene groups. It can live in any folder.
_Avoid_: boot profile, scene config, settings file

**Scene System page**:
The page in Project Settings where a project sets up its scene system and looks after it: the scene sets and the default one, where new scenes go, the checks before a build, and every scene group with its problems and their fixes. In the Editor it is titled "Scene System".
_Avoid_: Scenes page, scene settings window, Check All Scene Groups

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
A scene shown while the old place closes and the new one opens. It is one scene, never a scene group. Project Settings lists the loading scenes and names the default; a scene group says when it shows one and which.
_Avoid_: loading screen (when meaning the scene asset), loading scene group, interstitial

**Show the loading scene**:
The scene group setting that says when its loading scene appears: **Never**, **When arriving from another scene group** (a Retry goes straight back in), or **Every time, Retry included**.
_Avoid_: Retry option, Off / When changing level / Always, use a loading scene

**Loading progress view**:
The part of a loading scene that shows loading progress to the player: it smooths the value and plays its own animations when it appears, when it is full and when it leaves.
_Avoid_: loading bar (a bar is only one look), progress widget

**Scene status**:
Where a scene can be loaded from: In build, Addressable, Not in build, or Missing. A scene is In build or Addressable, never both.
_Avoid_: build state, scene state

## Starting the game

**Start-up**:
The first scene change of a session, made by the scene system itself when the game launches or Play is pressed. It ends with the game started.
_Avoid_: boot, bootstrap, initialization

**Start scene group**:
The scene group the game opens at launch, named in the scene system settings.
_Avoid_: first scene group, boot group, entry group

**Launch scene**:
An optional scene, such as a splash screen, that Unity opens before the start scene group. Start-up opens the start scene group behind it, and the launch scene closes itself.
_Avoid_: splash scene (as the general word), boot scene, intro scene

**Launch scene hold**:
The wait that keeps the start scene group from showing itself while the launch scene plays. Start-up holds it when Unity opens the launch scene; the launch scene releases it at its hand-off, and scenes waiting for it then start.
_Avoid_: entrance gate, splash gate, menu gate

## Changing scene

**Scene change**:
Leaving the current scene group and arriving in another one, or the same one again. Only one scene change runs at a time.
_Avoid_: transition (that word belongs to the screen effect), scene load, level load

**Retry**:
A scene change to the scene group that is already open, so the place starts again. It is never a second attempt at a scene that failed to load: a failed load is not tried again.
_Avoid_: reload, restart (in code names), retry (for trying a failed load again)

**Preload**:
Opening a scene group's scenes in the background before the scene change to it, keeping its main scene waiting until the change. The game asks for it per scene change; the scene system never preloads by itself. At most one scene group is preloaded.
_Avoid_: prefetch, background load

**Scene change result**:
What a scene change request tells its caller when it ends: Done, Refused (with the reason), or Failed (with the scene that failed).
_Avoid_: status code, success flag

**Loading progress**:
How far a scene change has got, from 0 to 1, for a loading scene to show.
_Avoid_: percent, load amount

**Fallback scene group**:
The scene group the scene system goes to when a scene change fails, named in the scene system settings. By default it is the start scene group.
_Avoid_: error scene, safe scene, recovery group

## The screen during a scene change

**Scene transition executor**:
The one thing the scene system asks to hide and show the screen around a scene change. The transitions module provides it; a game without transitions has none, and scenes change without an effect.
_Avoid_: change cover, fade hook, transition callback

**Hide screen**:
The step that covers the screen before scenes close, such as a fade to black. Its messages are "start hide screen transition" and "end hide screen transition" (the screen counts as covered).
_Avoid_: transition in, fade in, cover

**Show screen**:
The step that uncovers the screen after the new scenes are ready. Its messages are "start show screen transition" and "end show screen transition".
_Avoid_: transition out, fade out, reveal

**Screen state**:
What the scene transition executor is doing to the screen: Visible, Hiding, Hidden or Showing. Game management reacts to it; the screen effect never changes game time itself.
_Avoid_: transition in, transition out, transitioning, IsTransitioning (as a word in docs)

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
