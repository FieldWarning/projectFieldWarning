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
        private TextMeshProUGUI[] _shotsLeft = null;
        [SerializeField]
        private TextMeshProUGUI[] _description = null;
        [SerializeField]
        private Image _reload = null;
        [SerializeField]
        private Image _aim = null;

        private PlatoonBehaviour _platoon = null;
        private int _weaponId = -1;

        public void DisplayWeapon(PlatoonBehaviour platoon, int weaponId)
        {
            _platoon = platoon;
            _weaponId = weaponId;
            Cannon weapon = _platoon.Units[0].AllWeapons[_weaponId];
            _weaponIcon.sprite = weapon.HudIcon;

            int i = 0;
            for (; i < weapon.Ammo.Length; i++)
            {
                if (i > _description.Length)
                {
                    break;
                }

                _description[i].gameObject.transform.parent.gameObject.SetActive(true);
                _description[i].text = $" {weapon.Ammo[i].Description}";
            }

            for (; i < _description.Length; i++)
            {
                _description[i].gameObject.transform.parent.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (_platoon != null && _platoon.Units.Count != 0)
            {
                Cannon weapon = _platoon.Units[0].AllWeapons[_weaponId];
                if (weapon.PercentageAimed < 1)
                {
                    _aim.fillOrigin = 0;
                    _aim.fillAmount = weapon.PercentageAimed;
                }
                else
                {
                    _aim.fillOrigin = 1;
                    _aim.fillAmount = 1 - weapon.PercentageReloaded;
                    _reload.fillAmount = weapon.PercentageReloaded;
                }

                for (int i = 0; i < weapon.Ammo.Length; i++)
                {
                    int platoonShellsRemaining = 0;
                    for (int j = 0; j < _platoon.Units.Count; j++)
                    {
                        platoonShellsRemaining += 
                            _platoon.Units[j].AllWeapons[_weaponId].Ammo[i].ShellCountRemaining;
                    }

                    _shotsLeft[i].text = platoonShellsRemaining.ToString();
                        // + "/" + weapon.Ammo[i].ShellCount * _platoon.Units.Count + ", ";
                }
            }
        }
    }
}
