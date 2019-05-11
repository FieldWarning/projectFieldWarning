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

using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using PFW.Model.Armory;
using PFW.Model.Game;

namespace PFW.UI.Ingame
{
    /// <summary>
    /// Creates the interface for purchasing units within a match.
    /// </summary>
    public class DeploymentMenu : MonoBehaviour
    {
        public GameObject MenuButtonPrefab;
        public GameObject UnitCardDeploymentPrefab;

        public PlayerBehaviour LocalPlayer;

        private bool _isOpen = false;

        private Text _menuButton;
        private CanvasGroup _categoryButtonsPanel;
        private CanvasGroup _unitCardsPanel;

        private void Start()
        {
            var service = GameObject.Find("Service");

            _menuButton = GameObject.Find("OpenMenuButton").GetComponentInChildren<Text>();

            _categoryButtonsPanel = GameObject.Find("UnitButtons").GetComponent<CanvasGroup>();
            _unitCardsPanel = GameObject.Find("UnitCards").GetComponent<CanvasGroup>();

            CloseMenu();

            for (UnitCategory cat = 0; cat < UnitCategory._SIZE; cat++) {
                UnitCategory categoryForDelegate = cat;  // C# is bad
                GameObject btn = Instantiate(
                        MenuButtonPrefab, _categoryButtonsPanel.transform);
                btn.GetComponentInChildren<Text>().text = cat.ToString();
                btn.GetComponentInChildren<Button>().onClick.AddListener(
                        delegate { CategorySelected(categoryForDelegate); });
            }
        }

        private void CategorySelected(UnitCategory cat)
        {
            var allUnitCards = _unitCardsPanel.GetComponentsInChildren<Button>();

            foreach (var c in allUnitCards)
                Destroy(c.gameObject);

            var allUnitsOfCat = LocalPlayer.Data.Deck.ByCategory(cat);

            foreach (Unit unit in allUnitsOfCat) {
                var card = Instantiate(
                        UnitCardDeploymentPrefab, _unitCardsPanel.transform);
                card.GetComponentInChildren<Text>().text = unit.Name;

                // this is very hacky and WIP just to keep the current spawning system working
                var session = GameObject.Find("GameSession");

                // See above, we need to either make this fully dynamic or put the cat names in the type system:
                switch (cat) {
                case UnitCategory.TNK:
                    card.GetComponentInChildren<Button>().onClick.AddListener(session.GetComponent<InputManager>().TankButtonCallback);
                    break;
                case UnitCategory.SUP:
                    card.GetComponentInChildren<Button>().onClick.AddListener(session.GetComponent<InputManager>().ArtyButtonCallback);
                    break;
                default:
                    break;
                }

                // TODO Set picture too
                // TODO Transports?
            }
        }

        private void Update()
        {
            UpdateDeploymentPoints();
        }

        public void ToggleDeploymentMenu()
        {
            if (_isOpen)
                CloseMenu();
            else
                OpenMenu();
        }

        private void OpenMenu()
        {
            ShowPanel(_categoryButtonsPanel);
            ShowPanel(_unitCardsPanel);

            _isOpen = true;
        }

        private void CloseMenu()
        {
            HidePanel(_categoryButtonsPanel);
            HidePanel(_unitCardsPanel);

            _isOpen = false;
        }

        private void ShowPanel(CanvasGroup g)
        {
            g.alpha = 1f;
            g.blocksRaycasts = true;
        }

        private void HidePanel(CanvasGroup g)
        {
            g.alpha = 0f;
            g.blocksRaycasts = false;
        }

        private void UpdateDeploymentPoints()
        {
            _menuButton.text = LocalPlayer.Money.ToString();
        }
    }
}