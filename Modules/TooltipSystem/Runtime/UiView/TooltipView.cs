using UnityEngine;
using AbstractPixel.Core;
using TMPro;
using UnityEngine.UI;

namespace AbstractPixel.Tooltip
{
    public class TooltipView : MonoBehaviour, IInitializable<TooltipData>
    {
        [field: SerializeField] public GameObject tooltipHolder { get; private set; }
        [field:SerializeField] public TMP_Text headerText {  get; private set; }
        [field:SerializeField] public TMP_Text bodyText {  get; private set; }
        [field:SerializeField] public Image iconHolder {  get; private set; }

        [SerializeField] TooltipFeedback tooltipFeedback;
        public void Initialize(TooltipData _data)
        {
            tooltipFeedback.Initialize(this);
            if(_data.Header != null)
            {
                headerText.text = _data.Header;
            }

            if (_data.Body != null)
            {
                bodyText.text = _data.Body;
            }

            if (_data.Icon != null)
            { 
                iconHolder.sprite = _data.Icon;
            }
        }      

        public void Show()
        {
            tooltipFeedback.ExecuteShowFeedback();
        }

        public void Hide()
        {
            tooltipFeedback?.ExecuteHideFeedback();
        }
    }
}