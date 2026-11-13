using UnityEngine;
using UnityEngine.UI;
using CustomInspector;

namespace AbstractPixel.Core
{
   
    public sealed class GlyphToggler : MonoBehaviour
    {
        [SerializeField] private Image targetImage;
        [SerializeField] private ReorderableDictionary<DeviceFamily, Sprite> glyphMap;

        private bool cachedRaycastTarget;

        private void Awake()
        {
            if (targetImage == null) targetImage = GetComponent<Image>();
            cachedRaycastTarget = targetImage.raycastTarget;
        }

        private void OnEnable()
        {
            InputDeviceTracker.OnDeviceFamilyChanged += HandleDeviceFamilyChanged;
            ApplyDeviceFamily(InputDeviceTracker.CurrentDeviceFamily);
        }

        private void OnDisable()
        {
            InputDeviceTracker.OnDeviceFamilyChanged -= HandleDeviceFamilyChanged;
        }

        private void HandleDeviceFamilyChanged(DeviceFamily newFamily)
        {
            ApplyDeviceFamily(newFamily);
        }

        private void ApplyDeviceFamily(DeviceFamily family)
        {
            if (glyphMap != null && glyphMap.TryGetValue(family, out Sprite sprite) && sprite != null)
            {
                SetGlyphVisible(sprite);
            }
            else
            {
                SetGlyphHidden();
            }
        }

        private void SetGlyphVisible(Sprite sprite)
        {
            targetImage.sprite = sprite;
            // Keeps the current colour, because a theme may have recoloured the glyph since Awake.
            Color currentColor = targetImage.color;
            targetImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, 1f);
            targetImage.raycastTarget = cachedRaycastTarget;
        }

        private void SetGlyphHidden()
        {
            Color currentColor = targetImage.color;
            targetImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0f);
            targetImage.raycastTarget = false; // Prevents transparent quad from blocking clicks
        }
    }

    public interface IGlyphProvider
    {
        bool TryGetGlyph(DeviceFamily family, out Sprite glyph);
    }
}
