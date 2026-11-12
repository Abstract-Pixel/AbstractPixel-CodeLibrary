using System;
using System.Collections.Generic;

namespace SaveSystemSamples.PlainCSharpClass
{
    // The data that goes into the save file. Plain public fields, so JSON can store them.
    [Serializable]
    public class InventoryData
    {
        public List<string> Items = new List<string>();
    }
}
