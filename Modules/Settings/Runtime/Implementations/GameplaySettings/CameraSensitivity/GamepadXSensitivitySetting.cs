using System;

namespace AbstractPixel.Settings
{
    [Serializable] 
    public class GamepadXSensitivitySetting : BaseCameraSensitivitySetting 
    { 
        public override CameraAxisTarget TargetAxis => CameraAxisTarget.HorizontalX; 
    }
}