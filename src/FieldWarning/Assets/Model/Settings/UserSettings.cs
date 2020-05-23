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
 */

using System;

using PFW.Model.Settings.JsonContents;

/// <summary>
/// The classes here are the parsed and
/// interpreted user settings (Contrast: SettingConfigs.cs)
/// </summary>
namespace PFW.Model.Settings
{
    public class UserSettings
    {
        public readonly Hotkeys Hotkeys;
        public readonly CameraSettings CameraSettings;

        public UserSettings(SettingsConfig defaultConfig, SettingsConfig localConfig)
        {
            Hotkeys = new Hotkeys(defaultConfig.Hotkeys, localConfig.Hotkeys);
            CameraSettings = new CameraSettings(defaultConfig.Camera, localConfig.Camera);
        }
    }

    public class CameraSettings
    {
        public CameraSettings(CameraConfig defaultConfig, CameraConfig localConfig)
        {
            ZoomSpeed = localConfig.ZoomSpeed == 0 ? 
                    defaultConfig.ZoomSpeed : localConfig.ZoomSpeed;
            ZoomSpeed *= Constants.MAP_SCALE;

            RotationSpeed = localConfig.RotationSpeed == 0 ?
                    defaultConfig.RotationSpeed : localConfig.RotationSpeed;

            PanSpeed = localConfig.PanSpeed == 0 ?
                    defaultConfig.PanSpeed : localConfig.PanSpeed;
            PanSpeed *= Constants.MAP_SCALE;
        }

        public float ZoomSpeed;  // in unity units
        public float RotationSpeed;
        public float PanSpeed;  // in unity units
    }
}
