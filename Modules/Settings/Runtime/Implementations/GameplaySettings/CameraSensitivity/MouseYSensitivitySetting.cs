using System;

namespace AbstractPixel.Settings
{
    [Serializable] 
    public class MouseYSensitivitySetting : BaseCameraSensitivitySetting 
    { 
        public override CameraAxisTarget TargetAxis => CameraAxisTarget.VerticalY; 
    }
}