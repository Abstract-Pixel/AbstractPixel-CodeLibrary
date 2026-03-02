using AbstractPixel.Core;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    public  interface ISavable<T>
    {
        public T CaptureData();
        public void RestoreData(T _loadedData); 
    }
}
