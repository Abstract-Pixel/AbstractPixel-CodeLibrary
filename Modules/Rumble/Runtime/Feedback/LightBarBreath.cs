using UnityEngine;

namespace AbstractPixel.Rumble
{
    /// <summary>Slow "breathing" light bar pulse: the colour dims and brightens smoothly, one breath per period.</summary>
    public static class LightBarBreath
    {
        /// <summary>_shape.x = seconds per breath, _shape.y = brightness at the dimmest point (0..1).</summary>
        public static Color Evaluate(Color _color, Vector2 _shape, float _unscaledTime)
        {
            float period = Mathf.Max(0.1f, _shape.x);
            float phase = _unscaledTime / period;
            float wave = 0.5f - 0.5f * Mathf.Cos(phase * Mathf.PI * 2f);
            float brightness = Mathf.Lerp(_shape.y, 1f, wave);
            Color dimmed = _color * brightness;
            dimmed.a = 1f;
            return dimmed;
        }
    }
}
