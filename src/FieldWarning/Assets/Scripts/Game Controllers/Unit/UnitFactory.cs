﻿/**
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

using PFW.Model.Game;
using PFW.Units.Component.Movement;

namespace PFW.UI.Prototype
{
    public class UnitFactory
    {
        private MatchSession _session { get; }

        public UnitFactory(MatchSession session)
        {
            _session = session;
        }

        public GameObject FindPrefab(UnitType type)
        {
            GameObject unit;

            switch (type) {
            case UnitType.Tank:
                unit = Resources.Load<GameObject>("Tank");
                //label.GetComponentInChildren<Text>().text = "M1A2 Abrams";
                break;
            case UnitType.AFV:
                unit = Resources.Load<GameObject>("AFV");
                break;
            case UnitType.Infantry:
                var obj = new GameObject();
                var b = obj.AddComponent<InfantryMovementComponent>();
                b.enabled = false;
                unit = obj;
                break;
            case UnitType.Arty:
                unit = Resources.Load<GameObject>("Arty");
                break;
            default:
                unit = null;
                break;
            }

            //unit.GetComponent<UnitLabelAttacher>().Label = label;

            return unit;
        }

        public GameObject MakeUnit(GameObject prefab, Color minimapColor)
        {
            GameObject unit = Object.Instantiate(prefab);
            AddMinimapIcon(unit, minimapColor);

            return unit;
        }

        public GameObject MakeGhostUnit(GameObject prefab)
        {
            GameObject unit = Object.Instantiate(prefab);
            unit.GetComponent<MovementComponent>().enabled = false;

            Shader shader = Resources.Load<Shader>("Ghost");
            unit.ApplyShaderRecursively(shader);
            unit.transform.position = 100 * Vector3.down;

            return unit;
        }

        private void AddMinimapIcon(GameObject unit, Color minimapColor)
        {
            var minimapIcon = GameObject.Instantiate(Resources.Load<GameObject>("MiniMapIcon"));
            minimapIcon.GetComponent<SpriteRenderer>().color = minimapColor;
            minimapIcon.transform.parent = unit.transform;
            //The icon is placed slightly above ground to prevent flickering
            minimapIcon.transform.localPosition = new Vector3(0,0.01f,0);
        }
    }

    public enum UnitType
    {
        Tank,
        Infantry,
        AFV,
        Arty
    }
}