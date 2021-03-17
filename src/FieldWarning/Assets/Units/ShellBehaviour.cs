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
using System.Collections.Generic;
using UnityEngine;

using PFW.Model.Match;
using PFW.UI.Ingame;

namespace PFW.Units.Component.Weapon
{
    public class ShellBehaviour : MonoBehaviour
    {
        [Header("Explosion you want to appear when shell hits the target or ground")]
        [SerializeField]
        private GameObject _explosionPrefab = null;
        [Header("Trail emitter of this shell prefab - to be disabled on hit")]
        [SerializeField]
        private GameObject _trailEmitter = null;
        [SerializeField]
        private Vector3 _explosionSize = new Vector3(1, 1, 1);
        [SerializeField]
        private float _explosionTimeout = 3F;

        private static readonly float GRAVITY = 9.8F * Constants.MAP_SCALE;
        private float _forwardSpeed => _ammo.Velocity;
        private float _verticalSpeed = 0F;
        private Vector3 _targetCoordinates;
        private Vector3 _targetVelocity;
        //targetVelocity is the instantaneous target velocity when this shell is fired.

        private Vector3 _worldForward;

        private bool _dead = false;
        private float _prevDistanceToTarget = 100000F;
        private float _initialDistanceToTarget;

        private Ammo _ammo;

        /// <summary>
        ///     Call in the weapon class to initialize the shell/bullet.
        /// </summary>
        /// <param name="velocity">In meters.</param>
        public void Initialize(Vector3 target, Ammo ammo, Vector3 targetVelocity)
        {
            _targetCoordinates = target;
            _ammo = ammo;
            _targetVelocity = targetVelocity;
            _initialDistanceToTarget = (_targetCoordinates - transform.position).magnitude;
        }

        private void Start()
        {
            Launch();
        }

        public void Launch()
        {
            Quaternion angle = CalculateBarrelAngle(
                    _forwardSpeed, 
                    transform.position, 
                    _targetCoordinates, 
                    out _verticalSpeed);

            // rotate the object to face the target
            transform.LookAt(_targetCoordinates);
            transform.rotation *= angle;

            _worldForward = (_targetCoordinates - transform.position);
            _worldForward.y = 0;
            _worldForward = _worldForward.normalized;
        }

        /// <summary>
        /// Returns the barrel angle needed to hit at a given range.
        /// Also returns the implied vertical velocity.
        /// </summary>
        /// A realistic calculation would have a certain max shell speed,
        /// that would be divided between horizontal and vertical.
        /// For simplicity we don't do this and instead assume horizontal always
        /// moves at max speed and vertical can be infinite.
        /// TODO remove the simplification
        /// TODO take IsIndirect as an argument, have indirect shells use a steeper angle
        public static Quaternion CalculateBarrelAngle(
                float horizontalSpeed, 
                Vector3 start, 
                Vector3 target, 
                out float verticalSpeed)
        {
            Vector3 projectileXZPos = new Vector3(start.x, 0.0f, start.z);
            Vector3 targetXZPos = new Vector3(target.x, 0.0f, target.z);

            float horizontalDistanceToTarget = Vector3.Distance(projectileXZPos, targetXZPos);
            float verticalDistanceToTarget = target.y - start.y;

            // TODO adjust based on height difference between start and target points
            float distanceToHighestPoint = horizontalDistanceToTarget / 2f;
            float timeToHighestPoint = distanceToHighestPoint / horizontalSpeed;
            float gravityEffectToHighestPoint = GRAVITY * timeToHighestPoint;

            verticalSpeed = gravityEffectToHighestPoint;
            verticalSpeed = verticalSpeed + GRAVITY * verticalDistanceToTarget / (2f * verticalSpeed);

            // The vectors for the horizontal speed, vertical speed and 
            // the direction of the barrel form a triangle.
            // The angle can then be calculated with the formula
            // tan(angle) = verticalSpeed / horizontalSpeed
            return Quaternion.AngleAxis(
                    Mathf.Rad2Deg * Mathf.Atan(verticalSpeed / horizontalSpeed),
                    Vector3.left);
        }

        private void Update()
        {
            if (_dead)
            {
                return;
            }

            //Vector3 worldForward = transform.TransformDirection(Vector3.forward);
            //worldForward = new Vector3(worldForward.x, 0, worldForward.z);
            Vector3 translation = _forwardSpeed * _worldForward * Time.deltaTime
                                  + _verticalSpeed * Vector3.up * Time.deltaTime
                                  + _targetVelocity * Time.deltaTime; /// * Constants.MAP_SCALE;
            transform.LookAt(transform.position + translation);
            transform.Translate(
                    translation,
                    Space.World);

            _verticalSpeed -= GRAVITY * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
 
            UnitDispatcher target = other.gameObject.GetComponentInParent<UnitDispatcher>();

            if (other.GetComponent<CaptureZone>() == null)
            {
                if (target != null && !_ammo.IsAoe)
                {
                    KineticHit(target);
                }
                else if (_ammo.IsAoe)
                {
                    Explode();
                }
                else
                {
                    ///_verticalSpeed = -_verticalSpeed; riccochet logic here?
                }
            }
            
        }

        private void KineticHit(UnitDispatcher unit)
        {
            _dead = true;

            unit.HandleHit(
                    _ammo.DamageType,
                    _ammo.DamageValue,
                    transform.TransformDirection(Vector3.forward),
                    _initialDistanceToTarget);

            Destroy(gameObject);
        }

        private void Explode()
        {
            _dead = true;
            if (_explosionPrefab != null)
            {
                // instantiate explosion
                GameObject explosion = Instantiate(
                        _explosionPrefab, transform.position, transform.rotation);
                explosion.transform.localScale = _explosionSize;
                Destroy(explosion, _explosionTimeout);
            }

            if (_trailEmitter != null)
            {
                //ParticleSystem.EmissionModule emission = _trailEmitter.emission;
                //emission.enabled = false;
            }

            List<UnitDispatcher> units = 
                    MatchSession.Current.FindUnitsAroundPoint(
                            transform.position, _ammo.ExplosionRadius);

            foreach (UnitDispatcher unit in units)
            {
                Vector3 vectorToTarget = unit.transform.position - transform.position;
                unit.HandleHit(
                        _ammo.DamageType, 
                        _ammo.DamageValue, 
                        vectorToTarget, 
                        vectorToTarget.magnitude);
            }

            Destroy(gameObject);
        }
    }
}
