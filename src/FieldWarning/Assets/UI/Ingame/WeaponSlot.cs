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
using UnityEngine.UI;
using TMPro;
using PFW.Units.Component.Weapon;

namespace PFW.UI.Ingame
{
    public sealed class WeaponSlot : MonoBehaviour
    {
        [SerializeField]
        private Image _weaponIcon;
        [SerializeField]
        private TextMeshProUGUI _traits;
        [SerializeField]
        private TextMeshProUGUI _shotsLeft;
        [SerializeField]
        private TextMeshProUGUI _reload;

        public void DisplayWeapon(Cannon weapon)
        {
            _weaponIcon.sprite = weapon.HudIcon;
            _traits.text = "";
            foreach (Ammo ammo in weapon.Ammo)
            {
                string trait;
                switch (ammo.DamageType)
                {
                    case DamageType.HEAVY_ARMS:
                        trait = "HA";
                        break;
                    case DamageType.SMALL_ARMS:
                        trait = "SA";
                        break;
                    default:
                        trait = ammo.DamageType.ToString();
                        break;
                }
                _traits.text += trait + ", ";
            }
            _traits.text = _traits.text.Substring(0, _traits.text.Length - 2);
        }
    }
}
