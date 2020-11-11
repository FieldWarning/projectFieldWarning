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
        private GameObject _unitCardDeploymentPanel;
        [SerializeField]
        private TMP_Text _speedField;
        [SerializeField]
        private TMP_Text _opticsField;
        [SerializeField]
        private TMP_Text _stealthField;
        [SerializeField]
        private TMP_Text _frontArmorField;
        [SerializeField]
        private TMP_Text _sideArmorField;
        [SerializeField]
        private TMP_Text _rearArmorField;
        [SerializeField]
        private TMP_Text _topArmorField;

        public void ShowUnitInfo(Unit unit)
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
            _unitCardDeploymentPanel.GetComponentInChildren<Text>().text = unit.Name;
            Util.RecursiveFindChild(_unitCardDeploymentPanel.transform, "Price")
                    .GetComponent<TMP_Text>()
                    .text = unit.Price.ToString();

            _unitCardDeploymentPanel.transform.Find("Image").GetComponent<Image>().sprite =
                    unit.ArmoryImage;
            _unitCardDeploymentPanel.transform.Find("BackgroundImage").GetComponent<Image>().sprite =
                    unit.ArmoryBackgroundImage;

            // Stats:

            _speedField.text = unit.Config.MovementSpeed.ToString() + "kmh";
            _opticsField.text = unit.Config.Recon.MaxSpottingRange.ToString() 
                + "/" + unit.Config.Recon.StealthPenetration.ToString();
            _stealthField.text = unit.Config.Recon.Stealth.ToString();
            _frontArmorField.text = unit.Config.Armor.FrontArmor.ToString();
            _sideArmorField.text = unit.Config.Armor.SideArmor.ToString();
            _rearArmorField.text = unit.Config.Armor.RearArmor.ToString();
            _topArmorField.text = unit.Config.Armor.TopArmor.ToString();

            gameObject.SetActive(true);
        }

        public void HideUnitInfo()
        {
            gameObject.SetActive(false);
        }
    }

}
