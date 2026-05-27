using UnityEngine;

namespace AbstractPixel.GhostSystem
{
    /// <summary>
    /// A generic, performance-friendly visual fader that uses MaterialPropertyBlocks 
    /// to adjust alpha based on distance to a target without duplicating materials in memory.
    /// </summary>
    public class SimpleProximityVisualFader : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The object to measure distance against (e.g., the Player Car).")]
        [SerializeField] private Transform targetTransform;

        [Space]
        [Tooltip("Assign the root object containing the car visuals. OnValidate will auto-populate the renderers array.")]
        [SerializeField] private Transform parentTransformVisual;
        [Tooltip("All renderers that should fade. They should share the same material logic.")]
        [SerializeField] private Renderer[] targetRenderers;

        [Header("Distance Settings")]
        [Tooltip("Distance at which the object becomes completely invisible (Alpha = 0).")]
        [SerializeField] private float minFadeDistance = 5f;
        [Tooltip("Distance at which the object is fully visible (Alpha = Cached Alpha).")]
        [SerializeField] private float maxFadeDistance = 15f;

        [Header("Shader Settings")]
        [Tooltip("Use '_BaseColor' for URP/HDRP, or '_Color' for Built-in Pipeline.")]
        [SerializeField] private string colorPropertyName = "_BaseColor";

        private MaterialPropertyBlock propertyBlock;
        private int colorPropertyID;
        private Color cachedBaseColor;
        private bool isInitialized = false;

        private void OnValidate()
        {
            // Designer-Proof Tooling: Auto-fetch all renderers if a parent visual is assigned.
            if (parentTransformVisual != null)
            {
                // Passing 'true' ensures we also grab inactive renderers just in case.
                targetRenderers = parentTransformVisual.GetComponentsInChildren<Renderer>(true);
            }
        }


        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
            colorPropertyID = Shader.PropertyToID(colorPropertyName);
        }

        private void Start()
        {
            if (targetRenderers == null || targetRenderers.Length == 0)
            {
                Debug.LogWarning("[SimpleProximityVisualFader] No renderers assigned. Disabling fader.", this);
                enabled = false;
                return;
            }

            // CRITICAL: Read from sharedMaterial to prevent Unity from silently creating a duplicate material instance.
            Material baseMat = targetRenderers[0].sharedMaterial;
            if (baseMat != null && baseMat.HasProperty(colorPropertyID))
            {
                cachedBaseColor = baseMat.GetColor(colorPropertyID);
                isInitialized = true;
            }
            else
            {
                Debug.LogError($"[SimpleProximityVisualFader] Material does not have property '{colorPropertyName}'.", this);
                enabled = false;
            }
        }

        private void FixedUpdate()
        {
            if (!isInitialized || targetTransform == null) return;
            float distance = Vector3.Distance(transform.position, targetTransform.position);

            // Calculate alpha multiplier (0.0 to 1.0)
            // InverseLerp automatically clamps the result between 0 and 1.
            float alphaMultiplier = Mathf.InverseLerp(minFadeDistance, maxFadeDistance, distance);

            Color targetColor = cachedBaseColor;
            targetColor.a = cachedBaseColor.a * alphaMultiplier;

            // Push the new color to the GPU via MaterialPropertyBlock
            for (int i = 0; i < targetRenderers.Length; i++)
            {
                if (targetRenderers[i] == null) continue;

                // Always Get before Set to preserve other property block overrides (like emission/metallic)
                targetRenderers[i].GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(colorPropertyID, targetColor);
                targetRenderers[i].SetPropertyBlock(propertyBlock);
            }
        }
        public void SetTarget(Transform _newTarget)
        {
            targetTransform = _newTarget;
        }
    }
}