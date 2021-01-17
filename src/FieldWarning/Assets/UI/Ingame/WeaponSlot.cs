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
using PFW.Units;

namespace PFW.UI.Ingame
{
    /// <summary>
    /// When a unit is selected, this class is used to show
    /// the weapons of the unit at the bottom of the screen
    /// (one instance of this class per weapon)
    /// </summary>
    public sealed class WeaponSlot : MonoBehaviour
    {
        [SerializeField]
        private Image _weaponIcon = null;
        [SerializeField]
        private TextMeshProUGUI _traits = null;
        [SerializeField]
        private TextMeshProUGUI _shotsLeft = null;
        [SerializeField]
        private TextMeshProUGUI _reload = null;

        private PlatoonBehaviour _platoon = null;
        private int _weaponId = -1;

        public void DisplayWeapon(PlatoonBehaviour platoon, int weaponId)
        {
            _platoon = platoon;
            _weaponId = weaponId;
            Cannon weapon = _platoon.Units[0].AllWeapons[_weaponId];
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

        private void Update()
        {
            if (_platoon != null)
            {
                Cannon weapon = _platoon.Units[0].AllWeapons[_weaponId];
                if (weapon.PercentageAimed < 1)
                {
                    _reload.text = Mathf.Round(weapon.PercentageAimed * 100) + "% aim";
                }
                else
                {
                    _reload.text = Mathf.Round(weapon.PercentageReloaded * 100) + "% rld";
                }
                _shotsLeft.text = "";
                for (int i = 0; i < weapon.Ammo.Length; i++)
                {
                    int platoonShellsRemaining = 0;
                    for (int j = 0; j < _platoon.Units.Count; j++)
                    {
                        platoonShellsRemaining += 
                            _platoon.Units[j].AllWeapons[_weaponId].Ammo[i].ShellCountRemaining;
                    }

                    _shotsLeft.text += 
                        platoonShellsRemaining + "/" + 
                        weapon.Ammo[i].ShellCount * _platoon.Units.Count + ", ";
                }

                _shotsLeft.text = _shotsLeft.text.Substring(0, _shotsLeft.text.Length - 2);
            }
        }
    }
}
