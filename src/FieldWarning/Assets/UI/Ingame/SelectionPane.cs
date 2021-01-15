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

using PFW.Units;
using PFW.Units.Component.Weapon;
using System.Collections.Generic;
using UnityEngine;


namespace PFW.UI.Ingame
{
    /// <summary>
    /// Drives the UI section that shows information about the
    /// currently selected unit(s).
    /// </summary>
    public sealed class SelectionPane : MonoBehaviour
    {
        private WeaponSlot[] _weaponSlots;

        private void Start()
        {
            _weaponSlots = GetComponentsInChildren<WeaponSlot>();
            foreach (WeaponSlot slot in _weaponSlots)
            {
                slot.gameObject.SetActive(false);
            }
        }

        public void OnSelectionChanged(List<PlatoonBehaviour> selectedPlatoons)
        {
            gameObject.SetActive(true);
            List<Cannon> weapons = selectedPlatoons[0].Units[0].AllWeapons;
            int i = 0;
            foreach (WeaponSlot slot in _weaponSlots)
            {
                if (i < weapons.Count)
                {
                    slot.gameObject.SetActive(true);
                    slot.DisplayWeapon(selectedPlatoons[0], i);
                }
                else 
                {
                    slot.gameObject.SetActive(false);
                }

                i++;
            }
        }
        public void OnSelectionCleared()
        {
            gameObject.SetActive(false);
        }
    }
}
