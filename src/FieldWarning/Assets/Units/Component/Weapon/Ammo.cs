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
using UnityEngine;
using UnityEngine.VFX;
using PFW.Model.Armory.JsonContents;

namespace PFW.Units.Component.Weapon
{
    /// <summary>
    ///     The ammo class holds most of the weapon information.
    /// </summary>
    public class Ammo
    {
        public readonly string Description;
        public readonly DamageType DamageType;
        public readonly float DamageValue;
        public readonly float GroundRange;
        private readonly float _heloRange;
        public readonly float Accuracy;
        public readonly float Velocity;  // unity units per second
        public readonly bool IsIndirect;
        private readonly bool _isGuided;
        public readonly AudioClip ShotSound;
        public readonly VisualEffect MuzzleFlashEffect;
        public readonly GameObject ShellArtPrefab;
        public readonly int ShellCount;
        public int ShellCountRemaining;

        public bool IsAoe {
            get {
                if (DamageType == DamageType.HE
                    || DamageType == DamageType.SMALL_ARMS
                    || DamageType == DamageType.HEAVY_ARMS)
                {
                    return true;
                }

                return false;
            }
        }

        public float ExplosionRadius {
            get {
                float result = 0;

                switch (DamageType)
                {
                    case DamageType.KE:
                        break;
                    case DamageType.HE:
                        result = DamageValue * Constants.HE_FALLOFF;
                        break;
                    case DamageType.HEAT:
                        break;
                    case DamageType.SMALL_ARMS:
                        result = DamageValue * Constants.SMALL_ARMS_FALLOFF;
                        break;
                    case DamageType.HEAVY_ARMS:
                        result = DamageValue * Constants.HEAVY_ARMS_FALLOFF;
                        break;
                }

                return result;
            }
        }

        public Ammo(AmmoConfig config, Transform barrelTip)
        {
            if (!Enum.TryParse(config.DamageType.ToUpper(), out DamageType))
            {
                Logger.LogConfig(LogLevel.ERROR,
                    $"Could not parse damage type value '{config.DamageType}'" +
                    $" in a weapon's ammo entry. Defaulting to KE.");
            }

            Description = config.Description;
            DamageValue = config.DamageValue;
            GroundRange = config.GroundRange * Constants.MAP_SCALE;
            if (DamageType == DamageType.KE)
            {
                DamageValue += (int)(GroundRange / Constants.KE_FALLOFF);
            }

            _heloRange = config.HeloRange * Constants.MAP_SCALE;
            Accuracy = config.Accuracy;
            Velocity = config.Velocity * Constants.MAP_SCALE;
            IsIndirect = (bool)config.IsIndirect;
            _isGuided = (bool)config.IsGuided;
            GameObject muzzleFlash = Resources.Load<GameObject>(config.MuzzleFlash);
            if (muzzleFlash != null)
            {
                GameObject muzzleFlashGO = GameObject.Instantiate(
                               muzzleFlash, barrelTip);
                MuzzleFlashEffect = muzzleFlashGO.GetComponent<VisualEffect>();
            }
            else
            {
                Logger.LogConfig(LogLevel.ERROR,
                    $"Could not find the muzzle flash prefab '{config.MuzzleFlash}'" +
                    $" specified in a weapon's ammo entry.");
            }
            ShellArtPrefab = Resources.Load<GameObject>(config.Shell);
            ShotSound = Resources.Load<AudioClip>(config.Sound);
            ShellCount = config.ShellCount;
            ShellCountRemaining = ShellCount;
        }

        /// <summary>
        /// Get the range at which this ammo type can shoot
        /// a given target, or 0 if the target can't be shot at all.
        /// </summary>
        public float GetRangeAgainstTargetType(TargetType targetType)
        {
            float result = 0;

            switch (targetType)
            {
                case TargetType.GROUND:
                case TargetType.INFANTRY:
                    if (DamageType == DamageType.HE 
                        || DamageType == DamageType.SMALL_ARMS
                        || DamageType == DamageType.HEAVY_ARMS)
                        result = GroundRange;
                    break;
                case TargetType.VEHICLE:
                    // TODO this is wrong. Small arms can also shoot at vehicles (<2AV).
                    // If we add small arms here we should be careful to make sure
                    // that the turret system doesn't forever lock onto a vehicle target 
                    // that we can only shoot from behind
                    if (DamageType == DamageType.KE 
                        || DamageType == DamageType.HEAT
                        || DamageType == DamageType.HE)
                        result = GroundRange;
                    break;
                case TargetType.HELO:
                    if (DamageType == DamageType.HE
                        || DamageType == DamageType.SMALL_ARMS
                        || DamageType == DamageType.HEAVY_ARMS)
                        result = _heloRange;
                    break;
            }

            return result;
        }

        /// <summary>
        ///     To pick which ammo type to use we need an estimation
        ///     of the expected damage. This can differ from the actual
        ///     damage dealt if we hit a different armor section,
        ///     if the explosion lands to the side etc..
        /// </summary>
        public float EstimateDamageAgainstTarget(
                TargetTuple target,
                Vector3 displacement,
                float distance)
        {
            float result = 0;

            float range = GetRangeAgainstTargetType(target.Type);
            if (range > distance)
            {
                switch (DamageType)
                {
                    case DamageType.HE:
                        if (target.Type == TargetType.GROUND)
                        {
                            result = DamageValue;
                        }
                        else
                        {
                            // Distance = 0 because we assume the explosion is on the target
                            result = target.Enemy.EstimateDamage(
                                    DamageType, DamageValue, displacement, 0);
                        }
                        break;
                    case DamageType.HEAT:
                        result = target.Enemy.EstimateDamage(
                                DamageType, DamageValue, displacement, distance);
                        break;
                    case DamageType.KE:
                        result = target.Enemy.EstimateDamage(
                                DamageType, DamageValue, displacement, distance);
                        break;
                    case DamageType.SMALL_ARMS:
                        result = target.Enemy.EstimateDamage(
                                DamageType, DamageValue, displacement, distance);
                        break;
                    case DamageType.HEAVY_ARMS:
                        result = target.Enemy.EstimateDamage(
                                DamageType, DamageValue, displacement, distance);
                        break;
                }
            }
            Logger.LogDamage(
                    LogLevel.DUMP,
                    $"Estimating {result} damage with dmg type {DamageType}," +
                    $" firepower {DamageValue} at range {distance / Constants.MAP_SCALE}");

            return result;
        }

        public string RangeForUI() 
        {
            return GroundRange / Constants.MAP_SCALE + "m |" + _heloRange / Constants.MAP_SCALE +"m";
        }
    }

    public enum DamageType
    {
        KE = 0,
        HE,
        HEAT,
        /// <summary>
        /// Like HE, but does trivial dmg vs 1AV and no dmg vs 2AV.
        /// </summary>
        SMALL_ARMS,
        /// <summary>
        /// Larger small arms, e.g. .50 cals; smaller penalty vs 1AV targets.
        /// </summary>
        HEAVY_ARMS,
        _SIZE
    }
}
