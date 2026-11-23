# Get started <!-- level 0 -->

Set up the scene system once, then change scene with one call or one button. This page takes you from an empty project to your first scene change.

<!-- your-project get-started -->

## Run the setup <!-- level 0 -->

Run setup once per project. It makes the [scene system settings](term:scene-system-settings), a default [scene set](term:scene-set) and, if you want them, a [launch scene](term:launch-scene) and a [loading scene](term:loading-scene).

**Try it in the sample**

<!-- action open-settings "Setup" "Open the Setup section" -->

**Or do it yourself**

1. Open Tools > External Project Tools > Scene System > Set Up Scene System.
2. Answer the seven steps: Scene System, Settings Folder, New Scenes, Launch Scene, Loading Scene, Scene Sets, Opening Scenes.
3. On Ready, press Set up.

> [!TIP]
> The All set page, then the [Scene System page](term:scene-system-page) with the settings asset in its Setup section.

![The setup window at its Scene Sets step](../../Images/scene-system-setup.png)
<!-- marks 1@0.5,0.27 "The step and its question"; 2@0.15,0.36 "Terminology: the new words"; 3@0.5,0.5 "Your answer"; 4@0.5,0.94 "Step 6 of 7"; 5@0.86,0.965 "Next" -->

<details><summary>If it goes wrong</summary>

- The setup window never opened → the project already has a settings asset; run Set Up Scene System from the menu to see the Opening Scenes step.
- "Builds can read these settings" is not green → open the Setup section and let it put the asset back in Preloaded Assets.

</details>

## Make your first scene group <!-- level 0 -->

Right-click a scene in the Project window and pick Create > Scene Group. The new [scene group](term:scene-group) sits next to the scene, uses the default scene set and has the scene as its [main scene](term:main-scene).

**Try it in the sample**

<!-- action open-sample 01 "Open sample 01" -->
<!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Samples/01 Button Changes Level/Scene Groups/Sample 01 Room AGroup.asset" "Select Room A's group" -->

**Or do it yourself**

1. Right-click your level scene, Create > Scene Group.
2. Look at the group in the Inspector: main scene, scene set, Show the loading scene.

> [!TIP]
> A group asset named after the scene, with no problems listed in its Inspector.

## Change scene <!-- level 0 -->

Change scene from a button with `MonoSceneActions.ChangeTo`, or from code with `SceneActions.ChangeTo`. [Change scene](change-scene.md) has both.

## Pick the start scene group <!-- level 0 -->

The game opens the [start scene group](term:start-scene-group) at launch. Pick it in the Start-up section; no start-up object is needed in any scene.

<!-- action open-settings "Start-up" "Open Start-up" -->

<details><summary>Why it works</summary>

Start-up runs from `SceneSystemStartUp` before the first scene loads, reads the settings asset from Preloaded Assets and opens the group. See [Start-up](start-up.md) and [design decision 9](../../../../docs/adr/0009-start-up-opens-the-start-scene-group-behind-a-launch-scene.md).

</details>
