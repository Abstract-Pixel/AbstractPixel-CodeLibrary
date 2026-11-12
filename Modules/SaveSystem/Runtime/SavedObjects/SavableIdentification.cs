using System;

namespace AbstractPixel.SaveSystem
{
    [Serializable]
    public class SavableIdentification
    {
        public string ClassName;
        public string GUID;

        public SavableIdentification(string _className, string _guid)
        {
            ClassName = _className;
            GUID = _guid;
        }

    }
}
