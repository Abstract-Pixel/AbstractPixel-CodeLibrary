using System;
using UnityEngine;

namespace AbstractPixel.Core
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    namespace AbstractPixel.Core
    {
        [Serializable]
        public class PolymorphicType<TBase> where TBase : class
        {
            [SerializeField]
            private string selectedClassName = string.Empty;

            [SerializeField]
            private string selectedClassAssemblyQualifiedName = string.Empty;

            [SerializeField]
            private string[] compatibleClassAssemblyQualifiedNames = Array.Empty<string>();

            // Public properties with read-only/private access as requested
            public string TClassName => selectedClassName;

            public Type TBaseType
            {
                get
                {
                    if (string.IsNullOrEmpty(selectedClassAssemblyQualifiedName) == true)
                    {
                        return null;
                    }

                    Type resolvedType = Type.GetType(selectedClassAssemblyQualifiedName);
                    return resolvedType;
                }
            }

            public Type[] CompatibleTypes
            {
                get
                {
                    if (compatibleClassAssemblyQualifiedNames == null)
                    {
                        return Array.Empty<Type>();
                    }

                    List<Type> resolvedTypesList = new List<Type>();

                    foreach (string assemblyQualifiedName in compatibleClassAssemblyQualifiedNames)
                    {
                        Type resolvedType = Type.GetType(assemblyQualifiedName);

                        if (resolvedType != null)
                        {
                            resolvedTypesList.Add(resolvedType);
                        }
                    }

                    return resolvedTypesList.ToArray();
                }
            }
        }
    }
}
