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

using PFW.Model;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using PFW.Model.Armory;

namespace PFW.UI.MainMenu
{
    /// <summary>
    /// The UI for creating and editing decks in the main menu.
    /// 
    /// There are generally three views, only one visible at a time:
    /// * A list of decks, with options to create one, delete it or open it for editing
    /// * A detailed view on a single deck
    /// * A unit catalog that is browsed for info or to add a unit to the currently selected deck
    /// </summary>
    public class DeckPanel : MonoBehaviour
    {
        // A deck name that can be clicked to select and start editing the deck.
        [SerializeField]
        private GameObject _deckEntryPrefab = null;

        // List of deck entries.
        [SerializeField]
        private GameObject _deckList = null;

        // Container that holds all UI elements shown when a deck is opened for editing.
        [SerializeField]
        private GameObject _deckContents = null;

        // The panel with all units
        [SerializeField]
        private GameObject _armory = null;

        // Legacy fields, TODO remove:
        [SerializeField]
        private GameObject _topNation = null;
        [SerializeField]
        private GameObject _topDeckContents = null;
        [SerializeField]
        private GameObject _unitCategories = null;
        [SerializeField]
        private TMPro.TextMeshProUGUI _deckNameField = null;

        private void Start()
        {
            // Create a button in the deck list for each deck:
            float nextY = 1f;
            const float STEP = 0.05f;
            foreach (KeyValuePair<string, Model.Armory.Deck> 
                    nameDeck in GameSession.Singleton.Decks)
            {
                GameObject entry = Instantiate(_deckEntryPrefab);
                entry.transform.SetParent(_deckList.transform, false);
                RectTransform t = (RectTransform)entry.transform;
                t.anchorMax = new Vector2(t.anchorMax.x, nextY);
                nextY -= STEP;
                t.anchorMin = new Vector2(t.anchorMin.x, nextY);

                entry.GetComponent<OpenDeckButton>().Initialize(
                        nameDeck.Key, this);
            }
        }

        public void OnDeckClicked(string deckName)
        {
            if (GameSession.Singleton.Decks.TryGetValue(deckName, out Deck deck))
            {
                _deckList.SetActive(false);
                //_armory.SetActive(true);
                //_deckContents.SetActive(true);
                _unitCategories.SetActive(true);
                _topDeckContents.SetActive(true);
                //_topNation.SetActive(true);
                _deckNameField.text = deckName;
            }
            else
            {
                string msg = "User clicked a deck button for a deck that does not exist." +
                        $"Deck name = {deckName}. Known decks: ";
                foreach (string name in GameSession.Singleton.Decks.Keys)
                {
                    msg += $"{name}; ";
                }
                Logger.LogMenu(LogLevel.BUG, msg);
            }
        }
    }
}
