using UnityEngine;
using System;

namespace PFW.Weapons
{
    
    public class Weapon : MonoBehaviour
    {

        public WeaponData data { get; private set; }
        public UnitBehaviour unit { get; private set; }
        public float reloadTimeLeft { get; private set; }

        private TargetTuple target;
        public void setTarget(Vector3 position) {
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
        [SerializeField]
        private ParticleSystem shotEffect;

        [SerializeField]
        private AudioClip shotSound;
        [SerializeField]
        private float shotVolume = 1.0F;
        // ---------------- END PREFAB -----------------

        public void Start ()
        {
            unit = gameObject.GetComponent<UnitBehaviour> ();
            enabled = false;
        }

        public void Update()
        {
            if (target == null)
                target = new TargetTuple(FindClosestEnemy());

            if (RotateTurret (target))
                TryFireWeapon (target);
        }

        public void WakeUp ()
        {
            data = unit.data.weaponData[dataIndex];
            reloadTimeLeft = (float)data.ReloadTime;
            enabled = true;
        }

        bool RotateTurret (TargetTuple target)
        {
            bool aimed = false;
            float targetTurretAngle = 0f;
            float targetBarrelAngle = 0f;

            Vector3 pos = target.position == null ? target.enemy.transform.position : target.position;

            if (pos != Vector3.zero) {
                aimed = true;
                shotEmitter.LookAt (pos);

                Vector3 directionToTarget = pos - turret.position;
                Quaternion rotationToTarget = Quaternion.LookRotation (mount.transform.InverseTransformDirection (directionToTarget));

                targetTurretAngle = rotationToTarget.eulerAngles.y.unwrapDegree();
                if (Mathf.Abs (targetTurretAngle) > data.ArcHorizontal) {
                    targetTurretAngle = 0f;
                    aimed = false;
                }

                targetBarrelAngle = rotationToTarget.eulerAngles.x.unwrapDegree();
                if (targetBarrelAngle < -data.ArcUp || targetBarrelAngle > data.ArcDown) {
                    targetBarrelAngle = 0f;
                    aimed = false;
                }
            }

            float turretAngle = turret.localEulerAngles.y;
            float barrelAngle = barrel.localEulerAngles.x;
            float turn = Time.deltaTime * data.RotationRate;
            float deltaAngle;

            deltaAngle = (targetTurretAngle - turretAngle).unwrapDegree();
            if (Mathf.Abs (deltaAngle) > turn) {
                turretAngle += (deltaAngle > 0 ? 1 : -1) * turn;
                aimed = false;
            } else {
                turretAngle = targetTurretAngle;
            }

            deltaAngle = (targetBarrelAngle - barrelAngle).unwrapDegree();
            if (Mathf.Abs (deltaAngle) > turn) {
                barrelAngle += (deltaAngle > 0 ? 1 : -1) * turn;
                aimed = false;
            } else {
                barrelAngle = targetBarrelAngle;
            }

            turret.localEulerAngles = new Vector3 (0, turretAngle, 0);
            barrel.localEulerAngles = new Vector3 (barrelAngle, 0, 0);

            return aimed;
        }

        private bool FireWeapon(TargetTuple target) 
        {
            // sound
            unit.source.PlayOneShot(shotSound, shotVolume);
            // particle
            shotEffect.Play();


            if (target.enemy != null) {
                System.Random rnd = new System.Random();
                int roll = rnd.Next(1, 100);

                // HIT
                if (roll < data.Accuracy) {
                    target.enemy.GetComponent<UnitBehaviour>()
                        .HandleHit(data.Damage);
                    return true;
                }
            } else { 
                // ensure we only fire pos once
                this.target = null;
            }

            // MISS
            return false;
        }


        private bool TryFireWeapon (TargetTuple target)
        {
            reloadTimeLeft -= Time.deltaTime;
            if (reloadTimeLeft > 0)
                return false;

            reloadTimeLeft = (float)data.ReloadTime;        
            return FireWeapon(target);
        }

        public GameObject FindClosestEnemy ()
        {
            GameObject[] units = GameObject.FindGameObjectsWithTag (UnitBehaviour.UNIT_TAG);
            GameObject Target = null;
            Team thisTeam = unit.platoon.owner.getTeam();

            for (int i = 0; i < (int)units.Length; i++) {
                // Filter out friendlies:
                if (units [i].GetComponent<UnitBehaviour> ().platoon.owner.getTeam() == thisTeam)
                    continue;

                // See if they are in range of weapon:
                var distance = Vector3.Distance (units [i].transform.position, unit.transform.position);
                if (distance < data.FireRange) {
                    return units [i];
                }
            }
            return Target;
        }


        private class TargetTuple {
            public Vector3 position { get; private set; }
            public GameObject enemy { get; private set; }

            public TargetTuple(Vector3 position) {
                this.position = position;
                enemy = null;
            }
            public TargetTuple(GameObject go) {
                position = Vector3.zero;
                enemy = go;
            }
        }
    }
}

