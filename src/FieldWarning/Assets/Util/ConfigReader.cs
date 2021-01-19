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

        private static Dictionary<string, T> ParseAllJsonFiles<T>(
                string directory)
        {
            string[] configFiles = new string[0];
            try
            {
                configFiles = Directory.GetFiles(directory, "*.json", SearchOption.AllDirectories);
            }
            catch (System.Exception)
            {
                Logger.LogConfig(LogLevel.ERROR,
                        $"Could not access {directory} " +
                        "due to a system exception.");
            }

            var result = new Dictionary<string, T>();
            
            foreach (string configFile in configFiles)
            {
                // Turn 'C://UnitConfigTemplates/Tank.json' into 'Tank', which
                // is how this will be referred to in the 'Inherits' field
                // of the unit configs (or in the decks config files)
                string shortFileName = configFile.Substring(
                        directory.Length, configFile.Length - ".json".Length - directory.Length);
                shortFileName = shortFileName.Replace('\\', '/');  // Unix directory separators
                Logger.LogConfig(LogLevel.DEBUG,
                        $"Parsing config file: {shortFileName} at {configFile}");

                string configText = File.ReadAllText(configFile);
                result.Add(
                        shortFileName,
                        JsonConvert.DeserializeObject<T>(configText));
            }

            return result;
        }

        public static Armory ParseArmory()
        {
            // We take all jsons in the UnitConfigs folder and subfolders
            string unitsPath = Application.streamingAssetsPath +
                    "/UnitConfigs/";

            Logger.LogConfig(LogLevel.INFO, "Parsing unit configs.");
            Dictionary<string, UnitConfig> configs = ParseAllJsonFiles<UnitConfig>(
                    unitsPath);

            // Load the unit config templates, which look just like unit configs
            // but don't turn into real units (real units inherit from them).
            string templatesPath = Application.streamingAssetsPath +
                    "/UnitConfigTemplates/";

            Logger.LogConfig(LogLevel.INFO, "Parsing unit template configs.");
            Dictionary<string, UnitConfig> templateConfigs = ParseAllJsonFiles<UnitConfig>(
                    templatesPath);

            return new Armory(configs, templateConfigs);
        }

        public static SettingsConfig ParseDefaultSettingsRaw()
        {
            string path = Application.streamingAssetsPath +
                    "/Settings/DefaultSettings.json";
            if (!File.Exists(path))
            {
                Logger.LogConfig(
                        LogLevel.ERROR,
                        $"Corrupted installation: Default settings file not found at {path}");
                return null;
            }
            else
            {
                string configText = File.ReadAllText(path);
                SettingsConfig config =
                    JsonConvert.DeserializeObject<SettingsConfig>(configText);

                return config;
            }
        }

        public static SettingsConfig ParseSettingsRaw()
        {
            SettingsConfig config = ParseDefaultSettingsRaw();

            // The local settings file overrides anything set in the default
            // settings file. This file may have partial content or not exist
            // at all.
            // We need to load it like this and not as an asset, because
            // its contents can be changed during runtime.
            string path = Application.streamingAssetsPath +
                    "/Settings/LocalSettings.json";
            if (!File.Exists(path))
            {
                return config;
            }
            else
            {
                string localSettingsText = File.ReadAllText(path);
                SettingsConfig config2 =
                        JsonConvert.DeserializeObject<SettingsConfig>(localSettingsText);

                return SettingsConfig.MergeSettings(config, config2);
            }
        }

        public static void WriteLocalConfig(SettingsConfig localConfig)
        {
            string path = Application.streamingAssetsPath + 
                    "/Settings/LocalSettings.json";

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

        public static Dictionary<string, DeckConfig> ParseDecksRaw() 
        {
            string templatesPath = Application.streamingAssetsPath +
                    "/Decks/";

            Logger.LogConfig(LogLevel.INFO, "Parsing local decks.");
            return ParseAllJsonFiles<DeckConfig>(templatesPath); ;
        }
    }
}
