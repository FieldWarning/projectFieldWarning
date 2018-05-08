using System;

namespace PFW.Weapons
{
	//base class that is used for weapon intialization in the unit 
	//unit behavior class
	//should be made into a library later on
	public class WeaponData
	{
		public float FireRange;
		public float Damage; //will make this its own class later on so it can have HE,AP,HEAT etc...
		public float ReloadTime;
		public int ShotBurst; ///used to describe if the weapon fires single shell or in burst
		public float Accuracy;
		public float ArcHorizontal, ArcUp, ArcDown;
		public float RotationRate;

		public WeaponData (float fireRange=2000, float damage=5, float reloadTime=10, int shortBurst=1, float accuracy=40,
			float arcHorizontal=180, float arcUp=30, float arcDown=20, float rotationRate=40f)
		//base constructor with default values
		{
			FireRange = fireRange;
			Damage = damage;
			ReloadTime = reloadTime;
			ShotBurst = shortBurst;
			Accuracy = accuracy;
			ArcHorizontal = arcHorizontal;
			ArcUp = arcUp;
			ArcDown = arcDown;
			RotationRate = rotationRate;
		}
	}
}

