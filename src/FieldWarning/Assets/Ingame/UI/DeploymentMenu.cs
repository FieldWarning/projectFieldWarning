using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Model.Armory;
using Assets.Service;
using UnityEngine;
using UnityEngine.UI;

public class DeploymentMenu : MonoBehaviour
{
    public GameObject MenuButtonPrefab;
    public GameObject UnitCardDeploymentPrefab;

    private bool _isOpen = false;

    private Text _menuButton;
    private CanvasGroup _categoryButtonsPanel;
    private CanvasGroup _unitCardsPanel;
    private IUnitCategoryService _unitCategoryService;
    private IUnitService _unitService;

    private void Start()
    {
        var service = GameObject.Find("Service");
        _unitCategoryService = service.GetComponent<UnitCategoryService>();
        _unitService = service.GetComponent<UnitService>();

        _menuButton = GameObject.Find("OpenMenuButton").GetComponentInChildren<Text>();

        _categoryButtonsPanel = GameObject.Find("UnitButtons").GetComponent<CanvasGroup>();
        _unitCardsPanel = GameObject.Find("UnitCards").GetComponent<CanvasGroup>();

        CloseMenu();

        foreach (var cat in _unitCategoryService.All())
        {
            var btn = Instantiate(MenuButtonPrefab, _categoryButtonsPanel.transform);
            btn.GetComponentInChildren<Text>().text = cat.Name;
            btn.GetComponentInChildren<Button>().onClick.AddListener(delegate { CategorySelected(cat); });
        }
    }

    private void CategorySelected(UnitCategory cat)
    {
        var allUnitCards = _unitCardsPanel.GetComponentsInChildren<Button>();

        foreach (var c in allUnitCards)
            Destroy(c.gameObject);

        // TODO Get units from Deck not just all units.
        var allUnitsOfCat = _unitService.All().Where(u => u.Category.Name == cat.Name).ToList();

        foreach (var unit in allUnitsOfCat)
        {
            var card = Instantiate(UnitCardDeploymentPrefab, _unitCardsPanel.transform);
            card.GetComponentInChildren<Text>().text = unit.Name;

            // this is very hacky and WIP just to keep the current spawning system working
            var managers = GameObject.Find("Managers");
            card.GetComponentInChildren<Button>().onClick.AddListener(managers.GetComponent<UIManagerBehaviour>().tankButtonCallback);
            
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
        _menuButton.text = Random.Range(0, 2000).ToString();
    }
}
