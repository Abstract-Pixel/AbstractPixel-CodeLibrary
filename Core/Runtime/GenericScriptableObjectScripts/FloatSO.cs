using UnityEngine;

[CreateAssetMenu(fileName = "FloatSO", menuName = "GenericSOLibrary/FloatSO", order = 1)]
public class FloatSO : ValueTypeSO<float>
{
    protected override float ClampValue(float value)
    {
        return Mathf.Clamp(value, MinValue, MaxValue);
    }
}
