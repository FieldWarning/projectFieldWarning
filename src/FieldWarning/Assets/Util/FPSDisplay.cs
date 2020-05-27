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

namespace PFW
{
    public class FPSDisplay : MonoBehaviour
    {
        private float _deltaTime = 0.0f;

        private void Update()
        {
            _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;
        }

        private void OnGUI()
        {
            int w = Screen.width, h = Screen.height;

            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(0, 0, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperCenter;
            style.fontSize = h * 2 / 100;
            style.normal.textColor = Color.red;
            float msec = _deltaTime * 1000.0f;
            float fps = 1.0f / _deltaTime;
            string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
            GUI.Label(rect, text, style);
        }
    }
}
