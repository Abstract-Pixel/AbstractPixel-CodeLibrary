using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    /// <summary>An asset that names a kind of relocatable, such as "Player" or "Enemy" (ADR 0006).</summary>
    /// <remarks>
    /// - Relocatables and spawn point positions match by label.
    /// - An empty label field is "No label", a valid label of its own, so a game that relocates only its player needs no asset.
    /// </remarks>
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
        [Tooltip("The label's stable ID: its asset GUID, stamped in the Editor. Labels match by it, so a copy of this asset inside an Addressables bundle still counts as the same label.")]
        [SerializeField, HideInInspector] private string id = string.Empty;

        /// <summary>The stable ID labels match by; empty only for a label made in code that was never saved.</summary>
        public string Id
        {
            get { return id; }
            internal set { id = value; }
        }

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

#if UNITY_EDITOR
        // A new, moved or duplicated label takes its own asset GUID as its ID.
        private void OnValidate()
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            string guid = UnityEditor.AssetDatabase.AssetPathToGUID(path);
            if (id != guid)
            {
                id = guid;
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}
