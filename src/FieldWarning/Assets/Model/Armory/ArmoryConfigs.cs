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
using UnityEngine;

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
        public string CategoryKey;
        public string Name;
        public int? Price;
        public int? Availability;
        public int? ModelCount;
        public int? TransportableSize;
        public int? TransporterCapacity;
        public string PrefabPath;
        public string ArtPrefabPath;
        public string ArmoryImage;
        public string ArmoryBackgroundImage;
        public string LabelIcon;
        public string MinimapIcon;
        public float? MinimapIconSize;

        public bool? LeavesExplodingWreck;
        public bool? CanCaptureZones;
        public VoiceLineConfig VoiceLineFolders;
        public ArmorConfig Armor;
        public MobilityConfig Mobility;
        public List<TurretConfig> Turrets;
        public ReconConfig Recon;

        public float? MovementSpeed;
        public float? ReverseSpeed;
        public float? AccelRate;
        public float? MaxRotationSpeed;
        public float? MinTurnRadius;
        public float? MaxLateralAccel;
        public float? Suspension;
        public float? MaxHealth;
        public float? Length;
        public float? Width;
        public int MobilityTypeIndex;

        /// <summary>
        ///     Do consistency checks and final post-parse
        ///     adjustments to the config contents.
        /// </summary>
        public bool ParsingDone()
        {
            bool result = true;
            if (Turrets != null)
            {
                foreach (TurretConfig turret in Turrets)
                {
                    result &= turret.ParsingDone(Name);
                }
            }

            #region CheckThatValuesAreAllPresent
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

            if (!ModelCount.HasValue)
            {
                Logger.LogConfig(
                        LogLevel.WARNING,
                        $"The unit config for {Name} is missing the mandatory field" +
                        $" 'ModelCount' A default value will be used instead.");
                ModelCount = 1;
            }

            if (!CanCaptureZones.HasValue)
            {
                Logger.LogConfig(
                        LogLevel.ERROR,
                        $"The unit config for {Name} is missing the mandatory field" +
                        $" 'CanCaptureZones' The unit will be dropped" +
                        $" as a consequence of this error.");
                result = false;
            }

            if (!Availability.HasValue)
            {
                Logger.LogConfig(
                        LogLevel.ERROR,
                        $"The unit config for {Name} is missing the mandatory field" +
                        $" 'Availability' The unit will be dropped" +
                        $" as a consequence of this error.");
                result = false;
            }

            if (!TransportableSize.HasValue)
            {
                Logger.LogConfig(
                        LogLevel.WARNING,
                        $"The unit config for {Name} is missing the mandatory field" +
                        $" 'TransportableSize' A default value will be used.");
                TransportableSize = 999;
            }

            if (!TransporterCapacity.HasValue)
            {
                Logger.LogConfig(
                        LogLevel.WARNING,
                        $"The unit config for {Name} is missing the mandatory field" +
                        $" 'TransporterCapacity'. A default value will be used.");
                TransporterCapacity = 0;
            }

            if (!MovementSpeed.HasValue)
            {
                Logger.LogConfig(
                        LogLevel.ERROR,
                        $"The unit config for {Name} is missing the mandatory field" +
                        $" 'MovementSpeed' The unit will be dropped" +
                        $" as a consequence of this error.");
                result = false;
            }

            if (!ReverseSpeed.HasValue)
            {
                Logger.LogConfig(
                        LogLevel.ERROR,
                        $"The unit config for {Name} is missing the mandatory field" +
                        $" 'ReverseSpeed' The unit will be dropped" +
                        $" as a consequence of this error.");
                result = false;
            }

            if (!AccelRate.HasValue)
            {
                Logger.LogConfig(
                        LogLevel.ERROR,
                        $"The unit config for {Name} is missing the mandatory field" +
                        $" 'AccelRate' The unit will be dropped" +
                        $" as a consequence of this error.");
                result = false;
            }

            if (!MaxRotationSpeed.HasValue)
            {
                Logger.LogConfig(
                        LogLevel.ERROR,
                        $"The unit config for {Name} is missing the mandatory field" +
                        $" 'MaxRotationSpeed' The unit will be dropped" +
                        $" as a consequence of this error.");
                result = false;
            }

            if (!MinTurnRadius.HasValue)
            {
                Logger.LogConfig(
                        LogLevel.ERROR,
                        $"The unit config for {Name} is missing the mandatory field" +
                        $" 'MinTurnRadius' The unit will be dropped" +
                        $" as a consequence of this error.");
                result = false;
            }

            if (!MaxLateralAccel.HasValue)
            {
                Logger.LogConfig(
                        LogLevel.ERROR,
                        $"The unit config for {Name} is missing the mandatory field" +
                        $" 'MaxLateralAccel' The unit will be dropped" +
                        $" as a consequence of this error.");
                result = false;
            }

            if (!Suspension.HasValue)
            {
                Logger.LogConfig(
                        LogLevel.ERROR,
                        $"The unit config for {Name} is missing the mandatory field" +
                        $" 'Suspension' The unit will be dropped" +
                        $" as a consequence of this error.");
                result = false;
            }

            if (!MaxHealth.HasValue)
            {
                Logger.LogConfig(
                        LogLevel.ERROR,
                        $"The unit config for {Name} is missing the mandatory field" +
                        $" 'MaxHealth' The unit will be dropped" +
                        $" as a consequence of this error.");
                result = false;
            }

            if (!Length.HasValue)
            {
                Logger.LogConfig(
                        LogLevel.ERROR,
                        $"The unit config for {Name} is missing the mandatory field" +
                        $" 'Length' The unit will be dropped" +
                        $" as a consequence of this error.");
                result = false;
            }

            if (!Width.HasValue)
            {
                Logger.LogConfig(
                        LogLevel.ERROR,
                        $"The unit config for {Name} is missing the mandatory field" +
                        $" 'Width' The unit will be dropped" +
                        $" as a consequence of this error.");
                result = false;
            }
            #endregion

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
            if (LabelIcon == null || LabelIcon == "")
                LabelIcon = templateConfig.LabelIcon;
            if (MinimapIcon == null || MinimapIcon == "")
                MinimapIcon = templateConfig.MinimapIcon;
            if (MinimapIconSize == null)
                MinimapIconSize = templateConfig.MinimapIconSize;
            if (LeavesExplodingWreck == null)
                LeavesExplodingWreck = templateConfig.LeavesExplodingWreck;
            if (VoiceLineFolders == null)
                VoiceLineFolders = templateConfig.VoiceLineFolders;
            if (Armor == null)
                Armor = templateConfig.Armor;
            if (Mobility == null)
                Mobility = templateConfig.Mobility;
            if (Turrets == null)
                Turrets = templateConfig.Turrets;
            if (Recon == null)
                Recon = templateConfig.Recon;
            if (ModelCount == null)
                ModelCount = templateConfig.ModelCount;
            if (CanCaptureZones == null)
                CanCaptureZones = templateConfig.CanCaptureZones;
            if (Availability == null)
                Availability = templateConfig.Availability;
            if (TransportableSize == null)
                TransportableSize = templateConfig.TransportableSize;
            if (TransporterCapacity == null)
                TransporterCapacity = templateConfig.TransporterCapacity;
            if (MovementSpeed == null)
                MovementSpeed = templateConfig.MovementSpeed;
            if (ReverseSpeed == null)
                ReverseSpeed = templateConfig.ReverseSpeed;
            if (AccelRate == null)
                AccelRate = templateConfig.AccelRate;
            if (MaxRotationSpeed == null)
                MaxRotationSpeed = templateConfig.MaxRotationSpeed;
            if (MinTurnRadius == null)
                MinTurnRadius = templateConfig.MinTurnRadius;
            if (MaxLateralAccel == null)
                MaxLateralAccel = templateConfig.MaxLateralAccel;
            if (Suspension == null)
                Suspension = templateConfig.Suspension;
            if (MaxHealth == null)
                MaxHealth = templateConfig.MaxHealth;
            if (Length == null)
                Length = templateConfig.Length;
            if (Width == null)
                Width = templateConfig.Width;
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
        // Short snippet that will show in the UI
        public string Description;
        public string DamageType;
        public float DamageValue;
        // Beware: This is in meters, NOT unity units!
        public int GroundRange;
        public int HeloRange;
        [System.ComponentModel.DefaultValue(1.5f)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public float AimTime;
        public float Accuracy;
        public float ShotReload;
        public int SalvoLength;
        public float SalvoReload;
        public int Velocity;  // meters per second
        public bool IsIndirect;
        public bool IsGuided;
        public string MuzzleFlash;
        public string Shell;
        public string Sound;
        [System.ComponentModel.DefaultValue(10)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int ShellCount;  // If there are multiple ammo types, each will get this many
        public string BarrelTipRef;
        public string WeaponIcon;
        [JsonIgnore]
        public Sprite WeaponSprite;
        public List<AmmoConfig> Ammo;

        public bool ParsingDone(string unitName) 
        {
            /// This config has default values for its children ammo
            /// configs. Before use, we check for unset ammo fields
            /// and set them to the default values held here.

            if (Ammo == null || Ammo.Count == 0)
            {
                Logger.LogConfig(
                        LogLevel.ERROR,
                        $"Unit {unitName} has a cannon with no ammo config!");
                return false;
            }

            WeaponSprite = Util.LoadSpriteFromFile(
                    Application.streamingAssetsPath + "/" + WeaponIcon + ".png");
            if (WeaponSprite == null)
            {
                Logger.LogConfig(
                        LogLevel.WARNING,
                        $"Unit {unitName} has a missing weapon sprite!");
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
                if (ammo.ShellCount == 0)
                    ammo.ShellCount = ShellCount;
                if (ammo.IsIndirect == null)
                    ammo.IsIndirect = IsIndirect;
                if (ammo.IsGuided == null)
                    ammo.IsGuided = IsGuided;
            }
            return true;
        }
    }

    /// <summary>
    ///     Copy of the cannon config containing optional overrides
    /// </summary>
    public class AmmoConfig
    {
        // Short snippet that will show in the UI
        public string Description;
        public string DamageType;
        public float DamageValue;
        // Beware: This is in meters, NOT unity units!
        public int GroundRange;
        public int HeloRange;
        public float Accuracy;
        public int Velocity;  // meters per second
        public bool? IsIndirect;
        public bool? IsGuided;
        public string MuzzleFlash;
        public string Shell;
        public string Sound;
        public int ShellCount;
    }
}
