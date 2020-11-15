

using System;
/**
* Copyright (c) 2017-present, PFW Contributors.
*
* Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in
* compliance with the License. You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software distributed under the License is
* distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See
* the License for the specific language governing permissions and limitations under the License.
*//// <summary>
  /// The classes here represent purely what we write
  /// in our json config for the user settings.
  /// </summary>
namespace PFW.Model.Settings.JsonContents
{
    // Careful: If in the future we add config fields that
    //          allow zero, then we wont be able to distinguish
    //          unset values from values explicitly set to zero 
    //          in the local config. Such values should be wrapped
    //          in an object (which will be null on missing value).

    public class SettingsConfig
    {
        public CameraConfig Camera;
        public HotkeyConfig Hotkeys;

        /// <summary>
        /// Values that are set in the local config always
        /// override the values from the default config.
        /// </summary>
        internal static SettingsConfig MergeSettings(
                SettingsConfig defaultConfig, SettingsConfig localConfig)
        {
            return new SettingsConfig
            {
                Camera = new CameraConfig()
                {
                    ZoomSpeed = localConfig.Camera.ZoomSpeed == 0 ?
                            defaultConfig.Camera.ZoomSpeed : localConfig.Camera.ZoomSpeed,

                    RotationSpeed = localConfig.Camera.RotationSpeed == 0 ?
                            defaultConfig.Camera.RotationSpeed : localConfig.Camera.RotationSpeed,

                    PanSpeed = localConfig.Camera.PanSpeed == 0 ?
                            defaultConfig.Camera.PanSpeed : localConfig.Camera.PanSpeed
                },

                Hotkeys = new HotkeyConfig
                {
                    Smoke = localConfig.Hotkeys.Smoke == null || localConfig.Hotkeys.Smoke == "" ?
                            defaultConfig.Hotkeys.Smoke : localConfig.Hotkeys.Smoke,

                    WeaponsOff = localConfig.Hotkeys.WeaponsOff == null || localConfig.Hotkeys.WeaponsOff == "" ?
                            defaultConfig.Hotkeys.WeaponsOff : localConfig.Hotkeys.WeaponsOff,

                    Stop = localConfig.Hotkeys.Stop == null || localConfig.Hotkeys.Stop == "" ?
                            defaultConfig.Hotkeys.Stop : localConfig.Hotkeys.Stop,

                    Unload = localConfig.Hotkeys.Unload == null || localConfig.Hotkeys.Unload == "" ?
                            defaultConfig.Hotkeys.Unload : localConfig.Hotkeys.Unload,

                    Load = localConfig.Hotkeys.Load == null || localConfig.Hotkeys.Load == "" ?
                            defaultConfig.Hotkeys.Load : localConfig.Hotkeys.Load,

                    FirePosition = localConfig.Hotkeys.FirePosition == null || localConfig.Hotkeys.FirePosition == "" ?
                            defaultConfig.Hotkeys.FirePosition : localConfig.Hotkeys.FirePosition,

                    AttackMove = localConfig.Hotkeys.AttackMove == null || localConfig.Hotkeys.AttackMove == "" ?
                            defaultConfig.Hotkeys.AttackMove : localConfig.Hotkeys.AttackMove,

                    ReverseMove = localConfig.Hotkeys.ReverseMove == null || localConfig.Hotkeys.ReverseMove == "" ?
                            defaultConfig.Hotkeys.ReverseMove : localConfig.Hotkeys.ReverseMove,

                    FastMove = localConfig.Hotkeys.FastMove == null || localConfig.Hotkeys.FastMove == "" ?
                            defaultConfig.Hotkeys.FastMove : localConfig.Hotkeys.FastMove,

                    Split = localConfig.Hotkeys.Split == null || localConfig.Hotkeys.Split == "" ?
                            defaultConfig.Hotkeys.Split : localConfig.Hotkeys.Split,

                    VisionTool = localConfig.Hotkeys.VisionTool == null || localConfig.Hotkeys.VisionTool == "" ?
                            defaultConfig.Hotkeys.VisionTool : localConfig.Hotkeys.VisionTool,

                    MenuToggle = localConfig.Hotkeys.MenuToggle == null || localConfig.Hotkeys.MenuToggle == "" ?
                            defaultConfig.Hotkeys.MenuToggle : localConfig.Hotkeys.MenuToggle,

                    UnitInfo = localConfig.Hotkeys.UnitInfo == null || localConfig.Hotkeys.UnitInfo == "" ?
                            defaultConfig.Hotkeys.UnitInfo : localConfig.Hotkeys.UnitInfo,

                    FlareAttack = localConfig.Hotkeys.FlareAttack == null || localConfig.Hotkeys.FlareAttack == "" ?
                            defaultConfig.Hotkeys.FlareAttack : localConfig.Hotkeys.FlareAttack,

                    FlareStop = localConfig.Hotkeys.FlareStop == null || localConfig.Hotkeys.FlareStop == "" ?
                            defaultConfig.Hotkeys.FlareStop : localConfig.Hotkeys.FlareStop,

                    FlareCustom = localConfig.Hotkeys.FlareCustom == null || localConfig.Hotkeys.FlareCustom == "" ?
                            defaultConfig.Hotkeys.FlareCustom : localConfig.Hotkeys.FlareCustom
                }
            };
        }
    }

    public class CameraConfig
    {
        public int ZoomSpeed;  // in meters per sec
        public float RotationSpeed;
        public int PanSpeed;  // in meters per sec
    }

    public class HotkeyConfig
    {
        public string AttackMove;
        public string FastMove;
        public string ReverseMove;
        public string FirePosition;
        public string Split;
        public string VisionTool;
        public string Load;
        public string Unload;
        public string Stop;
        public string Smoke;
        public string WeaponsOff;
        public string MenuToggle;
        public string UnitInfo;
        public string FlareAttack;
        public string FlareStop;
        public string FlareCustom;
    }
}
