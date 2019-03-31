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

using AssemblyCSharp;
using UnityEngine;

using PFW.Weapons;

namespace PFW.Units.Component.Weapon
{
    public class WeaponComponent : MonoBehaviour
    {
        public WeaponData Data { get; private set; }
        public UnitBehaviour Unit { get; private set; }
        public float ReloadTimeLeft { get; private set; }
        public AudioSource Source { get; private set; }
        private bool _movingTowardsTarget = false;
        private TargetTuple _target;
        public void SetTarget(Vector3 position)
        {
            var distance = Vector3.Distance(Unit.transform.position, position);

            if (distance < Data.FireRange) {
                _target = new TargetTuple(position);
            } else {
                _target = new TargetTuple(position);
                _movingTowardsTarget = true;
                Unit.SetUnitDestination(position);
            }
        }

        // --------------- BEGIN PREFAB ----------------
        [SerializeField]
        private int _dataIndex;
        [SerializeField]
        private Transform _mount;
        [SerializeField]
        private Transform _turret;
        [SerializeField]
        private Transform _barrel;
        [SerializeField]
        private Transform _shotEmitter;
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

        private void Start()
        {
            Source = GetComponent<AudioSource>();
        }

        private void StopMovingIfInRangeOfTarget()
        {
            if (_movingTowardsTarget) {
                if (Vector3.Distance(Unit.transform.position, _target.Position) < Data.FireRange) {
                    _movingTowardsTarget = false;
                    Unit.SetUnitDestination(Unit.transform.position);
                }
            }
        }

        private void Update()
        {
            StopMovingIfInRangeOfTarget();

            if (Unit.Platoon.Type == Ingame.Prototype.UnitType.Tank) {
                if (_target != null) {

                    MaybeDropOutOfRangeTarget();

                    if (RotateTurret(_target) && !_movingTowardsTarget)
                        TryFireWeapon(_target);
                }
            }


            if (Unit.Platoon.Type == Ingame.Prototype.UnitType.Arty) {
                if (_target != null && !_movingTowardsTarget) {
                    RotateTurret(_target);
                    TryFireWeapon(_target);
                }
            }
        }

        public void WakeUp()
        {
            Data = Unit.Data.weaponData[_dataIndex];
            ReloadTimeLeft = Data.ReloadTime;
            enabled = true;
        }


        private bool RotateTurret(TargetTuple target)
        {
            bool aimed = false;
            float targetTurretAngle = 0f;
            float targetBarrelAngle = 0f;

            Vector3 pos = target.Position;

            if (pos != Vector3.zero) {
                aimed = true;
                // comented out because arty has no shot emmiter:
                // shotEmitter.LookAt(pos);

                Vector3 directionToTarget = pos - _turret.position;
                Quaternion rotationToTarget = Quaternion.LookRotation(_mount.transform.InverseTransformDirection(directionToTarget));

                targetTurretAngle = rotationToTarget.eulerAngles.y.unwrapDegree();
                if (Mathf.Abs(targetTurretAngle) > Data.ArcHorizontal) {
                    targetTurretAngle = 0f;
                    aimed = false;
                }

                targetBarrelAngle = rotationToTarget.eulerAngles.x.unwrapDegree();
                if (targetBarrelAngle < -Data.ArcUp || targetBarrelAngle > Data.ArcDown) {
                    targetBarrelAngle = 0f;
                    aimed = false;
                }
            }

            float turretAngle = _turret.localEulerAngles.y;
            float barrelAngle = _barrel.localEulerAngles.x;
            float turn = Time.deltaTime * Data.RotationRate;
            float deltaAngle;

            deltaAngle = (targetTurretAngle - turretAngle).unwrapDegree();
            if (Mathf.Abs(deltaAngle) > turn) {
                turretAngle += (deltaAngle > 0 ? 1 : -1) * turn;
                aimed = false;
            } else {
                turretAngle = targetTurretAngle;
            }

            #region ArtyAdditionalCode
            if (Unit.Platoon.Type == Ingame.Prototype.UnitType.Arty) {
                targetBarrelAngle = -Data.ArcUp;
            }
            #endregion

            deltaAngle = (targetBarrelAngle - barrelAngle).unwrapDegree();
            if (Mathf.Abs(deltaAngle) > turn) {
                barrelAngle += (deltaAngle > 0 ? 1 : -1) * turn;
                aimed = false;
            } else {
                barrelAngle = targetBarrelAngle;
            }

            _turret.localEulerAngles = new Vector3(0, turretAngle, 0);
            _barrel.localEulerAngles = new Vector3(barrelAngle, 0, 0);

            return aimed;
        }

        private bool FireWeapon(TargetTuple target)
        {

            if (Unit.Platoon.Type == Ingame.Prototype.UnitType.Tank) {

                // sound
                Source.PlayOneShot(_shotSound, _shotVolume);
                // particle
                _shotEffect.Play();

                if (target.IsUnit) {
                    System.Random rnd = new System.Random();
                    int roll = rnd.Next(1, 100);

                    // HIT
                    if (roll < Data.Accuracy) {
                        target.Enemy.GetComponent<UnitBehaviour>().HandleHit(Data.Damage);
                        return true;
                    }
                } else {
                    // ensure we only fire pos once
                    this._target = null;
                }

                // MISS
                return false;
            }

            if (Unit.Platoon.Type == Ingame.Prototype.UnitType.Arty) {
                //  Vector3 start = new Vector3(ShotStarterPosition.position.x, ShotStarterPosition.position.y+0., ShotStarterPosition.position.z);


                GameObject shell = Resources.Load<GameObject>("shell");
                GameObject shell_new = Instantiate(shell, _shotStarterPosition.position, _shotStarterPosition.transform.rotation);
                shell_new.GetComponent<BulletBehavior>().SetUp(_shotStarterPosition, target.Position, 60);

                return true;
            }

            return false;
        }


        private bool TryFireWeapon(TargetTuple target)
        {
            ReloadTimeLeft -= Time.deltaTime;
            if (ReloadTimeLeft > 0)
                return false;

            ReloadTimeLeft = Data.ReloadTime;
            return FireWeapon(target);
        }

        public GameObject FindClosestEnemy()
        {
            // TODO utilize precomputed distance lists from session
            // TODO Have a global List of enemy Units to prevent using FindGameobjects since it is very ressource intensive
            // Maybe add Sphere shaped collider with the radius of the range and then use trigger enter and exit to keep a list of in range Units
            GameObject[] Units = GameObject.FindGameObjectsWithTag(UnitBehaviour.UNIT_TAG);
            GameObject Target = null;
            var thisTeam = Unit.Platoon.Owner.Team;

            foreach (GameObject enemy in Units) {
                // Filter out friendlies:
                if (enemy.GetComponent<UnitBehaviour>().Platoon.Owner.Team == thisTeam)
                    continue;

                // See if they are in range of weapon:
                var distance = Vector3.Distance(Unit.transform.position, enemy.transform.position);
                if (distance < Data.FireRange) {
                    return enemy;
                }
            }
            return Target;
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
            if (distance > Data.FireRange)
                _target = null;
        }

        private class TargetTuple
        {
            private Vector3 _position { get; set; }
            public GameObject Enemy { get; private set; }
            /// <summary>
            /// The position (location) of the target, 
            /// regardless of whether its a unit or not.
            /// </summary>
            public Vector3 Position {
                get {
                    if (IsGround)
                        return _position;
                    else
                        return Enemy.transform.position;
                }
            }

            public TargetTuple(Vector3 position)
            {
                _position = position;
                Enemy = null;
            }
            public TargetTuple(GameObject go)
            {
                _position = Vector3.zero;
                Enemy = go;
            }

            public bool Exists()
            {
                return Enemy != null || _position != Vector3.zero;
            }

            /// <summary>
            /// Is the target just a position on the ground, 
            /// as opposed to an enemy unit?
            /// </summary>
            public bool IsGround {
                get {
                    return Enemy == null;
                }
            }

            /// <summary>
            /// Is the target an enemy unit, 
            /// as opposed to a position on the ground?
            /// </summary>
            public bool IsUnit {
                get {
                    return Enemy != null;
                }
            }
        }
    }
}

