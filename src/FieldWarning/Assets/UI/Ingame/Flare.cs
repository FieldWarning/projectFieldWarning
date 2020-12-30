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

namespace PFW.UI.Ingame
{
    /// <summary>
    /// Players can press F1 to place a flare on the map,
    /// which is a graphic indicator visible to their own team only.
    /// </summary>
    public sealed class Flare : MonoBehaviour
    {
        [SerializeField]
        private TMPro.TextMeshProUGUI _text = null;

        public string Text { get { return _text.text; } }

        public static Flare Create(string text, Vector3 position)
        {
            GameObject go = Instantiate(Resources.Load<GameObject>("Flare"));
            Flare flare = go.GetComponent<Flare>();
            flare._text.text = text;
            go.transform.position = position;
            return flare;
        }
    }
}
