using System.Collections.Generic;
using AbstractPixel.SaveSystem;

namespace AbstractPixel.SaveSystem.Samples.CustomData
{
    // ISavable<TData> is for when [Save] fields aren't enough: the saved shape differs from the live fields, the data
    // needs a copy or a conversion on the way in and out, or the fields hold things JSON can't store.
    // Here CaptureData hands over a copy, so later changes to Items never sneak into data already captured.
    // With no save file class it saves into the GameProfile file. To pick another: ISavable<InventoryData, SaveFiles.Scene>.
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
