using UnityEngine;

namespace AbstractPixel.Core
{
    public class GlobalShaderUnscaledTime : MonoBehaviour
    {

        void Update()
        {
            Shader.SetGlobalFloat("_unscaledTime", Time.unscaledTime);
        }
    }
}