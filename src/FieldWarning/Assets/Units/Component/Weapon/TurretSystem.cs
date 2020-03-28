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
using UnityEngine;
using Mirror;

using PFW.Model.Game;
using PFW.Model.Armory;
using PFW.Units.Component.Weapon;
using static PFW.Util;

namespace PFW.Units.Component.Weapon
{
    /// <summary>
    /// Controls a specific part of a unit model and rotates it to face a target.
    /// 
    /// A turret is any rotatable part of a weapon. This includes things like
    /// cannon barrels (vertical laying) and machine gun bodies.
    /// 
    /// A turret can either have a weapon, in which case it rotates based on what 
    /// the weapon is aiming at, or it has child turrets, in which case it 
    /// rotates based on what the child turrets need.
    /// </summary>
    public class Turret
    {
        // targetingStrategy
        private IWeapon _weapon; // weapon turret
        private int _fireRange; // weapon turret
        private TargetTuple _target;  // weapon turret, sync
        private int _priority;  // weapon turret, higher is better
        public List<Turret> Children; // parent turret, sync
        private const float SHOT_VOLUME = 1.0f;

        [SerializeField]
        private bool _isHowitzer = false; // purpose unclear, both

        [SerializeField]
        private Transform _mount = null;  // purpose unclear, both
        [SerializeField]
        private Transform _turret = null; // object being rotated, both

        [SerializeField]
        public float ArcHorizontal = 180, ArcUp = 40, ArcDown = 20, RotationRate = 40f;

        private static GameObject _shotEmitterResource;
        private static GameObject _muzzleFlashResource;
        private static AudioClip _gunSoundResource;

        public Turret(GameObject unit, TurretConfig turretConfig)
        {
            ArcHorizontal = turretConfig.ArcHorizontal;
            ArcUp = turretConfig.ArcUp;
            ArcDown = turretConfig.ArcDown;
            RotationRate = turretConfig.RotationRate;
            _priority = turretConfig.Priority;
            _turret = RecursiveFindChild(unit.transform, turretConfig.MountRef);
            _mount = RecursiveFindChild(unit.transform, turretConfig.TurretRef);

            if (turretConfig.Children.Count > 0)
            {
                Children = new List<Turret>();
                foreach (TurretConfig childTurretConfig in turretConfig.Children)
                {
                    Children.Add(new Turret(unit, childTurretConfig));
                }
            }
            else
            {
                // Hack: The old tank prefab has a particle system for shooting 
                // that we want to remove,
                // so instead of adding it to the models or having it in the config 
                // we hardcode it in here.
                // TODO might have to use a different object for the old arty effect.
                if (!_shotEmitterResource)
                {
                    _shotEmitterResource = Resources.Load<GameObject>("shot_emitter");
                }
                if (!_muzzleFlashResource)
                {
                    _muzzleFlashResource = Resources.Load<GameObject>("muzzle_flash");
                }
                if (!_gunSoundResource)
                {
                    _gunSoundResource = Resources.Load<AudioClip>("Tank_gun");
                }

                GameObject shotGO = GameObject.Instantiate(
                        _shotEmitterResource, _turret);
                AudioSource shotAudioSource = _turret.gameObject.AddComponent<AudioSource>();

                if (turretConfig.Howitzer != null)
                {
                    _isHowitzer = true;
                    _weapon = new Howitzer(
                            turretConfig.Howitzer,
                            shotAudioSource,
                            shotGO.GetComponent<ParticleSystem>(),
                            _gunSoundResource,
                            _turret,
                            SHOT_VOLUME);
                    _fireRange = turretConfig.Howitzer.FireRange;
                }
                else if (turretConfig.Cannon != null)
                {
                    GameObject muzzleFlashGO = GameObject.Instantiate(
                            _muzzleFlashResource, _turret);

                    _weapon = new Cannon(
                            turretConfig.Cannon,
                            shotAudioSource,
                            shotGO.GetComponent<ParticleSystem>(),
                            _gunSoundResource,
                            muzzleFlashGO.GetComponent<ParticleSystem>(),
                            SHOT_VOLUME);
                    _fireRange = turretConfig.Cannon.FireRange;
                }
                else
                {
                    Debug.LogError("Couldn't create a weapon in a turret without children. " +
                            "No weapon specified in the config?");
                }
            }
        }

        public void Update()
        { 

        }
    }

    public class TurretSystem : NetworkBehaviour
    {
        private TargetTuple _primaryTarget;
        public List<Turret> Children; // sync

      //  public UnitDispatcher Unit { get; private set; }
      //  private bool _movingTowardsTarget = false;

        /// <summary>
        /// Constructor for MonoBehaviour
        /// </summary>
        public void Initialize(GameObject unit, Unit armoryUnit)
        {
            Children = new List<Turret>();
            foreach (TurretConfig turretConfig in armoryUnit.Config.Turrets)
            {
                Children.Add(new Turret(unit, turretConfig));
            }
        }

        private void Update()
        {
            
        }
    }
}
