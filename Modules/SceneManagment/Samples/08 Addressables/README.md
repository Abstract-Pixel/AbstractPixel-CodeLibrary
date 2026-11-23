# 08 Addressables
**Level 3 · full control**
You will learn how a scene group opens an Addressable main scene, and how its scene reference shows and changes where the scene loads from.

Covers: F05 Page: New scenes section (look only) · F31 Addressables loader and moves

## What you need
Unity 6.3 or newer, the scene system set up, and the Addressables package (com.unity.addressables). Without it the sample shows one line saying so and compiles. A project with no Addressables settings yet: open Window > Asset Management > Addressables > Groups and click "Create Addressables Settings" first.

## Files
| Folder | What is in it |
|---|---|
| `Scenes/` | `Sample 08 Start` and `Sample 08 Addressable Level` |
| `Scene Groups/` | `Sample 08 StartGroup` and `Sample 08 Addressable LevelGroup` (its main scene is the one you make Addressable) |
| `Prefabs/` | `Sample 08 Crate`: a box with a band |
| `Materials/` | `Sample 08 Start` (sand) and `Sample 08 Addressable Level` (blue) |
| `Scripts/` | `Sample08Buttons`: the button and the Addressables status of the scene reference (compiled only with Addressables) |
| `Tests/` | The PlayMode test: makes the scene Addressable, changes to it, removes the entry again |

A package cannot ship an Addressables entry, so the sample ships the scene not Addressable; step 2 makes it Addressable and step 6 puts it back.

## Try it
1. Open `Sample 08 Start`: in the toolbar's scene picker, type "Sample 08 Start" and click the row.
2. Select `Sample 08 Addressable LevelGroup` in `Scene Groups/`. In its Inspector, right-click the Main scene reference and pick "Move to Addressables". Its status turns blue.
3. Open Window > Asset Management > Addressables > Groups: the scene is in the default group.
4. Press Play and click "Go to the Addressable level" (top right). <!-- done-when screen:Hidden -->
5. Click "Back". Stop Play. <!-- done-when screen:Hidden -->
6. In the group's Inspector, right-click the Main scene reference again and pick "Move to Build Settings". The entry leaves Addressables; a sample scene never goes into Build Settings, so it goes back to opening from its path.
7. Look only: open Project Settings > Abstract Pixel > Scene System and read the New scenes section (where new scenes go: Build Settings or Addressables).

## You should see
<!-- timeline -->

- After step 2 the Main scene reference's status is blue: Addressable.
- After step 4 the blue level appears, the label says "Sample 08 Addressable Level is Addressable: it opens through Addressables (now open)", and the Console logs "[SceneLoader] 'Sample 08 Addressable Level' is Addressable, opening it through Addressables."
- After step 6 the status is no longer blue, the Addressables Groups window no longer lists the scene, and Build Settings is unchanged. The Console shows "Sample scenes were left out of Build Settings" (a warning that is expected here) and a line that still says "Added … to Build Settings"; that wording is wrong (a known scene system item for 5B): nothing was added.

## The lines that matter
`Scripts/Sample08Buttons.cs`: the scene is a scene reference; Addressables knows it by the reference's GUID, as the scene system's loader does.
```csharp
private static bool HasAddressablesEntry(AbstractPixel.Core.SceneReference _scene)
{
    if (_scene == null || _scene.HasGuid == false || Addressables.ResourceLocators == null)
    {
        return false;
    }
```

`Scripts/Sample08Buttons.cs`: the change itself is the usual call; the loader picks Addressables on its own.
```csharp
public Task<SceneChangeResult> Go()
{
    return SceneActions.ChangeTo(otherGroup);
}
```

## Windows this sample shows
- The scene reference in the group inspector: blue status, right-click "Move to Addressables" and back (select `Sample 08 Addressable LevelGroup`).
- Addressables Groups: Window > Asset Management > Addressables > Groups.
- New scenes section: Project Settings > Abstract Pixel > Scene System (look only).

## How it's set up
1. Select the Addressable level's scene group; its main scene field shows Addressable.
   <!-- action select "Packages/com.abstractpixel.library/Modules/SceneManagment/Samples/08 Addressables/Scene Groups/Sample 08 Addressable LevelGroup.asset" "Select the group" -->
2. Select the object with `Sample08Buttons`.
   <!-- action select-in-scene "Packages/com.abstractpixel.library/Modules/SceneManagment/Samples/08 Addressables/Scenes/Sample 08 Start.unity" "Sample 08 Buttons" "Select the buttons" -->
3. Open the New scenes section: where new scenes go.
   <!-- action open-settings "New scenes" "Open New scenes" -->

## Why it works
- Each scene picks [its loader](../../Editor/Documentation/Pages/guide/build-settings-and-addressables.md#which-loader-opens-a-scene) from the asset: Build Settings or Addressables.
- Addressables is optional ([design decision 5](../../docs/adr/0005-addressables-and-broaudio-are-optional.md)).

## If it goes wrong
- "Move to Addressables (needs the Addressables package)" → install com.unity.addressables.
- "this project has no Addressables settings yet" → Addressables Groups window > Create Addressables Settings.
- The label says "not Addressable" in Play → step 2 was skipped or undone.

## Next steps
- [06 Loading Scene](../06%20Loading%20Scene/README.md): a loading scene while it loads.
- [09 Editor Tools](../09%20Editor%20Tools/README.md): the scene picker shows the same blue status.
