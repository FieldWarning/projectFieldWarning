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

using UnityEngine;

using PFW.Model.Settings.JsonContents;

namespace PFW.Model.Settings
{
    public class Hotkeys
    {
        public Hotkeys(HotkeyConfig config)
        {
            ApplySettings(config);
        }

        /// <summary>
        /// Recreate the settings from a config, 
        /// storing them in the current instance.
        /// </summary>
        public void ApplySettings(HotkeyConfig config)
        {
            Unload = (KeyCode)System.Enum.Parse(typeof(KeyCode), config.Unload);
            Load = (KeyCode)System.Enum.Parse(typeof(KeyCode), config.Load);
            FirePos = (KeyCode)System.Enum.Parse(typeof(KeyCode), config.FirePosition);
            AttackMove = (KeyCode)System.Enum.Parse(typeof(KeyCode), config.AttackMove);
            ReverseMove = (KeyCode)System.Enum.Parse(typeof(KeyCode), config.ReverseMove);
            FastMove = (KeyCode)System.Enum.Parse(typeof(KeyCode), config.FastMove);
            Split = (KeyCode)System.Enum.Parse(typeof(KeyCode), config.Split);
            VisionTool = (KeyCode)System.Enum.Parse(typeof(KeyCode), config.VisionTool);
            MenuToggle = (KeyCode)System.Enum.Parse(typeof(KeyCode), config.MenuToggle);
            Stop = (KeyCode)System.Enum.Parse(typeof(KeyCode), config.Stop);
            WeaponsOff = (KeyCode)System.Enum.Parse(typeof(KeyCode), config.WeaponsOff);
            Smoke = (KeyCode)System.Enum.Parse(typeof(KeyCode), config.Smoke);
            UnitInfo = (KeyCode)System.Enum.Parse(typeof(KeyCode), config.UnitInfo);
            FlareAttack = (KeyCode)System.Enum.Parse(typeof(KeyCode), config.FlareAttack);
            FlareStop = (KeyCode)System.Enum.Parse(typeof(KeyCode), config.FlareStop);
            FlareCustom = (KeyCode)System.Enum.Parse(typeof(KeyCode), config.FlareCustom);
        }

        public KeyCode Unload;
        public KeyCode Load;
        public KeyCode FirePos;
        public KeyCode AttackMove;
        public KeyCode ReverseMove;
        public KeyCode FastMove;
        public KeyCode Split;
        public KeyCode VisionTool;
        public KeyCode MenuToggle;
        public KeyCode Stop;
        public KeyCode WeaponsOff;
        public KeyCode Smoke;
        public KeyCode UnitInfo;
        public KeyCode FlareAttack;
        public KeyCode FlareStop;
        public KeyCode FlareCustom;
    }
}
