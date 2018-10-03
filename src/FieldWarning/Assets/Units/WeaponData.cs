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

        public WeaponData(float fireRange = 4000, float damage = 5, float reloadTime = 10, int shortBurst = 1, float accuracy = 40,
            float arcHorizontal = 180, float arcUp = 40, float arcDown = 20, float rotationRate = 40f)
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

