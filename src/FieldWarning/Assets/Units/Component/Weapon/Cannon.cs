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
        private float _shotReload;
        private int _salvoLength;
        private float _salvoReload;

        private float _reloadTimeLeft { get; set; }
        private AudioSource _audioSource { get; }

        private Transform _barrelTip;

        private readonly float _shotVolume;
        private static System.Random _random;

        private readonly GameObject _shellPrefab;

        private Ammo[] _ammo;

        private Sprite _hudIcon;

        public Cannon(
                CannonConfig data,
                AudioSource source,
                Transform barrelTip,
                float shotVolume = 1.0f)
        {
            _shotReload = data.ShotReload;
            _salvoLength = data.SalvoLength;
            _salvoReload = data.SalvoReload;

            _audioSource = source;
            _shotVolume = shotVolume;
            _barrelTip = barrelTip;
            _random = new System.Random(Environment.TickCount);
            _shellPrefab = Resources.Load<GameObject>("Shell");

            _ammo = new Ammo[data.Ammo.Count];
            for (int i = 0; i < data.Ammo.Count; i++)
            {
                _ammo[i] = new Ammo(data.Ammo[i], _barrelTip);
            }

            _hudIcon = data.WeaponSprite;
        }

        private void FireWeapon(
                TargetTuple target,
                Vector3 displacement,
                float distance,
                bool isServer)
        {
            Ammo ammo = PickBestAmmo(target, displacement, distance);

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

            ShellBehaviour shellBehaviour = shell.GetComponent<ShellBehaviour>();
            shellBehaviour.Initialize(shellDestination, ammo);

            if (isServer)
            {
                if (target.IsUnit)
                {
                    if (isHit && !ammo.IsAoe)
                    {
                        target.Enemy.HandleHit(
                                ammo.DamageType, ammo.DamageValue, displacement, distance);
                    }
                }
                else
                {
                    // HE damage is applied by the shellBehavior when it explodes
                }
            }
        }

        /// <summary>
        ///     Same as the update method on a MonoBehavior.
        ///     Used, for example, to update reload timers.
        /// </summary>
        public void HandleUpdate()
        {
            if (_reloadTimeLeft > 0)
                _reloadTimeLeft -= Time.deltaTime;
        }

        /// <summary>
        ///     Fire on the provided target if the weapon is not reloading etc.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="displacement">Vector from the firing unit to the target unit</param>
        /// <param name="distance">Distance from the firing unit to the target unit</param>
        /// <param name="isServer">Non-server code should only affect art.</param>
        /// <returns>True if a shot was fired, false otherwise.</returns>
        public bool TryShoot(
                TargetTuple target,
                Vector3 displacement,
                float distance,
                bool isServer)
        {
            if (_reloadTimeLeft > 0)
                return false;

            // TODO implement salvo + shot reload
            _reloadTimeLeft = _salvoReload;
            FireWeapon(target, displacement, distance, isServer);
            return true;
        }

        private Ammo PickBestAmmo(
                TargetTuple target,
                Vector3 displacement,
                float distance)
        {
            Ammo result = _ammo[0];
            float bestDamage = result.EstimateDamageAgainstTarget(
                        target, displacement, distance);

            for (int i = 1; i < _ammo.Length; i++)
            {
                float damage = _ammo[i].EstimateDamageAgainstTarget(
                        target, displacement, distance);
                if (damage > bestDamage)
                {
                    result = _ammo[i];
                    bestDamage = damage;
                }
            }

            return result;
        }

        /// <summary>
        ///     For every target type, find the max range
        ///     that this weapon can shoot it at (0 if it can't).
        /// </summary>
        public float[] CalculateMaxRanges()
        {
            float[] result = new float[(int)TargetType._SIZE];

            foreach (Ammo ammo in _ammo)
            {
                for (int i = 0; i < (int)TargetType._SIZE; i++)
                {
                    float range = ammo.GetRangeAgainstTargetType((TargetType)i);
                    if (range > result[i])
                        result[i] = range;
                }
            }

            return result;
        }
    }
}
