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
using UnityEngine;

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

        private void Start()
        {
        }

        public void ShowUnitInfo(Unit unit)
        {
            // Limited toggle behavior (prob belongs in InputManager):
            if (_currentlyShownUnit != null && _currentlyShownUnit.Id == unit.Id)
            {
                _currentlyShownUnit = null;
                gameObject.SetActive(false);
                return;
            }

            _currentlyShownUnit = unit;
            // TODO
            gameObject.SetActive(true);
        }

        public void HideUnitInfo()
        {
            gameObject.SetActive(false);
        }
    }

}
