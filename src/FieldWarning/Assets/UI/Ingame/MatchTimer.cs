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

using System;

using UnityEngine;

using TMPro;
using Mirror;

namespace PFW.UI.Ingame
{
    /**
     * MatchTimer keeps and updates the remaining match time based on how long the
     * network server has been up.
     */
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class MatchTimer : MonoBehaviour
    {
        private TextMeshProUGUI _text;

        [SerializeField]
        private double MAX_TIME_MINUTES = 100;
        private double MAX_TIME_SECONDS;

        private void Start()
        {
            _text = gameObject.GetComponent<TextMeshProUGUI>();
            MAX_TIME_SECONDS = MAX_TIME_MINUTES * 60;
        }

        // Update is called once per frame
        private void Update()
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(MAX_TIME_SECONDS - NetworkTime.time);

            string minutes = $"{timeSpan.Hours * 60 + timeSpan.Minutes}";
            // Ensure the display is always double-digit:
            if (10 > timeSpan.Hours * 60 + timeSpan.Minutes)
            {
                minutes = "0" + minutes;
            }

            string seconds = $"{(timeSpan.Seconds < 10 ? "0" : "")}{timeSpan.Seconds}";

            _text.text = minutes + ":" + seconds;
        }
    }
}
