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
        public Hotkeys(HotkeyConfig defaultConfig, HotkeyConfig localConfig)
        {
            Unload = localConfig.Unload == null ?
                (KeyCode)System.Enum.Parse(typeof(KeyCode), defaultConfig.Unload) :
                (KeyCode)System.Enum.Parse(typeof(KeyCode), localConfig.Unload);

            Load = localConfig.Unload == null ?
                (KeyCode)System.Enum.Parse(typeof(KeyCode), defaultConfig.Load) :
                (KeyCode)System.Enum.Parse(typeof(KeyCode), localConfig.Load);

            FirePos = localConfig.Unload == null ?
                (KeyCode)System.Enum.Parse(typeof(KeyCode), defaultConfig.FirePosition) :
                (KeyCode)System.Enum.Parse(typeof(KeyCode), localConfig.FirePosition);

            ReverseMove = localConfig.Unload == null ?
                (KeyCode)System.Enum.Parse(typeof(KeyCode), defaultConfig.ReverseMove) :
                (KeyCode)System.Enum.Parse(typeof(KeyCode), localConfig.ReverseMove);

            FastMove = localConfig.Unload == null ?
                (KeyCode)System.Enum.Parse(typeof(KeyCode), defaultConfig.FastMove) :
                (KeyCode)System.Enum.Parse(typeof(KeyCode), localConfig.FastMove);

            Split = localConfig.Unload == null ?
                (KeyCode)System.Enum.Parse(typeof(KeyCode), defaultConfig.Split) :
                (KeyCode)System.Enum.Parse(typeof(KeyCode), localConfig.Split);

            VisionTool = localConfig.Unload == null ?
                (KeyCode)System.Enum.Parse(typeof(KeyCode), defaultConfig.VisionTool) :
                (KeyCode)System.Enum.Parse(typeof(KeyCode), localConfig.VisionTool);
        }

        public KeyCode Unload = KeyCode.U;
        public KeyCode Load = KeyCode.L;
        public KeyCode FirePos = KeyCode.T;
        public KeyCode ReverseMove = KeyCode.G;
        public KeyCode FastMove = KeyCode.V;
        public KeyCode Split = KeyCode.K;
        public KeyCode VisionTool = KeyCode.R;
    }
}
