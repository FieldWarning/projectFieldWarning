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
 *
 */

namespace Assets.UI.Ingame.UnitLabel
{
    using UnityEngine;

    public class UnitLabelAttacher : MonoBehaviour
    {
        public GameObject Label { get; private set; }

        public bool Visible
        {
            get => this.Visible;
            set => this.Label.SetActive(value);
        }

        public Vector3 GetScreenPosition()
        {
            // new Vector3(x, y, z = 0) - that's why we can just use (0, 5). :)
            return Camera.main.WorldToScreenPoint(this.transform.position + new Vector3(0f, 5f));
        }

        /* LateUpdate is called after all Update functions have been called. 
           This is useful to order script execution. For example a follow camera should 
           always be implemented in LateUpdate because it tracks objects that might have moved inside Update. */
        public void LateUpdate()
        {
            this.Label.transform.position = this.GetScreenPosition();
        }

        public void Start()
        {
            this.Label = Instantiate(
                Resources.Load<GameObject>("UnitLabel"),
                GameObject.Find("UIWrapper").GetComponent<Canvas>().transform);
        }
    }
}
