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

namespace PFW.Ingame.Prototype
{
    public static class UnitFactory
    {
        public static GameObject FindPrefab(UnitType type)
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
                var b = obj.AddComponent<InfantryBehaviour>();
                b.enabled = false;
                unit = obj;
                break;
            default:
                unit = null;
                break;
            }

            //unit.GetComponent<UnitLabelAttacher>().Label = label;

            return unit;
        }

        public static GameObject MakeUnit(GameObject prefab, Color minimapColor)
        {
            GameObject unit = Object.Instantiate(prefab);
            AddMinimapIcon(unit, minimapColor);
            AddVisibleBehaviour(unit);

            return unit;
        }

        public static GameObject MakeGhostUnit(GameObject prefab)
        {
            GameObject unit = Object.Instantiate(prefab);
            unit.GetComponent<UnitBehaviour>().enabled = false;

            Shader shader = Resources.Load<Shader>("Ghost");
            unit.ApplyShaderRecursively(shader);
            unit.transform.position = 100 * Vector3.down;

            return unit;
        }

        private static void AddMinimapIcon(GameObject unit, Color minimapColor)
        {
            var minimapIcon = GameObject.Instantiate(Resources.Load<GameObject>("MiniMapIcon"));
            minimapIcon.GetComponent<SpriteRenderer>().color = minimapColor;
            minimapIcon.transform.parent = unit.transform;
            minimapIcon.transform.localPosition = Vector3.zero;
        }

        private static void AddVisibleBehaviour(GameObject unit)
        {
            var unitBehaviour = unit.GetComponent<UnitBehaviour>();
            VisibleBehavior vis = new VisibleBehavior(unit, unitBehaviour);
            unitBehaviour.VisibleBehavior = vis;
        }
    }

    public enum UnitType
    {
        Tank,
        Infantry,
        AFV
    }
}