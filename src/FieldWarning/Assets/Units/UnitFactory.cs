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

using PFW.Model.Armory;
using PFW.Units;

namespace PFW.UI.Prototype
{
    public class UnitFactory
    {
        public void MakeUnit(Unit armoryUnit, GameObject unit, PlatoonBehaviour platoon)
        {
            armoryUnit.Augment(unit, false);
            Color minimapColor = platoon.Owner.Team.Color;
            AddMinimapIcon(unit, minimapColor);

            UnitDispatcher unitDispatcher =
                    unit.GetComponent<UnitDispatcher>();
            unitDispatcher.Initialize(platoon);
            unitDispatcher.enabled = true;
        }

        public void MakeGhostUnit(Unit armoryUnit, GameObject unit)
        {
            armoryUnit.Augment(unit, true);
            unit.SetActive(true);
            unit.name = "Ghost" + unit.name;

            Shader shader = Resources.Load<Shader>("Ghost");
            unit.ApplyShaderRecursively(shader);
            unit.transform.position = 100 * Vector3.down;
        }

        private void AddMinimapIcon(GameObject unit, Color minimapColor)
        {
            GameObject minimapIcon = Object.Instantiate(
                    Resources.Load<GameObject>("MiniMapIcon"));
            minimapIcon.GetComponent<SpriteRenderer>().color = minimapColor;
            minimapIcon.transform.parent = unit.transform;
            // The icon is placed slightly above ground to prevent flickering
            minimapIcon.transform.localPosition = new Vector3(0,0.01f,0);
        }
    }
}
