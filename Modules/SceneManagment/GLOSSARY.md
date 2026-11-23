# Scene Management

The words for opening, closing and switching scenes in a game built on the Abstract Pixel Library. Use these words in code names, docs, inspector labels, tooltips and issues.

Start with the module `README.md`. Decisions and their reasons are in `docs/adr/`.

## Scenes and groups

**Scene group**:
An asset that describes one location the player can be in, such as a level, the main menu or a minigame. It names one main scene and picks a scene set. The game changes scene by going to a scene group.
_Avoid_: level, scene list, scene collection

**Main scene**:
The one scene in a scene group that makes it that location, such as "Level 7". It becomes Unity's active scene. A scene is the main scene of at most one scene group.
_Avoid_: level scene, primary scene, active scene (when meaning the asset)

**Scene set**:
An asset that holds the default manager scenes, shared scenes and Reset shared scenes choice for the scene groups that use it. Many scene groups share one scene set, and Project Settings names the default one.
_Avoid_: boot profile, boot group, boot scenes, scene preset

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
A scene shown while the old location closes and the new one opens. It is one scene, never a scene group. Project Settings lists the loading scenes and names the default; a scene group says when it shows one and which.
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

**Scene picker**:
The dropdown on the Editor's main toolbar that finds any scene or scene group by name, then opens it or selects it in the Project window.
_Avoid_: scene switcher, scene selector, level picker

**Scene in no group**:
A scene that is no scene group's main scene and is in no scene set. Played from the Editor, it opens in a temporary scene group built from the default scene set. A manager or shared scene played on its own opens the start scene group instead.
_Avoid_: random scene, loose scene, orphan scene

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
A scene change to the scene group that is already open, so the location starts again. It is never a second attempt at a scene that failed to load: a failed load is not tried again.
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

**Screen counts as visible at**:
The share of Show screen, set on a transition profile, at which the screen counts as visible enough for relocatables to be released.
_Avoid_: release gate, reveal threshold

**Screen state**:
What the scene transition executor is doing to the screen: Visible, Hiding, Hidden or Showing. Game management reacts to it; the screen effect never changes game time itself.
_Avoid_: transition in, transition out, transitioning, IsTransitioning (as a word in docs)

## Relocating objects

**Relocatable**:
An object in a manager scene or shared scene that the scene system moves to a spawn point position on every scene change, such as the player's car or a pooled enemy.
_Avoid_: placeable, relocatable boot object, boot object

**Relocation label**:
An asset that names a kind of relocatable, such as "Player" or "Enemy". Relocatables and spawn point positions match by label. An empty label field is a valid label of its own, shown as "No label", for games that relocate one kind of object. A label is known by its ID, so two copies of one label (an Addressables bundle carries its own) match.
_Avoid_: placement label, tag (Unity already has tags), spawn type, entity type, string id

**Instance**:
The number, counted from 1, that picks one relocatable among those with the same relocation label, and the one spawn point position it goes to. It is shown as "Instance 5". A relocatable with no instance set takes the next free one.
_Avoid_: slot, index, instance number, spawn number, number

**Spawn point**:
A component in a main scene that holds the positions where relocatables with its relocation label go in that scene group. Its own transform is its first position.
_Avoid_: relocator, spawn marker, placement point, spawn area

**Spawn point position**:
One position and rotation stored in a spawn point, with its instance. Positions are data on the spawn point, never GameObjects.
_Avoid_: slot, spawn slot, child point, marker

**Relocation broadcaster**:
The one thing the scene system asks to hold, relocate and release every relocatable during a scene change. The relocation sub-module provides it; a game without it changes scenes without relocation.
_Avoid_: placement step, placement hook, relocation service

**Hold**:
The state of a relocatable whose movement and physics are paused during a scene change, or because it has no spawn point position.
_Avoid_: freeze, lock, disable

**Relocate**:
The step that moves every held relocatable to its spawn point position once the new scene group is open.
_Avoid_: place, placement (as the step), teleport, spawn, move to spawn point

**Release**:
Ending the hold, once the screen counts as visible again (after the launch scene hold, while that still holds).
_Avoid_: unlock, unfreeze, enable physics

**Wait for ground below**:
A relocatable setting that keeps it held, and the screen covered, until a collider of the Ground layers is found under its spawn point position. It never moves the relocatable onto the ground.
_Avoid_: ground snap, snap to ground, drop to ground

**Ground layers**:
The physics layers that count as ground, both for Wait for ground below and for adding spawn point positions in the Editor.
_Avoid_: walkable layers, floor mask

**Steepest ground**:
The steepest slope, in degrees, that a relocation label accepts for a spawn point position. A steeper position is allowed but shown as a warning. For "No label" it is the Scene System page's "Steepest slope without a label".
_Avoid_: max slope, slope limit

## Samples

**Sample scene group**:
A scene group marked as a sample. Its scenes open in the Editor but never go into a build.
_Avoid_: demo group, test group, example scene group

**Sample launch scene**:
A launch scene that a sample scene group names, used only when Play starts from it in the Editor.
_Avoid_: test splash

## Help

**Scene documentation**:
The scene system's documentation, read in a window inside the Unity Editor. It explains each part, shows each sample's setup and links to the windows and objects it talks about.
_Avoid_: manual, wiki, readme, guide (for the whole documentation)

**Help button**:
The "?" next to a scene system window, page section, setup step or inspector. It opens the scene documentation at the part about that control.
_Avoid_: info icon, docs link, help URL
