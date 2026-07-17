using UnityEngine;

namespace AbstractPixel.Tooltip
{
    public class StaticTooltipData : MonoBehaviour, ITooltipDataProvider
    {
        [SerializeField] private string header;
        [SerializeField] private string body;
        [SerializeField] private Sprite icon;
        public TooltipData GetTooltipData()
        {
            TooltipData tooltipData = new TooltipData
            {
                Header = header,
                Body = body,
                Icon = icon,
                transform = this.transform,
            };
            return tooltipData;
        }
    }
}