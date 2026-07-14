using System;
using UnityEngine;

namespace AbstractPixel.Core
{
    [Serializable]
    public class PolymorphicType<TBase> where TBase : class
    {
        public string TclassName;
        public Type TBaseType;
    }
}
