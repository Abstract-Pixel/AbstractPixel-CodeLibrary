using UnityEngine;

public class GlobalShaderUnscaledTime : MonoBehaviour
{
    
    void Update()
    {
        Shader.SetGlobalFloat("_unscaledTime", Time.unscaledTime);
    }
}
