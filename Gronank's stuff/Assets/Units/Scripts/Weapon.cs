using System;

namespace PFW.Weapons
{
	//base class that is used for weapon intialization in the unit 
	//unit behavior class
	//should be made into a library later on
	public class Weapon
	{
		public int FireRange;
		public int Damage; //will make this its own class later on so it can have HE,AP,HEAT etc...
		public int ReloadTime;
		public int ShotBurst; ///used to describe if the weapon fires single shell or in burst
		public int Accuracy;

		public Weapon (int fireRange=2000,int damage=5,int reloadTime=10,int shortBurst=1, int accuracy=40)
		//base constructor with default values
		{
			FireRange = fireRange;
			Damage = damage;
			ReloadTime = reloadTime;
			ShotBurst = shortBurst;
			Accuracy = accuracy;
		}
	}
}

