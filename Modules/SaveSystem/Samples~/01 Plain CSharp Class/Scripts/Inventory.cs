using System.Collections.Generic;
using AbstractPixel.SaveSystem;

namespace SaveSystemSamples.PlainCSharpClass
{
    // A plain C# class: no MonoBehaviour, no GameObject.
    // [Savable] picks the save file. "Game" lives in the game profile, so each game profile has its own inventory.
    [Savable(SaveCategory.Game)]
    public class Inventory : ISavable<InventoryData>
    {
        public List<string> Items { get; private set; }

        public Inventory()
        {
            // New-game values first...
            Items = new List<string> { "Wooden Sword" };
            // ...then register last, because the save system may restore the saved items inside this call.
            SaveActions.Register(this, "player-inventory");
        }

        public void Add(string item)
        {
            Items.Add(item);
        }

        public void Remove(string item)
        {
            Items.Remove(item);
        }

        // Capture: give the save system a copy, so later changes to Items don't sneak into the saved data.
        public InventoryData CaptureData()
        {
            return new InventoryData { Items = new List<string>(Items) };
        }

        // Restore: put the saved items back.
        public void RestoreData(InventoryData _loadedData)
        {
            Items = new List<string>(_loadedData.Items);
        }
    }
}
