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

namespace PFW.Units
{
    public class ShellBehaviour : MonoBehaviour
    {
        [Header("Explosion you want to appear when shell hits the target or ground")]
        [SerializeField]
        private GameObject _explosionPrefab = null;
        [Header("Trail emitter of this shell prefab - to be disabled on hit")]
        [SerializeField]
        private GameObject _trailEmitter = null;

        private static readonly float GRAVITY = 9.8F * Constants.MAP_SCALE;
        private float _forwardSpeed = 0F;
        private float _verticalSpeed = 0F;
        private Vector3 _targetCoordinates;

        private bool _dead = false;
        private float _prevDistanceToTarget = 100000F;

        /// <summary>
        ///     Call in the weapon class to initialize the shell/bullet.
        /// </summary>
        /// <param name="velocity">In meters.</param>
        public void Initialize(Vector3 target, float velocity)
        {
            _targetCoordinates = target;
            _forwardSpeed = velocity * Constants.MAP_SCALE;
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
        }

        /// <summary>
        /// Returns the barrel angle needed to hit at a given range.
        /// Also returns the implied vertical velocity.
        /// </summary>
        /// A realistic calculation would have a certain max shell speed,
        /// that would be divided between horizontal and vertical.
        /// For simplicity we don't do this and instead assume horizontal always
        /// moves at max speed and vertical can be infinite.
        public static Quaternion CalculateBarrelAngle(
                float horizontalSpeed, 
                Vector3 start, 
                Vector3 target, 
                out float verticalSpeed)
        {
            Vector3 projectileXZPos = new Vector3(start.x, 0.0f, start.z);
            Vector3 targetXZPos = new Vector3(target.x, 0.0f, target.z);

            float horizontalDistanceToTarget = Vector3.Distance(projectileXZPos, targetXZPos);

            // TODO adjust based on height difference between start and target points
            float distanceToHighestPoint = horizontalDistanceToTarget / 2f;
            float timeToHighestPoint = distanceToHighestPoint / horizontalSpeed;
            float gravityEffectToHighestPoint = GRAVITY * timeToHighestPoint;

            verticalSpeed = gravityEffectToHighestPoint;

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

            Vector3 worldForward = transform.TransformDirection(Vector3.forward);
            worldForward = new Vector3(worldForward.x, 0, worldForward.z);
            Vector3 translation = _forwardSpeed * worldForward * Time.deltaTime
                                  + _verticalSpeed * Vector3.up * Time.deltaTime;
            transform.LookAt(transform.position + translation);
            transform.Translate(
                    translation,
                    Space.World);

            _verticalSpeed -= GRAVITY * Time.deltaTime;


            // small trick to detect if shell has reached the target
            float distanceToTarget = Vector3.Distance(transform.position, _targetCoordinates);
            if (distanceToTarget > _prevDistanceToTarget)
            {
                transform.position = _targetCoordinates;
                Explode();
            }
            _prevDistanceToTarget = distanceToTarget;

            //transform.rotation.SetLookRotation(translation);
        }

        private void OnTriggerEnter(Collider other)
        {
            Explode();
        }

        private void Explode()
        {
            _dead = true;
            if (_explosionPrefab != null)
            {
                // instantiate explosion
                GameObject explosion = Instantiate(
                        _explosionPrefab, transform.position, transform.rotation);
                explosion.transform.localScale = new Vector3(10, 10, 10);
                Destroy(explosion, 3F);
            }

            if (_trailEmitter != null)
            {
                //ParticleSystem.EmissionModule emission = _trailEmitter.emission;
                //emission.enabled = false;
            }

            Destroy(gameObject);
        }
    }
}
