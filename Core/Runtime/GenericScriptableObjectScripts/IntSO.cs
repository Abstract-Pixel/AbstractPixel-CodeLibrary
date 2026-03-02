using UnityEngine;

[CreateAssetMenu(fileName = "IntSO", menuName = "GenericSOLibrary/IntSO", order =0)]
public class IntSO : ValueTypeSO<int>
{
    protected override int ClampValue(int value)
    {
        return Mathf.Clamp(value,MinValue,MaxValue);
    }
}
