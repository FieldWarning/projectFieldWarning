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
using PFW.Model.Armory.JsonContents;

namespace PFW.Units.Component.Weapon
{
    /// <summary>
    /// A non-howitzer cannon.
    /// </summary>
    public sealed class Cannon
    {
        public readonly string Description;
        private readonly float _aimTime;
        private float _aimStartedTimestamp;
        private readonly float _shotReload;
        private readonly int _salvoLength;
        private int _salvoRemaining;
        private readonly float _salvoReload;
        private float _lastShotTimestamp;

        /// <summary>
        /// 1 = aimed, 0.5 = 50% time elapsed until aimed
        /// </summary>
        public float PercentageAimed {
            get {
                return
                    Mathf.Clamp(
                        _salvoRemaining > 0 ?
                                (Time.time - _aimStartedTimestamp) / _aimTime :
                                (Time.time - _aimStartedTimestamp) / _aimTime,
                        0,
                        1);
            }
        }

        /// <summary>
        /// 1 = can shoot, 0.5 = 50% time elapsed to next shot
        /// </summary>
        public float PercentageReloaded { 
                get {
                return 
                    Mathf.Clamp(
                        _salvoRemaining > 0 ?
                                (Time.time - _lastShotTimestamp) / _shotReload :
                                (Time.time - _lastShotTimestamp) / _salvoReload, 
                        0, 
                        1); 
            }
        }

        private bool CanReloadSalvo {
            get {
                return _salvoReload < (Time.time - _lastShotTimestamp);
            }
        }

        private AudioSource _audioSource { get; }

        private Transform _barrelTip;

        private readonly float _shotVolume;
        private static System.Random _random;

        private readonly GameObject _shellPrefab;

        private TargetTuple _lastTarget;

        /// <summary>
        /// If this weapon has HEAT or KE ammo, it will not use HE ammo against vehicles.
        /// 
        /// In unity units.
        /// </summary>
        private readonly float _apRange = 0;

        public Ammo[] Ammo { get; }

        public Sprite HudIcon { get; }

        public Cannon(
                CannonConfig data,
                AudioSource source,
                Transform barrelTip,
                float shotVolume = 1.0f)
        {
            Description = data.Description;
            _aimTime = data.AimTime;
            _shotReload = data.ShotReload;
            _salvoLength = data.SalvoLength;
            _salvoRemaining = _salvoLength;
            _salvoReload = data.SalvoReload;

            _audioSource = source;
            _shotVolume = shotVolume;
            _barrelTip = barrelTip;
            _random = new System.Random(Environment.TickCount);
            _shellPrefab = Resources.Load<GameObject>("Shell");

            Ammo = new Ammo[data.Ammo.Count];
            for (int i = 0; i < data.Ammo.Count; i++)
            {
                Ammo[i] = new Ammo(data.Ammo[i], _barrelTip);

                if (Ammo[i].DamageType == DamageType.KE || Ammo[i].DamageType == DamageType.HEAT)
                {
                    _apRange = _apRange > Ammo[i].GroundRange ? _apRange : Ammo[i].GroundRange;
                }
            }

            HudIcon = data.WeaponSprite;
        }

        private bool FireWeapon(
                TargetTuple target,
                Vector3 displacement,
                float distance,
                bool isServer)
        {
            Ammo ammo = PickBestAmmo(target, displacement, distance);
            if (ammo == null)
                return false;

            ammo.ShellCountRemaining--;

            // sound
            _audioSource.PlayOneShot(ammo.ShotSound, _shotVolume);

            if (ammo.MuzzleFlashEffect != null)
            {
                ammo.MuzzleFlashEffect.transform.LookAt(target.Position);
                ammo.MuzzleFlashEffect.Play();
            }

            GameObject shell = GameObject.Instantiate(
                    _shellPrefab,
                    _barrelTip.position,
                    _barrelTip.transform.rotation);
            GameObject.Instantiate(ammo.ShellArtPrefab, shell.transform);

            float roll = _random.NextFloat(0.0, 100.0);
            bool isHit = roll <= ammo.Accuracy;
            Vector3 shellDestination = target.Position;
            if (!isHit)
            {
                int deviationMode = (int)roll % 4;

                float missFactor = _random.NextFloat(
                        Constants.MISS_FACTOR_MIN,
                        Constants.MISS_FACTOR_MAX);

                float weightX = _random.NextFloat(0, 1);

                switch (deviationMode)
                {
                    case 0:
                        shellDestination.x += distance * missFactor * weightX;
                        shellDestination.y += distance * missFactor * (1 - weightX);
                        break;
                    case 1:
                        shellDestination.x -= distance * missFactor * weightX;
                        shellDestination.y += distance * missFactor * (1 - weightX);
                        break;
                    case 2:
                        shellDestination.x += distance * missFactor * weightX;
                        shellDestination.y -= distance * missFactor * (1 - weightX);
                        break;
                    case 3:
                        shellDestination.x -= distance * missFactor * weightX;
                        shellDestination.y -= distance * missFactor * (1 - weightX);
                        break;
                }
            }


            Vector3 targetVel;
            if (target.IsUnit)
            {
                targetVel = target.Enemy.gameObject.GetComponent<PFW.Units.Component.Movement.MovementComponent>().Velocity;
            }
            else
            {
                targetVel = Vector3.zero;
            }

            ShellBehaviour shellBehaviour = shell.GetComponent<ShellBehaviour>();
            shellBehaviour.Initialize(
                shellDestination, 
                ammo,
                targetVel
                );

            return true;
        }

        /// <summary>
        ///     Fire on the provided target if the weapon is not reloading etc.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="displacement">Vector from the firing unit to the target unit</param>
        /// <param name="distance">Distance from the firing unit to the target unit</param>
        /// <param name="isServer">Non-server code should only affect art.</param>
        /// <returns>True if the last shot of a salvo was just fired.</returns>
        public bool TryShoot(
                TargetTuple target,
                Vector3 displacement,
                float distance,
                bool isServer)
        {
            if (_lastTarget != target)
            {
                _lastTarget = target;
                _aimStartedTimestamp = Time.time;
                return false;
            }

            if (PercentageAimed < 1)
            {
                return false;
            }

            if (PercentageReloaded < 1)
                return false;

            if (CanReloadSalvo)
                _salvoRemaining = _salvoLength;

            if (FireWeapon(target, displacement, distance, isServer))
            {
                _lastShotTimestamp = Time.time;

                _salvoRemaining--;
                return _salvoRemaining == 0;
            }

            return false;
        }

        private Ammo PickBestAmmo(
                TargetTuple target,
                Vector3 displacement,
                float distance)
        {
            Ammo result = null;
            float bestDamage = 0;
            bool mustUseAp = _apRange > distance && target.Type == TargetType.VEHICLE;

            for (int i = 0; i < Ammo.Length; i++)
            {
                if (Ammo[i].ShellCountRemaining == 0)
                    continue;

                float damage = Ammo[i].EstimateDamageAgainstTarget(
                        target, displacement, distance);

                // Tanks and autocannons dont shoot other vehicles with HE:
                if (mustUseAp)
                {
                    if (Ammo[i].DamageType != DamageType.KE && Ammo[i].DamageType != DamageType.HEAT)
                    {
                        damage = 0;
                    }
                }

                if (damage > bestDamage)
                {
                    result = Ammo[i];
                    bestDamage = damage;
                }
            }

            return result;
        }

        /// <summary>
        ///     For every target type, find the max range
        ///     that this weapon can shoot it at (0 if it can't).
        /// </summary>
        private float[] CalculateMaxRanges(
                bool includeDirectFireAmmo,
                bool includeIndirectFireAmmo)
        {
            float[] result = new float[(int)TargetType._SIZE];

            foreach (Ammo ammo in Ammo)
            {
                if (ammo.ShellCountRemaining > 0)
                {
                    if (ammo.IsIndirect && !includeIndirectFireAmmo)
                    {
                        continue;
                    }
                    if (!ammo.IsIndirect && !includeDirectFireAmmo)
                    {
                        continue;
                    }

                    for (int i = 0; i < (int)TargetType._SIZE; i++)
                    {
                        float range = ammo.GetRangeAgainstTargetType((TargetType)i);
                        if (range > result[i])
                            result[i] = range;
                    }
                }
            }

            return result;
        }
        public float[] CalculateMaxRanges() => CalculateMaxRanges(true, true);
        public float[] CalculateMaxRangesDirectFireAmmo() => CalculateMaxRanges(true, false);
        public float[] CalculateMaxRangesIndirectFireAmmo() => CalculateMaxRanges(false, true);
    }
}
