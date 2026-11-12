using System;
using System.Reflection;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    [Serializable]
    public class SavableTarget
    {
        [HideInInspector][SerializeField] public string InspectorName;
        public MonoBehaviour Script;
        public SavableIdentification Identification;

        // Runtime Only
        public MethodInfo CaptureDataMethod;
        public MethodInfo RestoreDataMethod;
        public Type DataToSaveType;

        public SavableTarget(MonoBehaviour _script, SavableIdentification _identification)
        {
            Script = _script;
            Identification = _identification;
            InspectorName = _identification.ClassName;
        }

    }
}
