# 02 Buttons With No Code

A level saved with no code at all: three ready-made saver components and buttons wired in the inspector.

| Object | Component | What it saves |
|---|---|---|
| Crate | **Save Transform** | position, rotation and scale |
| Lamp | **Save Active State** | whether it's on or off |
| Coin | **Save Destroyed** | that it was picked up, so it stays gone |
| Save Actions | **Mono Save Actions** | nothing; its methods and events are what the buttons use |

## Before you start

- Make a save system config with **Tools > External Project Tools > Save System > Create Config**.
- This sample uses the built-in **GameProfile** save file. You don't need to add anything to the config.
- The buttons use uGUI and the Input System's UI input module.

## Try it

1. Open the **Buttons With No Code** scene and press Play.
2. Drag the **Crate** somewhere else in the Scene view. Press **Turn the lamp off** and **Pick up the coin**.
3. Press **Save**. "Saved!" appears; it comes from the **On Saved** event.
4. Stop, then press Play again.

The crate is where you left it, the lamp is off and the coin is gone. **Turn the lamp on** and **Save** to put the lamp back. A picked-up coin stays gone in this game profile for good.

## How it's wired

Select each button and look at its **On Click** list:

- **Save**: `MonoSaveActions.SaveAll`.
- **Turn the lamp off / on**: `GameObject.SetActive` on the Lamp. Save Active State notices by itself.
- **Pick up the coin**: `SaveDestroyed.DestroyAndForget` on the Coin. Don't use a plain Destroy: the coin would come back.

On the **Save Actions** object, **On Saved** turns the "Saved!" label on. **On Save Failed** passes the reason, so it can go straight into a Text. Its other buttons are **Save Chosen File**, **Reload Chosen File**, **Open Slot** and **New Game**.

Each saved object has a **Savable Bridge**. The editor adds one for you when you add a saver component.

## Next steps

Autosave needs no code either: tick **Save On Scene Unload** or set **Autosave Interval Minutes** in the save system config. When a component isn't enough, write `[Save]` on a field (sample 01).
