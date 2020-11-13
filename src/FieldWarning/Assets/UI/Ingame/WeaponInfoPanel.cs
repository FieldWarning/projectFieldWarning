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
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PFW.UI.Ingame
{
    /// <summary>
    /// Represents a weapon card in the armory, including detailed
    /// stats. These cards show up not only in the armory, but also 
    /// when the unit info hotkey is pressed inside a match.
    /// </summary>
    public class WeaponInfoPanel : MonoBehaviour
    {
        [SerializeField]
        private Image _image = null;
        [SerializeField]
        private TMP_Text _rangeField = null;
        [SerializeField]
        private TMP_Text _accuracyField = null;
        [SerializeField]
        private TMP_Text _damageField = null;
        [SerializeField]
        private TMP_Text _tagsField = null;

        public void ShowWeaponInfo(Cannon weapon)
        {
            _image.sprite = weapon.HudIcon;
            _tagsField.text = "";
            _accuracyField.text = "";
            _damageField.text = "";
            _rangeField.text = "";

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
                _tagsField.text += trait + "\n";

                _accuracyField.text += ammo.Accuracy + "%\n";
                _damageField.text += ammo.DamageValue + "\n";
                _rangeField.text += ammo.RangeForUI() + "\n";
            }
            _tagsField.text = _tagsField.text.Substring(0, _tagsField.text.Length - 1);
            _accuracyField.text = _accuracyField.text.Substring(0, _accuracyField.text.Length - 1);
            _damageField.text = _damageField.text.Substring(0, _damageField.text.Length - 1);
            _rangeField.text = _rangeField.text.Substring(0, _rangeField.text.Length - 1);

            gameObject.SetActive(true);
        }

        public void HideWeaponInfo()
        {
            gameObject.SetActive(false);
        }
    }
}
