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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using PFW.Model.Armory;
using PFW.Model.Match;

namespace PFW.UI.Ingame
{
    /// <summary>
    /// Creates the interface for purchasing units within a match.
    /// </summary>
    public class DeploymentMenu : MonoBehaviour
    {
        public GameObject MenuButtonPrefab;
        public GameObject UnitCardDeploymentPrefab;

        private PlayerBehaviour _localPlayer;

        private bool _isOpen = false;

        private Text _menuButton;
        private CanvasGroup _categoryButtonsPanel;
        private CanvasGroup _unitCardsPanel;

        private InputManager _inputManager;

        /// <summary>
        /// Pretend that this is a constructor.
        /// </summary>
        public void Initialize(
                InputManager inputManager, 
                PlayerBehaviour localPlayer)
        {
            _inputManager = inputManager;
            _localPlayer = localPlayer;
            enabled = true;
        }

        private void Start()
        {
            _menuButton = GameObject.Find("OpenMenuButton").GetComponentInChildren<Text>();

            _categoryButtonsPanel = GameObject.Find("UnitButtons").GetComponent<CanvasGroup>();
            _unitCardsPanel = GameObject.Find("UnitCards").GetComponent<CanvasGroup>();

            CloseMenu();

            for (UnitCategory cat = 0; cat < UnitCategory._SIZE; cat++) 
            {
                UnitCategory categoryForDelegate = cat;  // C# is bad
                GameObject btn = Instantiate(
                        MenuButtonPrefab, _categoryButtonsPanel.transform);
                btn.GetComponentInChildren<Text>().text = cat.ToString();
                btn.GetComponentInChildren<Button>().onClick.AddListener(
                        delegate { CategorySelected(categoryForDelegate); });
            }
        }

        private UnitCategory _lastCategory;
        public void UpdateTeamBelonging()
        {
            CategorySelected(_lastCategory);
        }

        private void CategorySelected(UnitCategory cat)
        {
            _lastCategory = cat;
            Button[] allUnitCards = _unitCardsPanel.GetComponentsInChildren<Button>();

            foreach (Button c in allUnitCards)
                Destroy(c.gameObject);

            List<Unit> allUnitsOfCat = _localPlayer.Data.Deck.ByCategory(cat);

            foreach (Unit unit in allUnitsOfCat) 
            {
                GameObject card = Instantiate(
                        UnitCardDeploymentPrefab, _unitCardsPanel.transform);
                card.GetComponentInChildren<Text>().text = unit.Name;
                Util.RecursiveFindChild(card.transform, "Price")
                        .GetComponent<TMPro.TMP_Text>()
                        .text = unit.Price.ToString();
                card.GetComponentInChildren<Button>().onClick.AddListener(
                        delegate {
                                _inputManager.BuyCallback(unit);
                        });

                card.transform.Find("Image").GetComponent<Image>().sprite =
                        unit.ArmoryImage;
                card.transform.Find("BackgroundImage").GetComponent<Image>().sprite =
                        unit.ArmoryBackgroundImage;
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
            _menuButton.text = _localPlayer.Money.ToString();
        }
    }
}
