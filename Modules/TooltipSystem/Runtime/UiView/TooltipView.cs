using UnityEngine;
using AbstractPixel.Core;
using TMPro;
using UnityEngine.UI;

namespace AbstractPixel.Tooltip
{
    [RequireComponent(typeof(TooltipPositioner),typeof(DynamicPanelWidthController))]
    public class TooltipView : MonoBehaviour, IInitializable<TooltipData>
    {
        [field: SerializeField] public RectTransform tooltipHolder { get; private set; }
        [field:SerializeField] public TMP_Text headerText {  get; private set; }
        [field:SerializeField] public TMP_Text bodyText {  get; private set; }
        [field:SerializeField] public Image iconHolder {  get; private set; }

        [SerializeField] TooltipFeedback tooltipFeedback;
        [SerializeField,ReadOnly] TooltipPositioner tooltipPositioner;

        private void OnValidate()
        {
            if(tooltipPositioner == null)
            {
                tooltipPositioner = GetComponent<TooltipPositioner>();
            }
        }

        private void Awake()
        {
            if(tooltipPositioner == null)
            {
                tooltipPositioner= GetComponent<TooltipPositioner>();
            }
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