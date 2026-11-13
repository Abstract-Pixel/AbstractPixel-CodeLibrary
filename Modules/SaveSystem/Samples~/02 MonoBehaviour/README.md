# 02 MonoBehaviour

A `Health` script on a GameObject with the Savable Bridge. New-game values go in `Awake`, the health bar reads the value in `Start`, and after a reload the bar shows the saved value.

## Before you start

- The project needs a save system config. Make one with **Tools > External Project Tools > Save System > Create Config**.
- This sample saves into the built-in **GameProfile** save file, so it needs no row of its own in the save file list.
- To make the "Reload scene" button work, add the **MonoBehaviour** scene to File > Build Profiles > Scene List.

## Try it

1. Open the **MonoBehaviour** scene and press Play.
2. Press **Take 10 damage** a few times, then **Save**.
3. Press **Reload scene**, or stop and press Play again.

You should see "Health read in Start" show the saved health, not 100.

## The lines that matter

```csharp
public class Health : MonoBehaviour, ISavable<int>  // no save file class: the built-in GameProfile file
void Awake() { Current = Max; }                     // Health: the new-game value goes in Awake, never Start
void Start() { ValueReadInStart = health.Current; } // HealthBar: already sees the saved value
```

The **Savable Bridge** component on the Player connects `Health` to the save system. The save system restores after every `Awake` and before any `Start`. A saved field assigned in `Start` would overwrite the restored value, so keep new-game values in `Awake`.
