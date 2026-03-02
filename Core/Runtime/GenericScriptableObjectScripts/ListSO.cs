using System.Collections.Generic;
using UnityEngine;

public abstract class ListSO<T> : ScriptableObject
{
    public List<T> Items = new List<T>();
}