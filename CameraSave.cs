using SharpPluginLoader.Core;
using System.Text.Json;

namespace DynamicCamera
{
    public class CameraSave
    {
        public bool PluginFlag { get; set; }
        public CameraClass? BaseCamera { get; set; }
        public CombatCamera? CombatCamera { get; set; }

        public CameraSave()
        {
            // Set default values for BaseCamera and CombatCamera
            PluginFlag = true;
            BaseCamera = new CameraClass { CameraDistance = 90f, CameraHeight = -40f };
            CombatCamera = new CombatCamera { CameraDistance = -210f, CameraHeight = 10f, ADSFlag = false };
        }
    }
    public class CameraClass
    {
        public float CameraDistance { get; set; }
        public float CameraHeight { get; set; }
    }

    public class CombatCamera : CameraClass
    {
        public bool ADSFlag;
    }
}
