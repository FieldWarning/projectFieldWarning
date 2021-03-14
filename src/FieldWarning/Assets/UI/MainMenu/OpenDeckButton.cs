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
using UnityEngine.UI;

namespace PFW.UI.MainMenu
{
    /// <summary>
    /// A button in the deck list that allows opening a specific deck.
    /// </summary>
    public class OpenDeckButton : MonoBehaviour
    {
        [SerializeField]
        private Button _button = null;
        [SerializeField]
        private TMPro.TextMeshProUGUI _deckNameField = null;

        private DeckPanel _panel;

        public void Initialize(string name, DeckPanel panel)
        {
            _deckNameField.text = name;
            _button.onClick.AddListener(OnDeckClicked);
            _panel = panel;
        }

        private void OnDeckClicked()
        {
            _panel.OnDeckClicked(_deckNameField.text);
        }
    }
}
