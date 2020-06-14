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
        public readonly DamageType DamageType;
        public readonly int DamageValue;
        private readonly float _groundRange;
        private readonly float _heloRange;
        public readonly float Accuracy;
        public readonly float Velocity;  // unity units per second
        private readonly bool _isIndirect;
        private readonly bool _isGuided;
        public readonly AudioClip ShotSound;
        public readonly VisualEffect MuzzleFlashEffect;
        public readonly GameObject ShellArtPrefab;

        public Ammo(AmmoConfig config, Transform barrelTip)
        {
            if (!Enum.TryParse(config.DamageType.ToUpper(), out DamageType))
            {
                Logger.LogConfig($"Could not parse damage type value '{config.DamageType}'" +
                    $" in a weapon's ammo entry. Defaulting to KE.", LogLevel.ERROR);
            }
            
            DamageValue = config.DamageValue;
            _groundRange = config.GroundRange * Constants.MAP_SCALE;
            if (DamageType == DamageType.KE)
            {
                DamageValue += (int)(_groundRange / Constants.KE_FALLOFF);
            }

            _heloRange = config.HeloRange * Constants.MAP_SCALE;
            Accuracy = config.Accuracy;
            Velocity = config.Velocity * Constants.MAP_SCALE;
            _isIndirect = config.Indirect;
            _isGuided = config.Guided;
            GameObject muzzleFlash = Resources.Load<GameObject>(config.MuzzleFlash);
            if (muzzleFlash != null)
            {
                GameObject muzzleFlashGO = GameObject.Instantiate(
                               muzzleFlash, barrelTip);
                MuzzleFlashEffect = muzzleFlashGO.GetComponent<VisualEffect>();
            }
            else
            {
                Logger.LogConfig($"Could not find the muzzle flash prefab '{config.MuzzleFlash}'" +
                    $" specified in a weapon's ammo entry.", LogLevel.ERROR);
            }
            ShellArtPrefab = Resources.Load<GameObject>(config.Shell);
            ShotSound = Resources.Load<AudioClip>(config.Sound);
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
                    if (DamageType == DamageType.HE)
                        result = _groundRange;
                    break;
                case TargetType.VEHICLE:
                    if (DamageType == DamageType.KE || DamageType == DamageType.HEAT)
                        result = _groundRange;
                    break;
                case TargetType.HELO:
                    if (DamageType == DamageType.HE)
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
            if (distance <= range)
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
                }
            }
            Logger.LogDamage(
                    $"Estimating {result} damage with dmg type {DamageType}," +
                    $" firepower {DamageValue} at range {distance}", 
                    LogLevel.DUMP);

            return result;
        }
    }

    public enum DamageType
    {
        KE = 0,
        HE,
        HEAT,
        _SIZE
    }
}
