using System.Collections.Generic;
using AbstractPixel.SaveSystem;

namespace SaveSystemSamples.SaveAField
{
    // A plain C# class: no MonoBehaviour, no GameObject. [Save] fields plus one Register(this) line.
    // With no ISaveIn<TFile>, the data goes in the built-in GameProfile save file, so each game profile has its own backpack.
    public class Backpack
    {
        // New-game value: what a brand new game profile starts with.
        [Save] List<string> items = new List<string> { "Wooden Sword" };

        public IReadOnlyList<string> Items => items;

        public Backpack()
        {
            // Register on the last line: the saved items may be restored inside this call.
            // No save ID needed; the class name is used. Renaming the class later? Add [SaveId("Backpack")] first.
            SaveActions.Register(this);
        }

        public void Add(string item)
        {
            items.Add(item);
        }

        public void RemoveLast()
        {
            if (items.Count > 0)
            {
                items.RemoveAt(items.Count - 1);
            }
        }
    }
}
