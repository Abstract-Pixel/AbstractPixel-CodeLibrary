using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// A relocation label: an asset that names a kind of relocatable, such as "Player" or "Enemy" (ADR 0006).
    /// Relocatables and spawn point positions match by label. An empty label field is "No label", a valid label of its
    /// own, so a game that relocates only its player needs no asset.
    /// </summary>
    [CreateAssetMenu(fileName = "RelocationLabel", menuName = "Abstract Pixel/Scene System/Relocation Label", order = 40)]
    public class RelocationLabel : ScriptableObject
    {
        [Tooltip("Optional. Drawn at every spawn point position with this label when no relocatable with it is open in the Editor.")]
        [SerializeField] private GameObject previewPrefab;
        [Tooltip("The colour of this label's previews and tags in the Scene view.")]
        [SerializeField] private Color colour = new Color(0.35f, 0.8f, 1f, 1f);
        [Tooltip("Steepest ground: the steepest slope, in degrees, this label accepts for a spawn point position. A steeper position is allowed but shown as a warning.")]
        [Range(0f, 90f)] [SerializeField] private float steepestGround = 35f;
        [Tooltip("Align to surface: positions added with Shift+click tilt to the ground's slope. Off keeps them upright.")]
        [SerializeField] private bool alignToSurface = false;

        public GameObject PreviewPrefab
        {
            get { return previewPrefab; }
            set { previewPrefab = value; }
        }

        public Color Colour
        {
            get { return colour; }
            set { colour = value; }
        }

        public float SteepestGround
        {
            get { return steepestGround; }
            set { steepestGround = Mathf.Clamp(value, 0f, 90f); }
        }

        public bool AlignToSurface
        {
            get { return alignToSurface; }
            set { alignToSurface = value; }
        }
    }
}
