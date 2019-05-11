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

namespace PFW.Units.Component.Weapon
{
    [System.Serializable]
    public class WeaponData
    {
        public float FireRange = 4000;
        public float Damage = 5;
        public float ReloadTime = 10;
		public float Accuracy = 40;

        public WeaponData(
            float fireRange, float damage, float reloadTime, float accuracy)
        {
            FireRange = fireRange;
            Damage = damage;
            ReloadTime = reloadTime;
            Accuracy = accuracy;
        }
    }
}

