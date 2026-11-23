# 01 Button Changes Level
**Level 0 · no code**
You will learn how a UI button changes the scene group with a fade, set up entirely in the Inspector.

Covers: F21 MonoSceneActions on a UnityEvent · F22 Basic fade, TransitionProfile, Screen counts as visible at

## What you need
Unity 6.3 or newer and the scene system set up (Project Settings > Abstract Pixel > Scene System). No extra packages: the button uses Unity UI and the Input System, which the library already needs.

## Files
| Folder | What is in it |
|---|---|
| `Scenes/` | `Sample 01 Room A` and `Sample 01 Room B`: a room each, with a button at the bottom of the screen |
| `Scene Groups/` | `Sample 01 Room AGroup` and `Sample 01 Room BGroup`: sample scene groups with `Samples Managers` (the basic fade) as their manager scene |
| `Prefabs/` | `Sample 01 Doorway`: a doorway made of three cubes |
| `Materials/` | `Sample 01 Room A` (orange) and `Sample 01 Room B` (teal), made from the library's sample shader |
| `Tests/` | The PlayMode test: the Room A button opens Room B, and the screen goes Hiding, Hidden, Showing, Visible |

## Try it
1. Open `Sample 01 Room A`: in the toolbar's scene picker, type "Sample 01 Room A" and click the row (or double-click it in the Project window under `Packages/Abstract Pixel Library/Modules/SceneManagment/Samples/01 Button Changes Level/Scenes/`).
2. Select `Change Room Button` under `Canvas` in the Hierarchy and look at the Button's On Click list: one entry, `Scene Actions` > `MonoSceneActions.ChangeTo`, with `Sample 01 Room BGroup` as its argument.
3. Press Play.
4. Click "Go to Room B" at the bottom of the Game view.
5. Click "Go to Room A" to go back.
6. Stop Play. In the Project window, select `Packages/Abstract Pixel Library/Modules/SceneTransitionSystem/Runtime/Transitions/BasicFade/ImageFadeProfile` and look at its Inspector: the fade's colour and times, and "Screen counts as visible at".

## You should see
- After step 3 the Game view shows an orange doorway and the label "Sample 01 · Button Changes Level · Room A".
- After step 4 the screen fades to black, then Room B appears with a teal doorway.
- The label now reads "Sample 01 · Button Changes Level · Room B" and "You are in Room B.".
- After step 5 the fade runs again and Room A is back.

## The lines that matter
This sample has no code. The Inspector settings that do the work:
- `Change Room Button` > Button > On Click: `Scene Actions` (a `MonoSceneActions` component) > `ChangeTo`, argument: the other room's scene group.
- Each scene group: main scene the room, manager scene `Samples Managers` (it holds the `BasicFadeController` prefab), "Sample scene group" ticked, no scene set.
- `ImageFadeProfile` (a `TransitionProfile`): the fade's look and times; "Screen counts as visible at" says how far into Show screen the screen counts as visible.

## Windows this sample shows
- The Button's On Click list: select `Change Room Button` in the Hierarchy, Inspector.
- The TransitionProfile and ImageFadeProfile Inspector: select `ImageFadeProfile` in the Project window (`SceneTransitionSystem/Runtime/Transitions/BasicFade/`).
- The scene group inspector: select a group in `Scene Groups/`.

## If it goes wrong
- The button does nothing → the scene has no EventSystem, or the On Click entry lost its target: pick `Scene Actions` and `MonoSceneActions.ChangeTo` again.
- The room changes with no fade → `Samples Managers` is missing from the group's manager scenes.
- The Console says the change was refused (Busy) → a change was still running: wait for the fade to end and click again.

## Next steps
- [02 Change Level From Code](../02%20Change%20Level%20From%20Code/README.md): the same change from a script, with its result.
- [06 Loading Scene](../06%20Loading%20Scene/README.md): show a loading scene between the two.
