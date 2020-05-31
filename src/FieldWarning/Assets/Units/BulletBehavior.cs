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
    public class BulletBehavior : MonoBehaviour
    {
        public BulletData Bullet; // contains attributes for the shell
        [Header("Explosion you want to appear when shell hits the target or ground")]
        [SerializeField]
        private GameObject _explosionPrefab = null;
        [Header("Trail emitter of this shell prefab - to be disabled on hit")]
        [SerializeField]
        private GameObject _trailEmitter = null;

        private Rigidbody _rigid;

        private float _launchAngle;
        private Transform _startingTransform;
        private Vector3 _targetCoordinates;
        
        // called by the weapon behaviour to set stats of the shell
        public void SetUp(Transform position, Vector3 target, float launchAngle)
        {
            _startingTransform = position;
            _targetCoordinates = target;
            _launchAngle = launchAngle;
        }

        private void Start()
        {
            Bullet = new BulletData();
            _rigid = GetComponent<Rigidbody>();

            _rigid.useGravity = false;
            _rigid.isKinematic = true; // means that rigidbody is moved by script and does not affected by physics engine

            Launch();
        }

        private float Gravity = 9.8F * Constants.MAP_SCALE;
        private float ForwardSpeed = 0F;
        private float VerticalSpeed = 0F;

        public void Launch()
        {
            Vector3 projectileXZPos = new Vector3(transform.position.x, 0.0f, transform.position.z);
            Vector3 targetXZPos = new Vector3(_targetCoordinates.x, 0.0f, _targetCoordinates.z);

            // rotate the object to face the target
            transform.LookAt(targetXZPos);

            // formula
            float distanceToTarget = Vector3.Distance(projectileXZPos, targetXZPos);

            // float tanAlpha = Mathf.Tan(_launchAngle * Mathf.Deg2Rad);
            // float heading = _targetCoordinates.y - transform.position.y;

            // calculate the local space components of the velocity
            // required to land the projectile on the target object
            //float Vz = Mathf.Sqrt(G * R * R / (2.0f * (H - R * tanAlpha)));

            float Vz = 2F;

            float DistanceToHighestPoint = distanceToTarget / 2;
            float TimeToHighestPoint = DistanceToHighestPoint / Vz;
            float GravityEffectToHighestPoint = Gravity * TimeToHighestPoint;

            float Vy = GravityEffectToHighestPoint;

            ForwardSpeed = Vz;
            VerticalSpeed = Vy;

            //Debug.LogFormat("BulletBehavior.Launch: ForwardSpeed={0}, VerticalSpeed={1}, LaunchAngle={2}, R={3}, tanALpha={4}, H={5}",
            //    ForwardSpeed, VerticalSpeed, LaunchAngle, R, tanAlpha, heading);

            // create the velocity vector in local space and get it in global space

            //BulletBehavior.Launch: ForwardSpeed=NaN, VerticalSpeed=NaN, LaunchAngle=60, R=15.60759, tanALpha=1.732051, H=-0.8795097
        }

        private bool _dead = false;
        private float _prevDistanceToTarget = 100000F;

        private void Update()
        {
            if (_dead)
            {
                return;
            }

            transform.Translate(
                    ForwardSpeed * Vector3.forward * Time.deltaTime 
                    + VerticalSpeed * Vector3.up * Time.deltaTime);

            VerticalSpeed -= Gravity * Time.deltaTime;


            // small trick to detect if shell has reached the target
            float distanceToTarget = Vector3.Distance(transform.position, _targetCoordinates);
            if (distanceToTarget > _prevDistanceToTarget)
            {
                Explode();
            }
            _prevDistanceToTarget = distanceToTarget;
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
                        _explosionPrefab, transform.position, Quaternion.identity);
                Destroy(explosion, 3F);
            }

            if (_trailEmitter != null)
            {
                //ParticleSystem.EmissionModule emission = _trailEmitter.emission;
                //emission.enabled = false;
            }

            Destroy(gameObject, 10F);
        }

        //public void setBullet(Vector3 StartPosition, Vector3 EndPosition, float Vellocity = 30, int arc = 60)
        //{
        //    bullet._startPosition = StartPosition;
        //    bullet._endPosition = EndPosition;
        //    bullet._vellocity = Vellocity;
        //    bullet._arc = 60;
        //}
    }
}
