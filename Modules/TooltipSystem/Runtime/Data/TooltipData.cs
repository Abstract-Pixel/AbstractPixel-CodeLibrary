
using UnityEngine;

namespace AbstractPixel.Tooltip
{
    [System.Serializable]
    public struct TooltipData
    {
        public string Header;
        public string Body;
        public Sprite Icon;
        public Transform transform;

        // The specific configuration this data should be displayed with
        public TooltipConfig Config;

        public TooltipData(string _header, string _body, Sprite _icon, TooltipConfig _config, Transform _transform)
        {
            Header = _header;
            Body = _body;
            Icon = _icon;
            Config = _config;
            transform = _transform;
        }
    }
}