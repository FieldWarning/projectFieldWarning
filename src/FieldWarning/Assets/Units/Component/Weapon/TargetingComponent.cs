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

using UnityEngine;

namespace PFW.Units.Component.Weapon
{
    /// <summary>
    /// Manages a single weapon by picking targets for it.
    /// </summary>
    public class TargetingComponent : MonoBehaviour
    {
        public UnitBehaviour Unit { get; private set; }
        private bool _movingTowardsTarget = false;
        private TargetTuple _target;
        public void SetTarget(Vector3 position, bool autoApproach = true)
        {
            SetTarget(new TargetTuple(position), autoApproach);
        }
        private void SetTarget(TargetTuple target, bool autoApproach)
        {
            var distance = Vector3.Distance(Unit.transform.position, target.Position);

            _target = target;

            if (distance > _data.FireRange && autoApproach) {
                _movingTowardsTarget = true;
                Unit.SetDestination(target.Position);
            }

            _turretComponent.SetTarget(_target, _turretPriority);
        }

        private IWeapon _weapon { get; set; }

        // --------------- BEGIN PREFAB ----------------
        [SerializeField]
        private WeaponData _data;

        // TODO the weapon class should create its own audio source:
        [SerializeField]
        private AudioSource _audioSource;

        [SerializeField]
        private TurretComponent _turretComponent;
        /// <summary>
        /// When a unit has multiple weapons that share a turret,
        /// the turret will prefer to rotate for the higher-priority weapon.
        /// </summary>
        [SerializeField]
        private int _turretPriority;
        // Where the shell spawns:
        [SerializeField]
        private Transform _shotStarterPosition;
        // The shell being fired:
        [SerializeField]
        private GameObject _bullet;
        // TODO Should aim to make actual objects fire and not effects:
        [SerializeField]
        private ParticleSystem _shotEffect;
        [SerializeField]
        private AudioClip _shotSound;
        [SerializeField]
        private float _shotVolume = 1.0F;
        // ---------------- END PREFAB -----------------

        private void Awake()
        {
            Unit = gameObject.GetComponent<UnitBehaviour>();
            enabled = false;
        }

        /// <summary>
        /// Initialization order: Awake() when a gameobject is created,
        /// WakeUp() enables the object, Start() runs on an enabled object.
        /// </summary>
        public void WakeUp()
        {
            enabled = true;
        }

        private void Start()
        {
            // TODO remove:
            if (Unit.Platoon.Type == UI.Prototype.UnitType.Tank)
                _weapon = new Cannon(
                        _data, _audioSource, _shotEffect, _shotSound, _shotVolume);
            else if (Unit.Platoon.Type == UI.Prototype.UnitType.Arty)
                _weapon = new Howitzer(
                        _data,
                        _audioSource,
                        _shotEffect,
                        _shotSound,
                        _shotStarterPosition,
                        _shotVolume);
        }

        private void StopMovingIfInRangeOfTarget()
        {
            if (_movingTowardsTarget) {
                if (Vector3.Distance(Unit.transform.position, _target.Position) < _data.FireRange) {
                    _movingTowardsTarget = false;
                    Unit.SetDestination(Unit.transform.position);
                }
            }
        }

        private void Update()
        {
            StopMovingIfInRangeOfTarget();

            if (_target != null && _target.Exists) {

                MaybeDropOutOfRangeTarget();
                bool targetInRange = !_movingTowardsTarget;
                bool shotFired = false;

                if (_turretComponent.IsFacingTarget && targetInRange)
                    shotFired = _weapon.TryShoot(_target, Time.deltaTime);

                // If shooting at the ground, stop after the first shot:
                if (shotFired && _target.IsGround)
                    _target = null;

            } else {
                FindAndTargetClosestEnemy();
            }
        }

        private void FindAndTargetClosestEnemy()
        {
            // TODO utilize precomputed distance lists from session
            // Maybe add Sphere shaped collider with the radius of the range and then use trigger enter and exit to keep a list of in range Units

            foreach (UnitDispatcher enemy in Unit.Platoon.Owner.Session.EnemiesByTeam[Unit.Platoon.Owner.Team]) {

                // See if they are in range of weapon:
                var distance = Vector3.Distance(Unit.transform.position, enemy.Transform.position);
                if (distance < _data.FireRange) {
                    SetTarget(new TargetTuple(enemy.GameObject), false);
                    break;
                }
            }
        }

        /// <summary>
        /// If the target is an enemy unit and it is out of range,
        /// forget about it.
        /// </summary>
        private void MaybeDropOutOfRangeTarget()
        {
            // We only drop unit targets, not positions:
            if (_target.Enemy == null)
                return;

            float distance = Vector3.Distance(Unit.transform.position, _target.Position);
            if (distance > _data.FireRange)
                _target = null;
        }
    }
}

