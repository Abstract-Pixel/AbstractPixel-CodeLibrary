using UnityEngine;
using AbstractPixel.Core;
using TMPro;
using UnityEngine.UI;

namespace AbstractPixel.Tooltip
{
    [RequireComponent(typeof(TooltipPositioner),typeof(DynamicPanelWidthController), typeof(CanvasGroup))]
    public class TooltipView : MonoBehaviour, IInitializable<TooltipData>
    {
        [field: SerializeField] public RectTransform tooltipHolder { get; private set; }
        [field:SerializeField] public TMP_Text headerText {  get; private set; }
        [field:SerializeField] public TMP_Text bodyText {  get; private set; }
        [field:SerializeField] public Image iconHolder {  get; private set; }

        [SerializeField] TooltipFeedback tooltipFeedback;
        [SerializeField,ReadOnly] TooltipPositioner tooltipPositioner;
        private CanvasGroup canvasGroup;
        private void OnValidate()
        {
            if(tooltipPositioner == null)
            {
                tooltipPositioner = GetComponent<TooltipPositioner>();
            }
        }

        private void Awake()
        {
            if (tooltipPositioner == null)
            {
                tooltipPositioner = GetComponent<TooltipPositioner>();
            }

            canvasGroup = GetComponent<CanvasGroup>();

            // Proactive Flicker Fix: This guarantees the tooltip can NEVER block the mouse raycast, 
            // preventing the infinite OnPointerEnter/Exit loop regardless of designer setup.
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        public void Initialize(TooltipData _hoveredData)
        {
            tooltipFeedback.Initialize(this);
            tooltipHolder.gameObject.SetActive(true);
            tooltipPositioner.Setup(tooltipHolder,_hoveredData.Config, _hoveredData.transform);
            tooltipPositioner.ForcePositionUpdate();
            if(_hoveredData.Header != null && headerText!=null)
            {
                headerText.text = _hoveredData.Header;
            }

            if (_hoveredData.Body != null && bodyText != null)
            {
                bodyText.text = _hoveredData.Body;
            }

            if (_hoveredData.Icon != null && iconHolder != null)
            { 
                iconHolder.sprite = _hoveredData.Icon;
            }
        }      

        public void Show()
        {
            tooltipFeedback.ExecuteShowFeedback();
            tooltipPositioner.EnablePositioning();
        }

        public void Hide()
        {
            tooltipFeedback?.ExecuteHideFeedback();
            tooltipPositioner?.DisablePositioning();
        }
    }
}