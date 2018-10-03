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

namespace PFW.Weapons
{
    public class Weapon : MonoBehaviour
    {
        public WeaponData data { get; private set; }
        public UnitBehaviour unit { get; private set; }
        public float reloadTimeLeft { get; private set; }

        private TargetTuple target;
        public void setTarget(Vector3 position)
        {
            var distance = Vector3.Distance(unit.transform.position, position);

            if (distance < data.FireRange)
                target = new TargetTuple(position);
        }

        // --------------- BEGIN PREFAB ----------------
        [SerializeField]
        private int dataIndex;
        [SerializeField]
        private Transform mount;
        [SerializeField]
        private Transform turret;
        [SerializeField]
        private Transform barrel;
        [SerializeField]
        private Transform shotEmitter;
        //-----newly added start-------------//
        [SerializeField]
        private Transform ShotStarterPosition;//where the shell spawns 
        [SerializeField]
        private GameObject bullet;//The shell being Fired 
        //-----newly added end-------------//
        [SerializeField]
        private ParticleSystem shotEffect;//will aim to make actual objects fire and not effects 
        [SerializeField]
        private AudioClip shotSound;
        [SerializeField]
        private float shotVolume = 1.0F;
        // ---------------- END PREFAB -----------------

        public void Awake()
        {
            unit = gameObject.GetComponent<UnitBehaviour>();
            enabled = false;
           

        }

        public void Update()
        {

            if (unit.Platoon.Type == Ingame.Prototype.UnitType.Tank)
            {

                if (target == null || !target.exists())
                    target = new TargetTuple(FindClosestEnemy());

                if (RotateTurret(target))
                    TryFireWeapon(target);
            }


            if (unit.Platoon.Type == Ingame.Prototype.UnitType.Arty)
            {
                if (target != null)
                {
                    RotateTurret(target);
                    TryFireWeapon(target);
                }
            }

        }

        public void WakeUp()
        {
            data = unit.Data.weaponData[dataIndex];
            reloadTimeLeft = data.ReloadTime;
            enabled = true;
        }



        private bool RotateTurret(TargetTuple target)
        {
            bool aimed = false;
            float targetTurretAngle = 0f;
            float targetBarrelAngle = 0f;

            Vector3 pos = target.enemy == null ? target.position : target.enemy.transform.position;

            if (pos != Vector3.zero)
            {
                aimed = true;
               // shotEmitter.LookAt(pos);
               //shot emmiter was comented out because arty has no shot emmiter

                Vector3 directionToTarget = pos - turret.position;
                Quaternion rotationToTarget = Quaternion.LookRotation(mount.transform.InverseTransformDirection(directionToTarget));

                targetTurretAngle = rotationToTarget.eulerAngles.y.unwrapDegree();
                if (Mathf.Abs(targetTurretAngle) > data.ArcHorizontal)
                {
                    targetTurretAngle = 0f;
                    aimed = false;
                }

                targetBarrelAngle = rotationToTarget.eulerAngles.x.unwrapDegree();
                if (targetBarrelAngle < -data.ArcUp || targetBarrelAngle > data.ArcDown)
                {
                    targetBarrelAngle = 0f;
                    aimed = false;
                }
            }

            float turretAngle = turret.localEulerAngles.y;
            float barrelAngle = barrel.localEulerAngles.x;
            float turn = Time.deltaTime * data.RotationRate;
            float deltaAngle;

            deltaAngle = (targetTurretAngle - turretAngle).unwrapDegree();
            if (Mathf.Abs(deltaAngle) > turn)
            {
                turretAngle += (deltaAngle > 0 ? 1 : -1) * turn;
                aimed = false;
            }
            else
            {
                turretAngle = targetTurretAngle;
            }

            #region ArtyAdditionalCode
            if (unit.Platoon.Type == Ingame.Prototype.UnitType.Arty)
            {
                targetBarrelAngle = -data.ArcUp;
            }
            #endregion

            deltaAngle = (targetBarrelAngle - barrelAngle).unwrapDegree();
            if (Mathf.Abs(deltaAngle) > turn)
            {
                barrelAngle += (deltaAngle > 0 ? 1 : -1) * turn;
                aimed = false;
            }
            else
            {
                barrelAngle = targetBarrelAngle;
            }

            turret.localEulerAngles = new Vector3(0, turretAngle, 0);
            barrel.localEulerAngles = new Vector3(barrelAngle, 0, 0);

            return aimed;
        }

        private bool FireWeapon(TargetTuple target)
        {

            if (unit.Platoon.Type == Ingame.Prototype.UnitType.Tank)
            {

                // sound
                unit.Source.PlayOneShot(shotSound, shotVolume);
                // particle
                shotEffect.Play();

                if (target.enemy != null)
                {
                    System.Random rnd = new System.Random();
                    int roll = rnd.Next(1, 100);

                    // HIT
                    if (roll < data.Accuracy)
                    {

                     
                        target.enemy.GetComponent<UnitBehaviour>()
                            .HandleHit(data.Damage);
                        return true;
                    }
                }
                else
                {
                    // ensure we only fire pos once
                    this.target = null;
                }

                // MISS
                return false;
            }

            if (unit.Platoon.Type == Ingame.Prototype.UnitType.Arty)
            {
                //  Vector3 start = new Vector3(ShotStarterPosition.position.x, ShotStarterPosition.position.y+0., ShotStarterPosition.position.z);


                GameObject shell = Resources.Load<GameObject>("shell");
                GameObject shell_new =Instantiate(shell, ShotStarterPosition.position, ShotStarterPosition.transform.rotation);
                shell_new.GetComponent<BulletBehavior>().SetUp(ShotStarterPosition, target.position, 60);

                //Debug.Break();
               

                return true;
                
            }

            return false;
        }


        private bool TryFireWeapon(TargetTuple target)
        {
            reloadTimeLeft -= Time.deltaTime;
            if (reloadTimeLeft > 0)
                return false;

            reloadTimeLeft = data.ReloadTime;
            return FireWeapon(target);
        }

        public GameObject FindClosestEnemy()
        {
            // TODO utilize precomputed distance lists from session
            GameObject[] units = GameObject.FindGameObjectsWithTag(UnitBehaviour.UNIT_TAG);
            GameObject Target = null;
            var thisTeam = unit.Platoon.Owner.Team;

            foreach (GameObject unit in units)
            {
                // Filter out friendlies:
                if (unit.GetComponent<UnitBehaviour>().Platoon.Owner.Team == thisTeam)
                    continue;

                // See if they are in range of weapon:
                var distance = Vector3.Distance(unit.transform.position, unit.transform.position);
                if (distance < data.FireRange)
                {
                    return unit;
                }
            }
            return Target;
        }


        private class TargetTuple
        {
            public Vector3 position { get; private set; }
            public GameObject enemy { get; private set; }

            public TargetTuple(Vector3 position)
            {
                this.position = position;
                enemy = null;
            }
            public TargetTuple(GameObject go)
            {
                position = Vector3.zero;
                enemy = go;
            }

            public bool exists()
            {
                return enemy != null || position != Vector3.zero;
            }
        }
    }
}

