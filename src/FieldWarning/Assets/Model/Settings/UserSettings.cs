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

        public UserSettings(SettingsConfig config)
        {
            Hotkeys = new Hotkeys(config.Hotkeys);
            CameraSettings = new CameraSettings(config.Camera);
        }

        /// <summary>
        /// Re-reads the settings after a config change,
        /// storing the values into the existing settings objects.
        /// </summary>
        public void ApplyLocalSettings(SettingsConfig config)
        {
            Hotkeys.ApplySettings(config.Hotkeys);
            CameraSettings.ApplySettings(config.Camera);
        }
    }

    public class CameraSettings
    {
        public CameraSettings(CameraConfig config)
        {
            ApplySettings(config);
        }

        /// <summary>
        /// Recreate the settings from a config, 
        /// storing them in the current instance.
        /// </summary>
        public void ApplySettings(CameraConfig config)
        {
            ZoomSpeed = config.ZoomSpeed * Constants.MAP_SCALE;
            RotationSpeed = config.RotationSpeed;
            PanSpeed = config.PanSpeed * Constants.MAP_SCALE;
        }

        public float ZoomSpeed;  // in unity units
        public float RotationSpeed;
        public float PanSpeed;  // in unity units
    }
}
