using AbstractPixel.Core;

namespace AbstractPixel.InputRebinding
{
    public readonly struct BindingDisplayPayload
    {
        public readonly string ActionName;
        public readonly string DisplayString;
        public readonly string DeviceLayoutName;
        public readonly string ControlPath;
        public readonly DeviceFamily ActiveDeviceFamily;

        public BindingDisplayPayload(
            string _actionName,
            string _displayString,
            string _deviceLayoutName,
            string _controlPath,
            DeviceFamily _activeDeviceFamily)
        {
            ActionName = _actionName;
            DisplayString = _displayString;
            DeviceLayoutName = _deviceLayoutName;
            ControlPath = _controlPath;
            ActiveDeviceFamily = _activeDeviceFamily;
        }
    }
}