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
using System.Collections.Generic;
using PFW.Units;

namespace PFW.UI.Ingame
{
    /// <summary>
    /// This is the health bar implementation from our old
    /// platoon label implementation (pre-04.2020).
    /// 
    /// It may serve as a reference if we want to have healthbars
    /// on the platoon labels again. Otherwise, we can delete it.
    /// </summary>
    public class CompoundHealthbarBehaviour : MonoBehaviour
    {
        // Use this for initialization
        private void Start()
        {
            transform.localScale = new Vector3(0.85f, 1, 1);
            transform.localPosition = new Vector3(0.116f, 0.441f, 0);
        }

        public void AssociateToRealUnits(List<UnitDispatcher> o)
        {
            float FIRST_BAR_POSITION = -0.16f;
            float TOTAL_LENGTH = 1.05f;

            DestroyChilden();

            float barLength = TOTAL_LENGTH / o.Count;
            float scale = barLength;

            for (int i = 0; i < o.Count; i++) {
                var obj = GameObject.Instantiate(Resources.Load<GameObject>("HealthbarContainer"));
                obj.GetComponent<HealthBarBehaviour>().SetUnit(o[i]);
                obj.transform.parent = transform;

                obj.transform.localScale = new Vector3(scale, .08f, 1);

                // Scale affects the magnitude of translations, e.g.
                // an object positioned at X=2 will show as if it is at X=1 if it has scale 0.5. So we move the starting point:
                float scaledFirstBarPosition;
                switch (o.Count) { // TODO should really figure out the mistake in the formula instead of hardcoding like this:
                case 1:
                    scaledFirstBarPosition = -0.16f;
                    break;
                case 2:
                    scaledFirstBarPosition = -0.4f;
                    break;
                case 3:
                    scaledFirstBarPosition = -0.5f;
                    break;
                case 4:
                    scaledFirstBarPosition = -0.53f;
                    break;
                default:
                    scaledFirstBarPosition = FIRST_BAR_POSITION / scale;
                    break;
                }

                float barEnd = scaledFirstBarPosition + ((o.Count - i) * (TOTAL_LENGTH / o.Count));
                float barStart = barEnd - barLength;

                obj.transform.localPosition = new Vector3(barStart, 0, -.01f);
            }
        }

        private void DestroyChilden()
        {
            for (int i = transform.childCount - 1; i >= 0; i--) {
                Object.Destroy(transform.GetChild(i));
            }
        }
    }
}
