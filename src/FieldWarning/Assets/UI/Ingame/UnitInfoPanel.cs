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

using PFW.Model.Armory;
using PFW.Units;
using PFW.Units.Component.Weapon;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PFW.UI.Ingame
{
    /// <summary>
    /// Represents a unit card in the armory, including detailed
    /// stats. These cards show up not only in the armory, but also 
    /// when the unit info hotkey is pressed inside a match.
    /// </summary>
    public sealed class UnitInfoPanel : MonoBehaviour
    {
        private Unit _currentlyShownUnit;

        [SerializeField]
        private Image _image = null;
        [SerializeField]
        private Image _backgroundImage = null;
        [SerializeField]
        private TMP_Text _nameField = null;
        [SerializeField]
        private TMP_Text _priceField = null;
        [SerializeField]
        private TMP_Text _speedField = null;
        [SerializeField]
        private TMP_Text _opticsField = null;
        [SerializeField]
        private TMP_Text _stealthField = null;
        [SerializeField]
        private TMP_Text _frontArmorField = null;
        [SerializeField]
        private TMP_Text _sideArmorField = null;
        [SerializeField]
        private TMP_Text _rearArmorField = null;
        [SerializeField]
        private TMP_Text _topArmorField = null;

        [SerializeField]
        private List<WeaponInfoPanel> _weaponPanels = null;

        /// <summary>
        /// TODO the cannon argument here is a hack because
        /// it only exists on live units. We need to extract weapon data
        /// from the turret config itself, otherwise it wont work for the armory.
        /// </summary>
        public void ShowUnitInfo(Unit unit, List<Cannon> weapons)
        {
            // Limited toggle behavior (prob belongs in InputManager):
            if (_currentlyShownUnit != null && _currentlyShownUnit.Name == unit.Name)
            {
                _currentlyShownUnit = null;
                gameObject.SetActive(false);
                return;
            }

            _currentlyShownUnit = unit;

            // Unit card:
            _nameField.text = unit.Name;
            _priceField.text = unit.Price.ToString() + "pts";

            _image.sprite = unit.ArmoryImage;
            _backgroundImage.sprite = unit.ArmoryBackgroundImage;

            // Stats:

            _speedField.text = unit.Config.MovementSpeed.ToString() + "kmh";
            _opticsField.text = unit.Config.Recon.MaxSpottingRange.ToString() 
                + "/" + unit.Config.Recon.StealthPenetration.ToString();
            _stealthField.text = unit.Config.Recon.Stealth.ToString();
            _frontArmorField.text = unit.Config.Armor.FrontArmor.ToString();
            _sideArmorField.text = unit.Config.Armor.SideArmor.ToString();
            _rearArmorField.text = unit.Config.Armor.RearArmor.ToString();
            _topArmorField.text = unit.Config.Armor.TopArmor.ToString();

            for (int i = 0; i < _weaponPanels.Count; i++)
            {
                if (i < weapons.Count)
                {
                    _weaponPanels[i].ShowWeaponInfo(weapons[i]);
                }
                else 
                {
                    _weaponPanels[i].HideWeaponInfo();
                }
            }

            gameObject.SetActive(true);
        }

        public void HideUnitInfo()
        {
            gameObject.SetActive(false);
        }
    }

}
