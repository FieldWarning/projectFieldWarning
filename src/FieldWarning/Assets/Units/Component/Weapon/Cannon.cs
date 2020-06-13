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
    /// A non-howitzer cannon.
    /// </summary>
    public sealed class Cannon : IWeapon
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
        }

        private void FireWeapon(
                TargetTuple target,
                Vector3 displacement,
                bool isServer)
        {
            Ammo ammo = PickBestAmmo(target, displacement);

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

            shell.GetComponent<ShellBehaviour>().Initialize(
                    target.Position, ammo.Velocity);

            if (isServer)
            {
                if (target.IsUnit)
                {
                    float roll = _random.NextFloat(0.0, 100.0);
                    // HIT
                    if (roll <= ammo.Accuracy)
                    {
                        Debug.LogWarning("Cannon shell dispersion is not implemented yet");
                        target.Enemy.HandleHit(ammo.DamageValue, displacement, null);
                    }
                }
                else
                {
                    // TODO: fire pos damage not implemented
                }
            }
        }

        public void HandleUpdate()
        {
            if (_reloadTimeLeft > 0)
                _reloadTimeLeft -= Time.deltaTime;
        }

        public bool TryShoot(
                TargetTuple target, 
                Vector3 displacement, 
                bool isServer)
        {
            if (_reloadTimeLeft > 0)
                return false;

            // TODO implement salvo + shot reload
            _reloadTimeLeft = _salvoReload;
            FireWeapon(target, displacement, isServer);
            return true;
        }

        private Ammo PickBestAmmo(
                TargetTuple target,
                Vector3 displacement)
        {
            // TODO
            return _ammo[0];
        }

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



    /// <summary>
    ///     The ammo class holds most of the weapon information.
    /// </summary>
    public class Ammo 
    {
        private readonly DamageType _damageType;
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
            if (!Enum.TryParse(config.DamageType.ToUpper(), out _damageType))
            {
                Logger.LogConfig($"Could not parse damage type value '{config.DamageType}'" +
                    $" in a weapon's ammo entry. Defaulting to KE.", LogLevel.ERROR);
            }

            DamageValue = config.DamageValue;
            _groundRange = config.GroundRange * Constants.MAP_SCALE;
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
                    if (_damageType == DamageType.HE)
                        result = _groundRange;
                    break;
                case TargetType.VEHICLE:
                    if (_damageType == DamageType.KE || _damageType == DamageType.HEAT)
                        result = _groundRange;
                    break;
                case TargetType.HELO:
                    if (_damageType == DamageType.HE)
                        result = _heloRange;
                    break;
            }

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
