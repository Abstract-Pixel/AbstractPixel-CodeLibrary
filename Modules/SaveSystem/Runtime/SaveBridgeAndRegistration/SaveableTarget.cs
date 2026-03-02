using System.Reflection;
using System;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    [Serializable]
    public class SaveableTarget
    {
        [HideInInspector][SerializeField]public string InpsectorName;
        public MonoBehaviour Script;
        public SaveableIdentification Identification;

        // Runtime Only
        public MethodInfo CaptureDataMethod;
        public MethodInfo RestoreDataMethod;
        public Type DataToSaveType;

        public SaveableTarget(MonoBehaviour _script, SaveableIdentification _identification)
        {
            Script = _script;
            Identification = _identification;
            InpsectorName = _identification.ClassName;
        }

    }
}
