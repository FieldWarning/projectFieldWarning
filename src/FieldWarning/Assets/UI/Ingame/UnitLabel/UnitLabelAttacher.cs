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
            get => Visible;
            set => Label.SetActive(value);
        }

        public Vector3 GetScreenPosition() => Camera.main.WorldToScreenPoint(transform.position + new Vector3(0f, 5f));

        public void LateUpdate() => Label.transform.position = GetScreenPosition();

        public void Start()
        {
            Label = Instantiate(
                Resources.Load<GameObject>("UnitLabel"),
                GameObject.Find("UIWrapper").GetComponent<Canvas>().transform);
        }
    }
}
