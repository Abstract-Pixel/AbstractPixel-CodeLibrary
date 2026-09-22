using System;

namespace AbstractPixel.Settings
{
    [Serializable] 
    public class GamepadYSensitivitySetting : BaseCameraSensitivitySetting 
    { 
        public override CameraAxisTarget TargetAxis => CameraAxisTarget.VerticalY; 
    }
}