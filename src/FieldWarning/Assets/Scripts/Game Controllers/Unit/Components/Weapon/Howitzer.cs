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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PFW.Units.Component.Weapon
{
    /// <summary>
    /// Represents a weapon firing in an arc.
    ///
    /// TODO rewrite or hopefully even entirely remove, this should not require
    /// a separate class.
    /// </summary>
    public class Howitzer : IWeapon
    {
        private WeaponData _data { get; }
        private float _reloadTimeLeft { get; set; }
        private AudioSource _audioSource { get; }

        // Where the shell spawns:
        private Transform _shotStarterPosition;

        // TODO Should aim to make actual objects fire and not effects:
        private ParticleSystem _shotEffect;
        private AudioClip _shotSound;
        private float _shotVolume;


        public Howitzer(
            WeaponData data,
            AudioSource source,
            ParticleSystem shotEffect,
            AudioClip shotSound,
            Transform shotStarterPosition,
            float shotVolume = 1.0F)
        {
            _data = data;
            _audioSource = source;
            _shotEffect = shotEffect;
            _shotSound = shotSound;
            _shotVolume = shotVolume;
            _shotStarterPosition = shotStarterPosition;
        }

        private bool Shoot(TargetTuple target)
        {
            //  Vector3 start = new Vector3(ShotStarterPosition.position.x, ShotStarterPosition.position.y+0., ShotStarterPosition.position.z);

            GameObject shell = Resources.Load<GameObject>("shell");
            GameObject shell_new = GameObject.Instantiate(
                    shell,
                    _shotStarterPosition.position,
                    _shotStarterPosition.transform.rotation);

            shell_new.GetComponent<BulletBehavior>().SetUp(_shotStarterPosition, target.Position, 60);

            return true;
        }

        public bool TryShoot(TargetTuple target, float deltaTime)
        {
            _reloadTimeLeft -= deltaTime;
            if (_reloadTimeLeft > 0)
                return false;

            _reloadTimeLeft = _data.ReloadTime;
            return Shoot(target);
        }
    }
}