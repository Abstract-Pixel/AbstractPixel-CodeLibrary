using System;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    [Serializable]
    public class SaveableIdentification
    {
        public string ClassName;
        public string GUID;

        public SaveableIdentification(string _className, string _guid)
        {
            ClassName = _className;
            GUID = _guid;
        }
    
    }
}
