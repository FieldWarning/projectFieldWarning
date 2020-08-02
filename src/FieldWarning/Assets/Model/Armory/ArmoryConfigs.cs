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
using Newtonsoft.Json;

/// <summary>
/// The classes here represent purely what we write
/// in our json config for the units/armory.
/// </summary>
namespace PFW.Model.Armory.JsonContents
{
    public class DeckConfig
    {
        public List<string> UnitIds;
    }

    public class UnitConfig
    {
        public List<string> Inherits;
        public string CategoryKey;
        public string Name;
        public int? Price;
        public string PrefabPath;
        public string ArtPrefabPath;
        public string ArmoryImage;
        public string ArmoryBackgroundImage;
        public string MinimapIcon;
        public float? MinimapIconSize;
        public bool? LeavesExplodingWreck;
        public VoiceLineConfig VoiceLineFolders;
        public UnitDataConfig Data;
        public ArmorConfig Armor;
        public MobilityConfig Mobility;
        public List<TurretConfig> Turrets;
        public ReconConfig Recon;

        /// <summary>
        ///     Do consistency checks and final post-parse
        ///     adjustments to the config contents.
        /// </summary>
        public bool ParsingDone(Dictionary<string, UnitConfig> templateConfigs)
        {
            bool result = true;
            if (Turrets != null)
            {
                foreach (TurretConfig turret in Turrets)
                {
                    result &= turret.ParsingDone(Name);
                }
            }

            if (Inherits != null)
            {
                foreach (string templateConfig in Inherits)
                {
                    if (templateConfigs.ContainsKey(templateConfig))
                    {
                        Inherit(templateConfigs[templateConfig]);
                    }
                    else
                    {
                        Logger.LogConfig(
                                LogLevel.ERROR,
                                $"The unit config for {Name} declares inheritance" +
                                $" from {templateConfig}, but no such template config" +
                                $" exists. The unit will be dropped" +
                                $" as a consequence of this error.");
                        result = false;
                    }
                }
            }

            if (!Price.HasValue)
            {
                Logger.LogConfig(
                        LogLevel.ERROR,
                        $"The unit config for {Name} is missing the mandatory field" +
                        $" 'Price' The unit will be dropped" +
                        $" as a consequence of this error.");
                result = false;
            }

            if (!MinimapIconSize.HasValue)
            {
                Logger.LogConfig(
                        LogLevel.ERROR,
                        $"The unit config for {Name} is missing the mandatory field" +
                        $" 'MinimapIconSize' The unit will be dropped" +
                        $" as a consequence of this error.");
                result = false;
            }

            if (!LeavesExplodingWreck.HasValue)
            {
                Logger.LogConfig(
                        LogLevel.ERROR,
                        $"The unit config for {Name} is missing the mandatory field" +
                        $" 'LeavesExplodingWreck' The unit will be dropped" +
                        $" as a consequence of this error.");
                result = false;
            }

            return result;
        }

        private void Inherit(UnitConfig templateConfig)
        {
            if (CategoryKey == null || CategoryKey == "")
                CategoryKey = templateConfig.CategoryKey;
            if (Name == null || Name == "")
                Name = templateConfig.Name;
            if (Price == null)
                Price = templateConfig.Price;
            if (PrefabPath == null || PrefabPath == "")
                PrefabPath = templateConfig.PrefabPath;
            if (ArtPrefabPath == null || ArtPrefabPath == "")
                ArtPrefabPath = templateConfig.ArtPrefabPath;
            if (ArmoryImage == null || ArmoryImage == "")
                ArmoryImage = templateConfig.ArmoryImage;
            if (ArmoryBackgroundImage == null || ArmoryBackgroundImage == "")
                ArmoryBackgroundImage = templateConfig.ArmoryBackgroundImage;
            if (MinimapIcon == null || MinimapIcon == "")
                MinimapIcon = templateConfig.MinimapIcon;
            if (MinimapIconSize == null)
                MinimapIconSize = templateConfig.MinimapIconSize;
            if (LeavesExplodingWreck == null)
                LeavesExplodingWreck = templateConfig.LeavesExplodingWreck;
            if (VoiceLineFolders == null)
                VoiceLineFolders = templateConfig.VoiceLineFolders;
            if (Data == null)
                Data = templateConfig.Data;
            if (Armor == null)
                Armor = templateConfig.Armor;
            if (Mobility == null)
                Mobility = templateConfig.Mobility;
            if (Turrets == null)
                Turrets = templateConfig.Turrets;
            if (Recon == null)
                Recon = templateConfig.Recon;
        }
    }

    public class VoiceLineConfig
    {
        [JsonProperty("Movement")]
        public List<string> Movement;
        [JsonProperty("Aggressive")]
        public List<string> Aggressive;
        [JsonProperty("Selection")]
        public List<string> Selection;
    }

    public class ReconConfig
    {
        public int MaxSpottingRange;
        public float Stealth;
        public float StealthPenetration;
    }

    public class ArmorConfig
    {
        public bool ApImmunity;  // Infantry and flying units can't be shot with KE etc.
        public int FrontArmor;
        public int SideArmor;
        public int RearArmor;
        public int TopArmor;
    }

    public class UnitDataConfig
    {
        public float MovementSpeed;
        public float ReverseSpeed;
        public float AccelRate;
        public float MaxRotationSpeed;
        public float MinTurnRadius;
        public float MaxLateralAccel;
        public float Suspension;
        public float MaxHealth;
        public float Length;
        public float Width;
        public int MobilityTypeIndex;
    }

    public class MobilityConfig
    {
        public float SlopeSensitivity;
        public float DirectionalSlopeSensitivity;
        public float PlainSpeed;
        public float ForestSpeed;
        public float WaterSpeed;
    }

    public class TurretConfig
    {
        public string TurretRef;
        public int ArcHorizontal;
        public int ArcUp;
        public int ArcDown;
        public int RotationRate;

        // Only relevant on child turrets:
        public int Priority;

        // One of these:
        public List<TurretConfig> Children;  // JSONUtility generates a
                                             // warning about the recursion..
        public CannonConfig Cannon;

        public bool ParsingDone(string unitName)
        {
            bool result = true;

            if (Cannon != null)
            {
                result &= Cannon.ParsingDone(unitName);
            }

            if (Children != null)
            {
                foreach (TurretConfig turret in Children)
                {
                    result &= turret.ParsingDone(unitName);
                }
            }

            return result;
        }
    }

    public class CannonConfig
    {
        public string DamageType;
        public int DamageValue;
        // Beware: This is in meters, NOT unity units!
        public int GroundRange;
        public int HeloRange;
        public float Accuracy;
        public float ShotReload;
        public int SalvoLength;
        public float SalvoReload;
        public int Velocity;  // meters per second
        public bool Indirect;
        public bool Guided;
        public string MuzzleFlash;
        public string Shell;
        public string Sound;
        public string BarrelTipRef;
        public List<AmmoConfig> Ammo;

        public bool ParsingDone(string unitName) 
        {
            /// This config has default values for its children ammo
            /// configs. Before use, we check for unset ammo fields
            /// and set them to the default values held here.

            if (Ammo == null || Ammo.Count == 0)
            {
                UnityEngine.Debug.LogError(
                        $"Unit {unitName} has a cannon with no ammo config!");
                return false;
            }

            foreach (AmmoConfig ammo in Ammo)
            {
                if (ammo.DamageType == "" || ammo.DamageType == null)
                    ammo.DamageType = DamageType;
                if (ammo.DamageValue == 0)
                    ammo.DamageValue = DamageValue;
                if (ammo.GroundRange == 0)
                    ammo.GroundRange = GroundRange;
                if (ammo.HeloRange == 0)
                    ammo.HeloRange = HeloRange;
                if (ammo.Accuracy == 0)
                    ammo.Accuracy = Accuracy;
                if (ammo.Velocity == 0)
                    ammo.Velocity = Velocity;
                if (ammo.MuzzleFlash == "" || ammo.MuzzleFlash == null)
                    ammo.MuzzleFlash = MuzzleFlash;
                if (ammo.Shell == "" || ammo.Shell == null)
                    ammo.Shell = Shell;
                if (ammo.Sound == "" || ammo.Sound == null)
                    ammo.Sound = Sound;
            }
            return true;
        }
    }

    /// <summary>
    ///     Copy of the cannon config containing optional overrides
    /// </summary>
    public class AmmoConfig
    {
        public string DamageType;
        public float DamageValue;
        // Beware: This is in meters, NOT unity units!
        public int GroundRange;
        public int HeloRange;
        public float Accuracy;
        public int Velocity;  // meters per second
        public bool Indirect;
        public bool Guided;
        public string MuzzleFlash;
        public string Shell;
        public string Sound;
    }
}
