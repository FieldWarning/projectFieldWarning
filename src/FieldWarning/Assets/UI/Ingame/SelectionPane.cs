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
using UnityEngine.UI;

namespace PFW.UI.Ingame
{
    /// <summary>
    /// Drives the UI section that shows information about the
    /// currently selected unit(s).
    /// </summary>
    public sealed class SelectionPane : MonoBehaviour
    {
        [SerializeField]
        private WeaponSlot[] _weaponSlots = null;
        [SerializeField]
        private Image _unitBackgroundImage = null;
        [SerializeField]
        private Image _unitImage = null;
        [SerializeField]
        private Image[] _healthBars = null;
        [SerializeField]
        private TMPro.TextMeshProUGUI _unitName = null;
        [SerializeField]
        private Color _highHealth = Color.cyan;
        [SerializeField]
        private Color _lowHealth = Color.red;

        private PlatoonBehaviour _selectedPlatoon;

        private void Start()
        {
            foreach (WeaponSlot slot in _weaponSlots)
            {
                slot.gameObject.SetActive(false);
            }
            gameObject.SetActive(false);
            _selectedPlatoon = null;
        }

        private void Update()
        {
            if (_selectedPlatoon == null)
                return;

            for (int i = 0; i < _healthBars.Length; i++)
            {
                if (_selectedPlatoon.Units.Count > i)
                {
                    UnitDispatcher unit = _selectedPlatoon.Units[i];
                    _healthBars[i].fillAmount = unit.GetHealth() / unit.MaxHealth;
                    _healthBars[i].color = Util.InterpolateColors(
                            _lowHealth, _highHealth, unit.GetHealth() / unit.MaxHealth);
                    _healthBars[i].transform.parent.gameObject.SetActive(true);
                }
                else
                {
                    _healthBars[i].transform.parent.gameObject.SetActive(false);
                }
            }
        }

        public void OnSelectionChanged(List<PlatoonBehaviour> selectedPlatoons)
        {
            if (selectedPlatoons.Count == 1)
            {
                gameObject.SetActive(true);

                _selectedPlatoon = selectedPlatoons[0];
                UnitDispatcher unit = _selectedPlatoon.Units[0];
                _unitBackgroundImage.sprite = _selectedPlatoon.Unit.ArmoryBackgroundImage;
                _unitImage.sprite = _selectedPlatoon.Unit.ArmoryImage;
                _unitName.text = _selectedPlatoon.Unit.Name;

                List<Cannon> weapons = unit.AllWeapons;
                int i = 0;
                foreach (WeaponSlot slot in _weaponSlots)
                {
                    if (i < weapons.Count)
                    {
                        slot.gameObject.SetActive(true);
                        slot.DisplayWeapon(_selectedPlatoon, i);
                    }
                    else
                    {
                        slot.gameObject.SetActive(false);
                    }

                    i++;
                }
            }
            else 
            {
                // TODO show a panel with a clickable selection button for each platoon;
                gameObject.SetActive(false);
                _selectedPlatoon = null;
            }
        }
        public void OnSelectionCleared()
        {
            gameObject.SetActive(false);
            _selectedPlatoon = null;
        }
    }
}
