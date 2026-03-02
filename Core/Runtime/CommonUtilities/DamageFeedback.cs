using UnityEngine;
using System.Collections;

[System.Serializable]
public class DamageFeedbackSettings
{
    [Header("Timing")]
    public float duration = 0.5f;
    public AnimationCurve damageCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Visual Effects")]
    public bool animateScale = true;
    public float takeDamageScale = 0.8f;
    public Color takeDamageColor = Color.red;
    [Range(0, 10)] public float emissionIntensity = 2f;
}

public class DamageFeedback : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private Transform scaleParent;
    [SerializeField] private MeshRenderer mainRenderer;
    [SerializeField] private Renderer[] targetRenderers;

    [Header("Settings")]
    [SerializeField] private bool useMultipleRenderers = false;
    [SerializeField] private DamageFeedbackSettings settings = new DamageFeedbackSettings();

    // Scale animation state
    private Vector3 initialScale;
    private Vector3[] initialScales;

    // Color animation state
    private Color[] initialBaseColors;
    private Color[] initialEmissionColors;

    // Animation state
    private bool isAnimating;
    private float elapsedTime;

    /*   #region Unity Events
     private void OnEnable() => IDamageable.OnDamageTaken += HandleDamageTaken;
      private void OnDisable() => IDamageable.OnDamageTaken -= HandleDamageTaken;

      private void Start()
      {
          InitializeScaleReferences();
          InitializeMaterialReferences();
      }

      private void Update()
      {
          if (!isAnimating) return;
          UpdateDamageAnimation();
      }
      #endregion

      #region Initialization
      private void InitializeScaleReferences()
      {
          var targetTransform = scaleParent ? scaleParent : transform;

          if (useMultipleRenderers)
          {
              targetRenderers = GetComponentsInChildren<Renderer>();
              initialScales = new Vector3[targetRenderers.Length];
              for (int i = 0; i < targetRenderers.Length; i++)
              {
                  initialScales[i] = targetRenderers[i].transform.localScale;
              }
          }
          else
          {
              mainRenderer = mainRenderer ? mainRenderer : GetComponent<MeshRenderer>();
              initialScale = targetTransform.localScale;
          }
      }

      private void InitializeMaterialReferences()
      {
          Renderer[] renderers = useMultipleRenderers ? targetRenderers : new Renderer[] { mainRenderer };

          initialBaseColors = new Color[renderers.Length];
          initialEmissionColors = new Color[renderers.Length];

          for (int i = 0; i < renderers.Length; i++)
          {
              if (!renderers[i]) continue;

              Material mat = renderers[i].material;
              initialBaseColors[i] = mat.HasProperty("_BaseColor")
                  ? mat.GetColor("_BaseColor")
                  : mat.color;

              initialEmissionColors[i] = mat.HasProperty("_EmissionColor")
                  ? mat.GetColor("_EmissionColor")
                  : Color.black;
          }
      }
      #endregion

      #region Damage Handling
      private void HandleDamageTaken(int damage, GameObject damagedObject)
      {
          if (damagedObject == gameObject && damage > 0)
          {
              StartDamageAnimation();
          }
      }

      private void StartDamageAnimation()
      {
          elapsedTime = 0f;
          isAnimating = true;
      }

      private void UpdateDamageAnimation()
      {
          elapsedTime += Time.deltaTime;
          float progress = Mathf.Clamp01(elapsedTime / settings.duration);
          float curveValue = settings.damageCurve.Evaluate(progress);

          if (useMultipleRenderers)
          {
              UpdateMultipleRenderers(curveValue);
          }
          else
          {
              UpdateSingleRenderer(curveValue);
          }

          if (progress >= 1f) isAnimating = false;
      }
      #endregion

      #region Renderer Updates
      private void UpdateSingleRenderer(float t)
      {
          if (!mainRenderer) return;

          UpdateRendererVisuals(mainRenderer,
              scaleParent ? scaleParent : mainRenderer.transform,
              initialScale,
              initialBaseColors[0],
              initialEmissionColors[0],
              t);
      }

      private void UpdateMultipleRenderers(float t)
      {
          for (int i = 0; i < targetRenderers.Length; i++)
          {
              if (!targetRenderers[i]) continue;

              UpdateRendererVisuals(targetRenderers[i],
                  targetRenderers[i].transform,
                  initialScales[i],
                  initialBaseColors[i],
                  initialEmissionColors[i],
                  t);
          }
      }

      private void UpdateRendererVisuals(Renderer renderer, Transform targetTransform,
          Vector3 initialScale, Color baseColor, Color emissionColor, float t)
      {
          if (settings.animateScale)
          {
              UpdateScale(targetTransform, initialScale, t);
          }

          UpdateMaterials(renderer, baseColor, emissionColor, t);
      }
      #endregion

      #region Visual Updates
      private void UpdateScale(Transform target, Vector3 initialScale, float t)
      {
          float scaleModifier = Mathf.Lerp(settings.takeDamageScale, 1f, t);
          target.localScale = initialScale * scaleModifier;
      }

      private void UpdateMaterials(Renderer renderer, Color baseColor, Color emissionColor, float t)
      {
          foreach (Material mat in renderer.materials)
          {
              // Update base color
              Color currentColor = Color.Lerp(settings.takeDamageColor, baseColor, t);
              if (mat.HasProperty("_BaseColor"))
                  mat.SetColor("_BaseColor", currentColor);
              else
                  mat.color = currentColor;

              // Update emission
              Color emission = Color.Lerp(
                  settings.takeDamageColor * settings.emissionIntensity,
                  emissionColor,
                  t
              );

              if (mat.HasProperty("_EmissionColor"))
                  mat.SetColor("_EmissionColor", emission);
          }
      }
      #endregion */
}