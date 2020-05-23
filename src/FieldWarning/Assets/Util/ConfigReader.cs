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
using UnityEngine;

using PFW.Model.Armory;
using PFW.Model.Armory.JsonContents;
using PFW.Model.Settings;
using PFW.Model.Settings.JsonContents;

namespace PFW
{
	public static class ConfigReader
    {
        public static Deck ParseDeck(string deckName, Armory armory)
        {
            TextAsset configFile = Resources.Load<TextAsset>($"Decks/{deckName}");
            DeckConfig config = JsonUtility.FromJson<DeckConfig>(configFile.text);

            return new Deck(config, armory);
        }

        public static Armory ParseArmory()
        {
            // We take all jsons in the UnitConfigs folder and subfolders
            TextAsset[] configFiles = Resources.LoadAll<TextAsset>("UnitConfigs");
            List<UnitConfig> configs = new List<UnitConfig>();

            foreach (TextAsset configFile in configFiles)
            {
                configs.Add(JsonUtility.FromJson<UnitConfig>(configFile.text));
            }

            return new Armory(configs);
        }

        public static UserSettings ParseSettings()
        {
            TextAsset configFile = Resources.Load<TextAsset>("Settings/DefaultSettings");
            SettingsConfig config = JsonUtility.FromJson<SettingsConfig>(configFile.text);

            // The local settings file overrides anything set in the default
            // settings file. This file may have partial content or not exist
            // at all.
            TextAsset configFile2 = Resources.Load<TextAsset>("Settings/LocalSettings");
            SettingsConfig config2 = configFile2 == null ? 
                config :
                JsonUtility.FromJson<SettingsConfig>(configFile2.text);

            return new UserSettings(config, config2);
        }
    }
}
