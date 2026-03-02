using System.Xml;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    public interface  ISavableBridge
    {
        public string UniqueId{  get; }

        public object CaptureState(SaveCategory caregoryFilter);

        public void RestoreState(object data, SaveCategory caregoryFilter);
      
    }
}
