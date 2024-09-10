using ImGuiNET;
using SharpPluginLoader.Core;
using SharpPluginLoader.Core.Memory;
using SharpPluginLoader.Core.View;
using SharpPluginLoader.Core.Rendering;
using System.Text.Json;

namespace DynamicCamera
{
    public class Plugin : IPlugin
    {
        public string Name => "Dynamic Camera";

        public string Author => "Psly";

        private Patch _distPatch;

        public CameraSave? cameraSave;
        private const string FILE_PATH = ".\\nativePC\\plugins\\CSharp\\camera_config.json";

        private const nint DISTANCE_OFFSET = 0x748;
        private const nint HEIGHT_OFFSET = 0x744;

        private const float CAMERA_DISTANCE_MIN = -1500f;
        private const float CAMERA_DISTANCE_MAX = 2000f;
        private const float CAMERA_HEIGHT_MIN = -1125f;
        private const float CAMERA_HEIGHT_MAX = 1950f;

        private readonly Stage[] nonCombatStages = StageConditionals.nonCombatStages;
        private readonly Dictionary<Stage, float> FOVRange = StageConditionals.FOVRanges;

        private bool pluginFlag;
        private float cameraBaseDistance;
        private float cameraBaseHeight;
        private float cameraCombatDistance;
        private float cameraCombatHeight;
        private bool combatADSFlag;

        public CameraSave? DeserializeFromJsonFile(string filePath)
        {
            try
            {
                var jsonString = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<CameraSave>(jsonString);
            }
            catch (Exception ex)
            {
                Log.Error($"Error during JSON serialization/deserialization: {ex.Message}");
                ImGuiExtensions.NotificationError($"Camera config did not load : {ex.Message}");
            }
            return null;
        }

        public void SerializeToJsonFile(CameraSave cameraSave, string filePath)
        {
            cameraSave.PluginFlag = pluginFlag;
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true // This makes the output JSON formatted for better readability
            };
            try
            {
                File.WriteAllText(filePath, JsonSerializer.Serialize(cameraSave, options));
                ImGuiExtensions.NotificationSuccess("Camera config saved");
            }
            catch (Exception ex)
            {
                ImGuiExtensions.NotificationError($"Camera config did not save : {ex.Message}");
                Log.Error($"Error during JSON serialization: {ex.Message}");
            }
        }

        public PluginData Initialize() 
        {
            return new PluginData();
        }

        public void OnLoad()
        {
            Log.Info("Loaded Dynamic Camera");
            var nopBytes = Enumerable.Repeat<byte>(0x90, 15).ToArray();
            _distPatch = new Patch(unchecked((nint)0x141fa6504), nopBytes, true);

            cameraSave = File.Exists(FILE_PATH) ? DeserializeFromJsonFile(FILE_PATH) ?? new CameraSave() : new CameraSave();

            pluginFlag = cameraSave.PluginFlag;
            cameraBaseDistance = cameraSave.BaseCamera.CameraDistance;
            cameraBaseHeight = cameraSave.BaseCamera.CameraHeight;
            cameraCombatDistance = cameraSave.CombatCamera.CameraDistance;
            cameraCombatHeight = cameraSave.CombatCamera.CameraHeight;
            combatADSFlag = cameraSave.CombatCamera.ADSFlag;
        }

        private bool IsInNonCombatArea() => nonCombatStages.Contains(Area.CurrentStage);

        private bool CheckFOV(Stage stage, float FOV)
        {
            if (combatADSFlag && !IsInNonCombatArea()) 
            {
                if (FOVRange.TryGetValue(stage, out var fovRange))
                {
                    return FOV >= 45f && FOV <= (fovRange + 4f);
                }
            }
            else 
            {
                if (FOVRange.TryGetValue(stage, out var fovRange))
                {
                    return FOV >= fovRange && FOV <= (fovRange + 4f);
                }
            }
            return FOV >= 53f && FOV <= 57f;
        }
        private void DistpatchSwitch(bool distPatchFlag, Camera camera)
        {
            if (camera.Is("uInterpolationCamera")) 
            {
                camera.GetRef<float>(DISTANCE_OFFSET) = 0f;
                _distPatch.Disable();
                return;
            }

            if (!(camera.Is("uMhCamera")))
                return;

            if (camera.Get<float>(HEIGHT_OFFSET) != 0f)
            {
                if (!float.IsNormal(camera.GetRef<float>(HEIGHT_OFFSET)))
                {
                    _distPatch.Disable();
                    return;
                }
            }

            if (camera.Get<float>(DISTANCE_OFFSET) != 0f)
            {
                if (!float.IsNormal(camera.GetRef<float>(DISTANCE_OFFSET)))
                {
                    _distPatch.Disable();
                    return;
                }
            }

            float actualCamDistance = (float)Math.Round(camera.Get<float>(DISTANCE_OFFSET), 6);
            float actualCamHeight = (float)Math.Round(camera.Get<float>(HEIGHT_OFFSET), 6);

            if (distPatchFlag)
            {
                float tempCustomDistanceEN = IsInNonCombatArea() ? cameraBaseDistance : cameraCombatDistance;
                float tempCustomHeightEN = IsInNonCombatArea() ? cameraBaseHeight : cameraCombatHeight;

                _distPatch.Enable();

                camera.GetRef<float>(DISTANCE_OFFSET) = Utils.CameraSmooth(actualCamDistance, tempCustomDistanceEN);
                camera.GetRef<float>(HEIGHT_OFFSET) = Utils.CameraSmooth(actualCamHeight, tempCustomHeightEN);
            }
            else
            { 
                if (actualCamDistance >= CAMERA_DISTANCE_MIN && actualCamDistance <= CAMERA_DISTANCE_MAX)
                {
                    if (Math.Abs(actualCamDistance) > 0.1f)
                    {
                        camera.GetRef<float>(DISTANCE_OFFSET) = Utils.CameraSmooth(actualCamDistance, 0f);
                    }
                    else
                    {
                        _distPatch.Disable();
                        return;
                    }
                      
                }
                if (actualCamHeight >= CAMERA_HEIGHT_MIN && actualCamHeight <= CAMERA_HEIGHT_MAX)
                {
                    if (Math.Abs(actualCamHeight) > 0.1f)
                    {
                        camera.GetRef<float>(HEIGHT_OFFSET) = Utils.CameraSmooth(actualCamHeight, 0f);
                    }
                    else
                    {
                        _distPatch.Disable();
                        return;
                    }
                }
            }
        }

        private void RenderCameraControls(Camera camera)
        {   
            if (IsInNonCombatArea())
            {
                ImGui.Text($"Base/Hub Camera Settings");

                ImGui.DragFloat("Camera Distance", ref cameraBaseDistance, 1f, CAMERA_DISTANCE_MIN, CAMERA_DISTANCE_MAX);
                ImGui.DragFloat("Camera Height", ref cameraBaseHeight, 1f, CAMERA_HEIGHT_MIN, CAMERA_HEIGHT_MAX);

                cameraSave.BaseCamera.CameraDistance = cameraBaseDistance;
                cameraSave.BaseCamera.CameraHeight = cameraBaseHeight;
            }
            else
            {
                ImGui.Text($"Combat Camera Settings");

                ImGui.DragFloat("Camera Distance", ref cameraCombatDistance, 1f, CAMERA_DISTANCE_MIN, CAMERA_DISTANCE_MAX);
                ImGui.DragFloat("Camera Height", ref cameraCombatHeight, 1f, CAMERA_HEIGHT_MIN, CAMERA_HEIGHT_MAX);

                cameraSave.CombatCamera.CameraDistance = cameraCombatDistance;
                cameraSave.CombatCamera.CameraHeight = cameraCombatHeight;
            }

            ImGui.Checkbox("Enable it for ADS in Combat Zones", ref combatADSFlag);
            cameraSave.CombatCamera.ADSFlag = combatADSFlag;
        }

        public void OnImGuiRender()
        {
            ImGui.Checkbox("Enable", ref pluginFlag);
            if (Area.CurrentStage == 0)
                return;
            var camera = CameraSystem.MainViewport.Camera;
            if (!camera.Is("uMhCamera"))
                return;
            if (camera is not null)
            {
                //var weapon = Player.MainPlayer?.CurrentWeapon;
                //ImGui.Text($"Area - {Area.CurrentStage} FOV - {camera.FieldOfView}");
                //ImGui.Text($"Type - {camera.GetDti().Name}");
                //ImGui.Text($"Distance - {camera.Get<float>(DISTANCE_OFFSET)} | Height - {camera.Get<float>(HEIGHT_OFFSET)}");
                //ImGui.Text($"Weapon Type - {weapon.GetDti().Name}");
                if (CheckFOV(Area.CurrentStage, camera.FieldOfView))
                {
                    RenderCameraControls(camera);
                }
            }
            if (ImGui.Button("Save"))
            {              
                SerializeToJsonFile(cameraSave, FILE_PATH);
                Log.Info("Settings saved in \\nativePC\\plugins\\CSharp\\camera_config.json");
            }
        }

        public void OnUpdate(float dt)
        {
            if (this.pluginFlag) 
            {
                if (Area.CurrentStage == 0)
                    return;
                var camera = CameraSystem.MainViewport.Camera;
                if (camera is not null)
                {
                    if (CheckFOV(Area.CurrentStage, camera.FieldOfView))
                    {
                        DistpatchSwitch(true, camera);
                    }
                    else
                    {
                        DistpatchSwitch(false, camera);
                    }
                }
            }
            else 
            {
                if (this._distPatch.IsEnabled) 
                {
                    this._distPatch.Disable();
                }
            }
        }
    }
}