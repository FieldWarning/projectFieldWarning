using UnityEngine;
using System;

namespace PFW.Weapons
{
	
	public class Weapon : MonoBehaviour
	{

		public WeaponData data { get; private set; }
		public UnitBehaviour unit { get; private set; }
		public float reloadTimeLeft { get; private set; }

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

			GameObject target = FindClosestEnemy ();
			if (RotateTurret (target))
				TryFireWeapon (target);
		}

		public void WakeUp ()
		{
			data = unit.data.weaponData[dataIndex];
			reloadTimeLeft = (float)data.ReloadTime;
			enabled = true;
		}

		bool RotateTurret (GameObject target)
		{
			bool aimed = false;
			float targetTurretAngle = 0f;
			float targetBarrelAngle = 0f;

			if (target != null) {
				aimed = true;
				shotEmitter.LookAt (target.transform);

				Vector3 directionToTarget = target.transform.position - turret.position;
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

		public bool FireWeapon(GameObject target) 
		{
			// sound
			unit.source.PlayOneShot(shotSound, shotVolume);
			// particle
			shotEffect.Play();


			System.Random rnd = new System.Random();
			int roll = rnd.Next(1, 100);

			// HIT
			if (roll < data.Accuracy) {
				target.GetComponent<UnitBehaviour>()
					.HandleHit(data.Damage);
				return true;
			}

			// MISS
			return false;
		}


		public bool TryFireWeapon (GameObject target)
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
			Team thisTeam = unit.platoon.team;

			for (int i = 0; i < (int)units.Length; i++) {
				// Filter out friendlies:
				if (units [i].GetComponent<UnitBehaviour> ().platoon.team == thisTeam)
					continue;

				// See if they are in range of weapon:
				var distance = Vector3.Distance (units [i].transform.position, unit.transform.position);
				if (distance < data.FireRange) {
					return units [i];
				}
			}
			return Target;
		}
	}
}

