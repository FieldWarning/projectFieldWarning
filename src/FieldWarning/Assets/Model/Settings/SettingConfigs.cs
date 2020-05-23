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

/// <summary>
/// The classes here represent purely what we write
/// in our json config for the user settings.
/// </summary>
namespace PFW.Model.Settings.JsonContents
{
    // Careful: If in the future we add config fields that
    //          allow zero, then we wont be able to distinguish
    //          unset values from values explicitly set to zero 
    //          in the local config.

    [Serializable]
    public class SettingsConfig
    {
        public CameraConfig Camera;
        public HotkeyConfig Hotkeys;
    }

    [Serializable]
    public class CameraConfig
    {
        public int ZoomSpeed;  // in meters per sec
        public float RotationSpeed;
        public int PanSpeed;  // in meters per sec
    }

    [Serializable]
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
    }
}
