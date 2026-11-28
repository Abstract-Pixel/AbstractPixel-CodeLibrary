using UnityEngine;

namespace AbstractPixel.Core.Editor.UI
{
    /// <summary>A small flat white gear for the system pill, drawn in memory so no image asset is needed.</summary>
#if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
#endif
    internal static class SystemPillIcon
    {
        private const int Size = 18;
        private static Texture2D cached;

        internal static Texture2D Get()
        {
            if (cached != null)
            {
                return cached;
            }
            Texture2D texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[Size * Size];
            DrawGear(pixels, 9f, 9f, 7.5f, 3f);
            texture.SetPixels(pixels);
            texture.Apply();
            cached = texture;
            return cached;
        }

        private static void DrawGear(Color[] _pixels, float _centreX, float _centreY, float _outerRadius, float _innerRadius)
        {
            float toothSpan = _outerRadius + 1.6f;
            float sixth = Mathf.PI * 2f / 6f;
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float deltaX = x + 0.5f - _centreX;
                    float deltaY = y + 0.5f - _centreY;
                    float distance = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
                    bool inRing = distance <= _outerRadius && distance >= _innerRadius;
                    bool inTooth = false;
                    if (!inRing && distance <= toothSpan && distance > _outerRadius - 1f)
                    {
                        float phase = Mathf.Repeat(Mathf.Atan2(deltaY, deltaX) + Mathf.PI, sixth);
                        inTooth = phase < sixth * 0.5f;
                    }
                    if (inRing || inTooth)
                    {
                        _pixels[(Size - 1 - y) * Size + x] = Color.white;
                    }
                }
            }
        }
    }
}
