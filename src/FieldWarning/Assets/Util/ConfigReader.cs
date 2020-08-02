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

using System.Collections.Generic;
using System.IO;
using UnityEngine;

using PFW.Model.Armory;
using PFW.Model.Armory.JsonContents;
using PFW.Model.Settings.JsonContents;
using Newtonsoft.Json;

namespace PFW
{
    public static class ConfigReader
    {
        public static Deck ParseDeck(string deckName, Armory armory)
        {
            TextAsset configFile = Resources.Load<TextAsset>($"Decks/{deckName}");
            DeckConfig config = JsonConvert.DeserializeObject<DeckConfig>(configFile.text);

            return new Deck(config, armory);
        }

        public static Armory ParseArmory()
        {
            // We take all jsons in the UnitConfigs folder and subfolders
            TextAsset[] configFiles = Resources.LoadAll<TextAsset>("UnitConfigs");
            List<UnitConfig> configs = new List<UnitConfig>();

            foreach (TextAsset configFile in configFiles)
            {
                Logger.LogConfig(LogLevel.DEBUG,
                        $"Parsing unit config: {configFile.name}");
                configs.Add(JsonConvert.DeserializeObject<UnitConfig>(configFile.text));
            }

            // Load the unit config templates, which look just like unit configs
            // but don't turn into real units (real units inherit from them).
            string path = Application.dataPath +
                    "/Configuration/Resources/UnitConfigTemplates/";
            string[] templateConfigFiles = new string[0];
            try
            {
                templateConfigFiles = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);
            }
            catch (System.Exception)
            {
                Logger.LogConfig(LogLevel.ERROR, 
                        "Could not access the template unit configs " +
                        "due to a system exception.");
            }

            Dictionary<string, UnitConfig> templateConfigs = 
                new Dictionary<string, UnitConfig>();

            foreach (string configFile in templateConfigFiles)
            {
                // Turn 'C://UnitConfigTemplates/Tank.json' into 'Tank', which
                // is how this will be referred to in the 'Inherits' field
                // of the unit configs
                string shortFileName = configFile.Substring(
                        path.Length, configFile.Length - ".json".Length - path.Length);
                Logger.LogConfig(LogLevel.DEBUG, 
                        $"Parsing unit template: {shortFileName} at {configFile}");

                string configText = File.ReadAllText(configFile);
                templateConfigs.Add(
                        shortFileName, 
                        JsonConvert.DeserializeObject<UnitConfig>(configText));
            }

            return new Armory(configs, templateConfigs);
        }

        public static SettingsConfig ParseDefaultSettingsRaw()
        {
            TextAsset configFile = Resources.Load<TextAsset>("Settings/DefaultSettings");
            SettingsConfig config = 
                    JsonConvert.DeserializeObject<SettingsConfig>(configFile.text);

            return config;
        }

        public static SettingsConfig ParseSettingsRaw()
        {
            SettingsConfig config = ParseDefaultSettingsRaw();

            // The local settings file overrides anything set in the default
            // settings file. This file may have partial content or not exist
            // at all.
            // We need to load it like this and not as an asset, because
            // its contents can be changed during runtime.
            string path = Application.dataPath +
                    "/Configuration/Resources/Settings/LocalSettings.json";
            if (!File.Exists(path))
            {
                return config;
            }
            else
            {
                string localSettingsText = File.ReadAllText(path);
                SettingsConfig config2 =
                        JsonConvert.DeserializeObject<SettingsConfig>(localSettingsText);

                return MergeSettings(config, config2);
            }
        }

        /// <summary>
        /// Values that are set in the local config always
        /// override the values from the default config.
        /// </summary>
        private static SettingsConfig MergeSettings(
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
                            defaultConfig.Hotkeys.MenuToggle : localConfig.Hotkeys.MenuToggle
                }
            };
        }

        public static void WriteLocalConfig(SettingsConfig localConfig)
        {
            // TODO use an asset bundle or Application.persistentDataPath outside editor
            //      The current implementation wont work in the built version!
            string path = Application.dataPath + 
                    "/Configuration/Resources/Settings/LocalSettings.json";

            string contents = JsonConvert.SerializeObject(localConfig, Formatting.Indented);

            // Overwrite the file if it exists
            using (FileStream fs = File.Create(path))
            {
                foreach (char c in contents)
                {
                    fs.WriteByte((byte)c);
                }
            }
        }
    }
}
