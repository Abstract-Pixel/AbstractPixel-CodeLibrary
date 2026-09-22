using System;

namespace AbstractPixel.Settings
{
    [Serializable] 
    public class MouseXSensitivitySetting : BaseCameraSensitivitySetting 
    { 
        public override CameraAxisTarget TargetAxis => CameraAxisTarget.HorizontalX; 
    }
}