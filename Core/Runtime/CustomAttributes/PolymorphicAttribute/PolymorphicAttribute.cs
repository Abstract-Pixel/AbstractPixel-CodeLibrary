using System;
using UnityEngine;

namespace AbstractPixel.Core
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class PolymorphicAttribute : PropertyAttribute
    {

    }
}