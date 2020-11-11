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
using PFW.Model.Armory.JsonContents;

namespace PFW.Model.Armory
{
    /// <summary>
    /// Decks share unit objects.
    /// </summary>
    //[Serializable]
    public class Unit
    {
        // Identifies which category the unit is in.
        public byte CategoryId;
        // Unique within a category, 
        // should match the index in the unit list.
        public int Id;

        public string Name { get; }
        public int Price { get; }

        public readonly UnitConfig Config;

        //[Tooltip("The gameobject this will be cloned from.")]
        public GameObject Prefab { get; }
        public GameObject ArtPrefab { get; }
        public bool LeavesExplodingWreck { get; }

        public Sprite ArmoryImage { get; }
        public Sprite ArmoryBackgroundImage { get; }

        /// <summary>
        /// If multiple units have the same mobility stats, they share
        /// references to the same mobility data.
        /// 
        /// TODO only stored here so we can create the DataComponent later,
        /// maybe just create the DataComponent earlier and cache it here..
        /// </summary>
        public MobilityData MobilityData { get; }

        public VoiceLines VoiceLines { get; }

        public Unit(UnitConfig config, MobilityData mobility)
        {
            Logger.LogConfig(LogLevel.DEBUG, $"Creating unit object for {config.Name}.");
            MobilityData = mobility;
            Name = config.Name;
            Price = config.Price.Value;
            Prefab = Resources.Load<GameObject>(config.PrefabPath);
            ArtPrefab = Resources.Load<GameObject>(config.ArtPrefabPath);

            LeavesExplodingWreck = config.LeavesExplodingWreck.Value;

            ArmoryImage = Resources.Load<Sprite>(config.ArmoryImage);
            ArmoryBackgroundImage = Resources.Load<Sprite>(config.ArmoryBackgroundImage);
            Config = config;
            VoiceLines = new VoiceLines(config.VoiceLineFolders);
        }
    }

    public class VoiceLines
    {
        public List<AudioClip> aggressiveLines = new List<AudioClip>();
        public List<AudioClip> movementLines = new List<AudioClip>();
        public List<AudioClip> selectionLines = new List<AudioClip>();

        public VoiceLines(VoiceLineConfig voiceLineFolders)
        {
            foreach (string folderName in voiceLineFolders.Aggressive)
            {
                aggressiveLines.AddRange(Resources.LoadAll<AudioClip>(folderName));
            }

            foreach (string folderName in voiceLineFolders.Movement)
            {
                movementLines.AddRange(Resources.LoadAll<AudioClip>(folderName));
            }

            foreach (string folderName in voiceLineFolders.Selection)
            {
                selectionLines.AddRange(Resources.LoadAll<AudioClip>(folderName));
            }
        }
    }
}
