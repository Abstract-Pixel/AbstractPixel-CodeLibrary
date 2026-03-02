using System;
using UnityEngine;

namespace AbstractPixel.Core
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple =  false)]
    public class ReadOnlyAttribute : PropertyAttribute
    {
        public bool IsEditable { get; private set; } = false;

        public ReadOnlyAttribute(bool _isEditable = false)
        {
            IsEditable = _isEditable;
        }
    }
}
