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
    /// A non-howitzer cannon.
    /// </summary>
    public class Cannon : IWeapon
    {
        private WeaponData _data { get; }
        private float _reloadTimeLeft { get; set; }
        private AudioSource _source { get; }
        private TargetTuple _target;
        
        // TODO Should aim to make actual objects fire and not effects:
        private ParticleSystem _shotEffect;
        private AudioClip _shotSound;
        private float _shotVolume = 1.0F;

        public Cannon(
            WeaponData data, 
            AudioSource source, 
            ParticleSystem shotEffect,
            AudioClip shotSound, 
            float shotVolume = 1.0F)
        {
            _data = data;
            _source = source;
            _shotEffect = shotEffect;
            _shotSound = shotSound;
            _shotVolume = shotVolume;
        }

        private bool FireWeapon(TargetTuple target)
        {
            // sound
            _source.PlayOneShot(_shotSound, _shotVolume);
            // particle
            _shotEffect.Play();

            if (target.IsUnit) {
                System.Random rnd = new System.Random();
                int roll = rnd.Next(1, 100);

                // HIT
                if (roll < _data.Accuracy) {
                    target.Enemy.GetComponent<UnitBehaviour>().HandleHit(_data.Damage);
                    return true;
                }
            } else {
                // ensure we only fire pos once
                this._target = null;
            }

            // MISS
            return false;

            //if (Unit.Platoon.Type == Ingame.Prototype.UnitType.Arty) {
            //    //  Vector3 start = new Vector3(ShotStarterPosition.position.x, ShotStarterPosition.position.y+0., ShotStarterPosition.position.z);

            //    GameObject shell = Resources.Load<GameObject>("shell");
            //    GameObject shell_new = Instantiate(shell, _shotStarterPosition.position, _shotStarterPosition.transform.rotation);
            //    shell_new.GetComponent<BulletBehavior>().SetUp(_shotStarterPosition, target.Position, 60);

            //    return true;
            //}

            return false;
        }

        public bool TryShoot(TargetTuple target, float deltaTime)
        {
            _reloadTimeLeft -= deltaTime;
            if (_reloadTimeLeft > 0)
                return false;

            _reloadTimeLeft = _data.ReloadTime;
            return FireWeapon(target);
        }
    }
}