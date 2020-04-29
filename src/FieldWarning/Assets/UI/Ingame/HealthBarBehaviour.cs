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

using PFW.Units;

namespace PFW.UI.Ingame
{
    /// <summary>
    ///     Unused leftover from the old UI system, can be recycled
    ///     or removed.
    /// </summary>
    public class HealthBarBehaviour : MonoBehaviour
    {
        private UnitDispatcher _unit;
        private GameObject _bar;

        private void Awake()
        {
            _bar = transform.GetChild(0).gameObject;
        }

        private void Update()
        {
            SetHealth(_unit.GetHealth() / _unit.MaxHealth);
        }

        public void SetUnit(UnitDispatcher o)
        {
            _unit = o;
            SetHealth(_unit.MaxHealth);
        }

        private void SetHealth(float h)
        {
            float health = Mathf.Clamp01(h);

            //bar.transform.localScale = new Vector3(health, 1, 1);
            //var offset = bar.GetComponent<Renderer>().bounds.extents.x ;
            //bar.transform.localPosition = new Vector3(offset-0.5f, 0, -.01f);
            _bar.GetComponent<Renderer>()
                .material
                .color = PickColor(health);

            //Debug.Log("-- hp : " + (1f - health));
            _bar.GetComponent<Renderer>().material.SetFloat("_Cutoff", 1f - health);
        }

        private Color PickColor(float h)
        {
            if (h < 0.25f)
                return Color.red;

            if (h < 0.5f)
                return Color.yellow;

            return Color.green;
        }
    }
}
