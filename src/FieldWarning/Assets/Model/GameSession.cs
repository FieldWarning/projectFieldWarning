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

using PFW.Model.Armory;
using PFW.Model.Armory.JsonContents;
using PFW.Model.Settings;
using PFW.Model.Settings.JsonContents;
using System.Collections.Generic;

namespace PFW.Model
{
    /// <summary>
    /// Singleton that is created at game launch.
    /// This represents the application session,
    /// NOT individual matches (contrast: MatchSession).
    /// </summary>
    public class GameSession
    {
        // TODO: C# will only initialize static members when they
        //       are accessed, so we might need an init script..
        public static GameSession Singleton = new GameSession();

        public readonly UserSettings Settings;

        /// <summary>
        ///  Cached for use by the settings menu.
        /// </summary>
        public SettingsConfig SettingsRaw;

        public readonly Armory.Armory Armory;

        public readonly Dictionary<string, DeckConfig> DecksRaw;
        public readonly Dictionary<string, Deck> Decks;

        private GameSession()
        {
            SettingsRaw = ConfigReader.ParseSettingsRaw();
            Settings = new UserSettings(SettingsRaw);
            Armory = ConfigReader.ParseArmory();
            DecksRaw = ConfigReader.ParseDecksRaw();
            Decks = new Dictionary<string, Deck>();
            foreach (KeyValuePair<string, DeckConfig> kv in DecksRaw)
            {
                Decks.Add(kv.Key, new Deck(kv.Value, Armory));
            }
        }

        public void ReloadSettings()
        { 
            SettingsRaw = ConfigReader.ParseSettingsRaw();
            Settings.ApplyLocalSettings(SettingsRaw);
        }
    }
}
