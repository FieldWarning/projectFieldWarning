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
        private CannonConfig _data { get; }
        private float _reloadTimeLeft { get; set; }
        private AudioSource _audioSource { get; }

        private Transform _barrelTip;

        private readonly AudioClip _shotSound;
        private readonly VisualEffect _muzzleFlashEffect;
        private readonly float _shotVolume;
        private static System.Random _random;

        private readonly GameObject _shellArtPrefab;
        private readonly GameObject _shellPrefab;

        public Cannon(
                CannonConfig data,
                AudioSource source,
                AudioClip shotSound,
                VisualEffect muzzleFlashEffect,
                Transform barrelTip,
                float shotVolume = 1.0f)
        {
            _data = data;
            _audioSource = source;
            _shotSound = shotSound;
            _muzzleFlashEffect = muzzleFlashEffect;
            _shotVolume = shotVolume;
            _barrelTip = barrelTip;
            _random = new System.Random(Environment.TickCount);
            _shellPrefab = Resources.Load<GameObject>("Shell");
            _shellArtPrefab = Resources.Load<GameObject>(_data.Shell);
        }

        private void FireWeapon(
                TargetTuple target,
                Vector3 displacement,
                bool isServer)
        {
            // sound
            _audioSource.PlayOneShot(_shotSound, _shotVolume);

            if (_muzzleFlashEffect != null)
            {
                _muzzleFlashEffect.transform.LookAt(target.Position);
                _muzzleFlashEffect.Play();
            }

            GameObject shell = GameObject.Instantiate(
                    _shellPrefab,
                    _barrelTip.position,
                    _barrelTip.transform.rotation);
            GameObject.Instantiate(_shellArtPrefab, shell.transform);

            shell.GetComponent<ShellBehaviour>().Initialize(
                    target.Position, _data.Velocity);

            if (isServer)
            {
                if (target.IsUnit)
                {
                    float roll = _random.NextFloat(0.0, 100.0);
                    // HIT
                    if (roll <= _data.Accuracy)
                    {
                        Debug.LogWarning("Cannon shell dispersion is not implemented yet");
                        target.Enemy.HandleHit(_data.DamageValue, displacement, null);
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
            _reloadTimeLeft = _data.SalvoReload;
            FireWeapon(target, displacement, isServer);
            return true;
        }
    }
}
