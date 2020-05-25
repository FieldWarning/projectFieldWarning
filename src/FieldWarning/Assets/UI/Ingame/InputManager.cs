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

using UnityEngine.EventSystems;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

using PFW.Model;
using PFW.Model.Armory;
using PFW.Model.Match;
using PFW.Model.Settings;
using PFW.Units;
using PFW.Units.Component.Movement;

namespace PFW.UI.Ingame
{
    /**
     * Handles almost all input during a match.
     *
     * Some input, particularly for to selecting and deselecting units,
     * is handled in SelectionManager instead.
     */
    public class InputManager : MonoBehaviour
    {
        private Texture2D _firePosReticle;
        private Texture2D _primedReticle;
        private Texture2D _visionRulerReticle;
        private Texture2D _forestReticle;

        private List<SpawnPointBehaviour> _spawnPointList = new List<SpawnPointBehaviour>();
        private ClickManager _rightClickManager;

        public enum MouseMode {
            NORMAL,       //< Left click selects, right click orders normal movement or attack.
            NORMAL_COVER, //< Same as above, cursor over forest
            PURCHASING,   //< Left click purchases platoon, right click cancels.
            FIRE_POS,     //< Left click orders fire position, right click cancels.
            REVERSE_MOVE, //< Left click reverse moves to cursor, right click cancels.
            FAST_MOVE,    //< Left click fast moves to cursor, right click cancels.
            SPLIT,        //< Left click splits the platoon, right click cancels.
            VISION_RULER, //< Left click selects and cancels, right click cancels.
            IN_MENU       //< Escape (or another hotkey) cancels, clicks do nothing
        };

        public MouseMode CurMouseMode { get; private set; } = MouseMode.NORMAL;

        private BuyTransaction _currentBuyTransaction;

        private MatchSession _session;
        public MatchSession Session {
            set {
                if (_session == null)
                {
                    _session = value;
                }
            }
        }

        private SelectionManager _selectionManager;
        public void PlatoonLabelClicked(PlatoonBehaviour platoon) =>
                _selectionManager.PlatoonLabelClicked(platoon);

        private PlayerData _localPlayer;
        public PlayerData LocalPlayer {
            set {
                _localPlayer = value;
                _selectionManager.LocalPlayer = value;
            }
        }

        private GameObject _rangeTooltip;
        private TMPro.TextMeshProUGUI _rangeTooltipText;
        private GameObject _settingsMenu;

        private Commands _commands;

        private void Awake()
        {
            _selectionManager = new SelectionManager();
            _selectionManager.Awake();

            _commands = new Commands(GameSession.Singleton.Settings.Hotkeys);
        }

        private void Start()
        {
            _firePosReticle = (Texture2D)Resources.Load(
                    "Cursors/FirePosCursor");
            if (_firePosReticle == null)
                throw new Exception("No fire pos reticle specified!");

            _visionRulerReticle = (Texture2D)Resources.Load(
                    "Cursors/VisionRulerCursor");
            if (_visionRulerReticle == null)
                throw new Exception("No vision ruler reticle specified!");

            _primedReticle = (Texture2D)Resources.Load("Cursors/PrimedCursor");
            if (_primedReticle == null)
                throw new Exception("No primed reticle specified!");

            _forestReticle = (Texture2D)Resources.Load("Cursors/ForestCursor");
            if (_primedReticle == null)
                throw new Exception("No primed reticle specified!");

            _rightClickManager = new ClickManager(1, MoveGhostsToMouse, OnOrderShortClick, OnOrderLongClick, OnOrderHold);

            _rangeTooltip = GameObject.Find("RangeTooltip");
            _rangeTooltipText = 
                    _rangeTooltip.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (_rangeTooltipText == null)
            {
                throw new Exception(
                    "There should be a range tooltip in the HUD hierarchy!");
            }
            _rangeTooltip.SetActive(false);

            _settingsMenu = GameObject.Find("Settings");
            if (_settingsMenu == null)
            {
                throw new Exception(
                    "There should be a settings menu object in the HUD hierarchy!");
            }
            _settingsMenu.SetActive(false);
        }

        private void Update()
        {
            _selectionManager.UpdateMouseMode(CurMouseMode);

            switch (CurMouseMode) 
            {
            case MouseMode.PURCHASING:
            {
                RaycastHit hit;
                if (Util.GetTerrainClickLocation(out hit))
                {
                    ShowGhostUnitsAndMaybePurchase(hit);
                }
                else
                {
                    _currentBuyTransaction.HidePreview();
                }

                MaybeExitPurchasingModeAndRefund();
                break;
            }
            case MouseMode.NORMAL:
            {
                ApplyHotkeys();
                _rightClickManager.Update();

                if (IsMouseOverCover())
                {
                    EnterNormalCoverMode();
                }

                break;
            }
            case MouseMode.NORMAL_COVER:
            {
                ApplyHotkeys();
                _rightClickManager.Update();

                if (!IsMouseOverCover())
                {
                    EnterNormalMode();
                }

                break;
            }
            case MouseMode.VISION_RULER:
            {
                ApplyHotkeys();

                // Show range and line of sight indicators
                RaycastHit hit;
                if (Util.GetTerrainClickLocation(out hit))
                {
                    _selectionManager.ToggleTargetingPreview(true);
                    int minDistance = _selectionManager.PlaceTargetingPreview(
                            hit.point, false);
                    _rangeTooltipText.text = minDistance.ToString() + "m";
                    _rangeTooltip.SetActive(true);
                    _rangeTooltip.transform.position = Input.mousePosition;
                }
                else 
                {
                    _selectionManager.ToggleTargetingPreview(false);
                    _rangeTooltip.SetActive(false);
                }
                

                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                    EnterNormalModeNaive();
                break;
            }
            case MouseMode.FIRE_POS:
            {
                ApplyHotkeys();

                // Show range and line of sight indicators
                RaycastHit hit;
                if (Util.GetTerrainClickLocation(out hit))
                {
                    _selectionManager.ToggleTargetingPreview(true);
                    int minDistance = _selectionManager.PlaceTargetingPreview(
                            hit.point, true);
                    _rangeTooltipText.text = minDistance.ToString() + "m";
                    _rangeTooltip.SetActive(true);
                    _rangeTooltip.transform.position = Input.mousePosition;
                }
                else 
                {
                    _selectionManager.ToggleTargetingPreview(false);
                    _rangeTooltip.SetActive(false);
                }

                // React to clicks
                if (Input.GetMouseButtonDown(0))
                    _selectionManager.DispatchFirePosCommand();

                if ((Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftShift))
                    || Input.GetMouseButtonDown(1))
                    EnterNormalModeNaive();

                break;
            }
            case MouseMode.REVERSE_MOVE:
                ApplyHotkeys();
                if (Input.GetMouseButtonDown(0)) {
                    MoveGhostsToMouse();
                    _selectionManager.DispatchMoveCommand(
                            false, MoveCommandType.REVERSE);
                }

                if ((Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftShift))
                    || Input.GetMouseButtonDown(1))
                    EnterNormalModeNaive();
                break;

            case MouseMode.FAST_MOVE:
                ApplyHotkeys();
                if (Input.GetMouseButtonDown(0)) {
                    MoveGhostsToMouse();
                    _selectionManager.DispatchMoveCommand(
                            false, MoveCommandType.FAST);
                }

                if ((Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftShift))
                    || Input.GetMouseButtonDown(1))
                    EnterNormalModeNaive();
                break;

            case MouseMode.SPLIT:
                ApplyHotkeys();
                if (Input.GetMouseButtonDown(0)) {
                    _selectionManager.DispatchSplitCommand();
                }

                if ((Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftShift))
                    || Input.GetMouseButtonDown(1))
                    EnterNormalModeNaive();
                break;
            case MouseMode.IN_MENU:
            {
                if (_commands.ToggleMenu)
                {
                    _settingsMenu.SetActive(false);
                    EnterNormalModeNaive();
                }
                break;
            }
            default:
                throw new Exception("impossible state");
            }
        }

        private void OnGUI()
        {
            _selectionManager.OnGUI();
        }

        private void ShowGhostUnitsAndMaybePurchase(RaycastHit terrainHover)
        {
            // Show ghost units under mouse:
            SpawnPointBehaviour closestSpawn = GetClosestSpawn(terrainHover.point);
            _currentBuyTransaction.PreviewPurchase(
                terrainHover.point,
                2 * terrainHover.point - closestSpawn.transform.position);

            MaybePurchaseGhostUnits(closestSpawn);
        }

        /// <summary>
        ///     Purchase units if there is a buy selection.
        /// </summary>
        /// <param name="closestSpawn"></param>
        private void MaybePurchaseGhostUnits(SpawnPointBehaviour closestSpawn)
        {
            if (Input.GetMouseButtonUp(0)) 
            {
                bool noUIcontrolsInUse = EventSystem.current.currentSelectedGameObject == null;

                if (!noUIcontrolsInUse)
                    return;

                if (_currentBuyTransaction == null)
                    return;

                closestSpawn.BuyPlatoons(_currentBuyTransaction.PreviewPlatoons);

                if (Input.GetKey(KeyCode.LeftShift)) 
                {
                    // We turned the current ghosts into real units, so:
                    _currentBuyTransaction = _currentBuyTransaction.Clone();
                } 
                else 
                {
                    ExitPurchasingMode();
                }
            }
        }

        private void MaybeExitPurchasingModeAndRefund()
        {
            if (Input.GetMouseButton(1)) 
            {
                foreach (PlatoonBehaviour p in _currentBuyTransaction.PreviewPlatoons) 
                {
                    p.DestroyPreview();
                }

                int unitPrice = _currentBuyTransaction.Unit.Price;
                _session.LocalPlayer.Refund(
                        unitPrice * _currentBuyTransaction.UnitCount);

                ExitPurchasingMode();
            }
        }

        
        /// <summary>
        ///     The ghost units are used to briefly hold the destination
        ///     for a move order, so they need to be moved to the cursor
        ///     if a move order click is issued.
        /// </summary>
        private void MoveGhostsToMouse()
        {
            RaycastHit hit;
            if (Util.GetTerrainClickLocation(out hit))
                _selectionManager.PrepareMoveOrderPreview(hit.point);
        }

        private void OnOrderHold()
        {
            RaycastHit hit;
            if (Util.GetTerrainClickLocation(out hit))
                _selectionManager.RotateMoveOrderPreview(hit.point);
        }

        private void OnOrderShortClick()
        {
            if (!_selectionManager.Empty) 
            {
                DisplayOrderFeedback();
            }

            _selectionManager.DispatchMoveCommand(false, MoveCommandType.NORMAL);
        }

        private void OnOrderLongClick()
        {
            _selectionManager.HideMoveOrderPreview();
            _selectionManager.DispatchMoveCommand(true, MoveCommandType.NORMAL);
        }

        /// <summary>
        ///     Show a symbol at the position where a move order was issued:
        /// </summary>
        private void DisplayOrderFeedback()
        {
            RaycastHit hit;
            if (Util.GetTerrainClickLocation(out hit))
                GameObject.Instantiate(
                        Resources.Load(
                                "MoveMarker",
                                typeof(GameObject)),
                        hit.point + new Vector3(0, 0.01f, 0),
                        Quaternion.Euler(new Vector3(90, 0, 0))
                );
        }

        /// <summary>
        ///     Called when a unit card from the buy menu is pressed.
        /// </summary>
        /// <param name="unit"></param>
        public void BuyCallback(Unit unit)
        {
            bool paid = _session.LocalPlayer.TryPay(unit.Price);
            if (!paid)
                return;

            if (_currentBuyTransaction == null)
                _currentBuyTransaction = new BuyTransaction(unit, _localPlayer);
            else
                _currentBuyTransaction.AddUnit();

            //buildUnit(UnitType.Tank);
            CurMouseMode = MouseMode.PURCHASING;
        }

        private void ExitPurchasingMode()
        {
            _currentBuyTransaction.PreviewPlatoons.Clear();

            _currentBuyTransaction = null;

            CurMouseMode = MouseMode.NORMAL;
        }

        private SpawnPointBehaviour GetClosestSpawn(Vector3 p)
        {
            var pointList = _spawnPointList.Where(
                x => x.Team == _localPlayer.Team).ToList();

            SpawnPointBehaviour go = pointList.First();
            float distance = Single.PositiveInfinity;

            foreach (var s in pointList) {
                if (Vector3.Distance(p, s.transform.position) < distance) {
                    distance = Vector3.Distance(p, s.transform.position);
                    go = s;
                }
            }
            return go;
        }

        public void RegisterSpawnPoint(SpawnPointBehaviour s)
        {
            if (!_spawnPointList.Contains(s))
                _spawnPointList.Add(s);
        }

        public void ApplyHotkeys()
        {
            if (!_session.IsChatFocused) 
            {
                if (_commands.Unload) 
                {
                    _selectionManager.DispatchUnloadCommand();
                } 
                else if (_commands.Load) 
                {
                    _selectionManager.DispatchLoadCommand();
                }
                else if (_commands.ToggleMenu)
                {
                    EnterMenuMode();
                }
                else if (_commands.FirePos && !_selectionManager.Empty) 
                {
                    EnterFirePosMode();
                }
                else if (_commands.AttackMove && !_selectionManager.Empty)
                {
                    Debug.LogWarning("Attack move is not implemented.");
                }
                else if (_commands.ReverseMove && !_selectionManager.Empty) 
                {
                    EnterReverseMoveMode();
                } 
                else if (_commands.FastMove && !_selectionManager.Empty)
                {
                    EnterFastMoveMode();
                } 
                else if (_commands.Split && !_selectionManager.Empty)
                {
                    EnterSplitMode();
                }
                else if (_commands.VisionTool && !_selectionManager.Empty)
                {
                    EnterVisionRulerMode();
                }
            }
        }

        private void EnterFirePosMode()
        {
            CurMouseMode = MouseMode.FIRE_POS;
            Vector2 hotspot = new Vector2(_firePosReticle.width / 2, _firePosReticle.height / 2);
            Cursor.SetCursor(_firePosReticle, hotspot, CursorMode.Auto);
        }

        /// <summary>
        /// Returns true if the user is hovering over a forest.
        /// </summary>
        private bool IsMouseOverCover() 
        {
            RaycastHit hit;
            if (Util.GetTerrainClickLocation(out hit))
            {
                if (TerrainMap.FOREST == _session.TerrainMap.GetTerrainType(
                                hit.point))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// We have two normal modes based on whether or not
        /// we're hovering over a forest (only affects cursor color).
        /// </summary>
        private void EnterNormalModeNaive()
        {
            if (IsMouseOverCover())
            {
                EnterNormalCoverMode();
            }
            else 
            {
                EnterNormalMode();
            }
        }

        private void EnterNormalMode()
        {
            CurMouseMode = MouseMode.NORMAL;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            _selectionManager.ToggleTargetingPreview(false);
            _rangeTooltip.SetActive(false);
        }

        private void EnterNormalCoverMode()
        {
            CurMouseMode = MouseMode.NORMAL_COVER;
            Cursor.SetCursor(_forestReticle, Vector2.zero, CursorMode.Auto);
            _selectionManager.ToggleTargetingPreview(false);
            _rangeTooltip.SetActive(false);
        }

        private void EnterFastMoveMode()
        {
            CurMouseMode = MouseMode.FAST_MOVE;
            Cursor.SetCursor(_primedReticle, Vector2.zero, CursorMode.Auto);
            _selectionManager.ToggleTargetingPreview(false);
            _rangeTooltip.SetActive(false);
        }

        private void EnterReverseMoveMode()
        {
            CurMouseMode = MouseMode.REVERSE_MOVE;
            Cursor.SetCursor(_primedReticle, Vector2.zero, CursorMode.Auto);
            _selectionManager.ToggleTargetingPreview(false);
            _rangeTooltip.SetActive(false);
        }

        private void EnterSplitMode()
        {
            CurMouseMode = MouseMode.SPLIT;
            Cursor.SetCursor(_primedReticle, Vector2.zero, CursorMode.Auto);
            _selectionManager.ToggleTargetingPreview(false);
            _rangeTooltip.SetActive(false);
        }

        private void EnterVisionRulerMode()
        {
            CurMouseMode = MouseMode.VISION_RULER;
            Vector2 hotspot = new Vector2(_visionRulerReticle.width / 2, _visionRulerReticle.height / 2);
            Cursor.SetCursor(_visionRulerReticle, hotspot, CursorMode.Auto);
        }

        private void EnterMenuMode()
        {
            CurMouseMode = MouseMode.IN_MENU;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            _settingsMenu.SetActive(true);
        }

        public void RegisterPlatoonBirth(PlatoonBehaviour platoon)
        {
            _selectionManager.RegisterPlatoonBirth(platoon);
        }

        public void RegisterPlatoonDeath(PlatoonBehaviour platoon)
        {
            _selectionManager.RegisterPlatoonDeath(platoon);
        }
    }

    public class Commands
    {
        private readonly Hotkeys _hotkeys;

        public Commands(Hotkeys hotkeys)
        {
            _hotkeys = hotkeys;
        }

        public bool Unload {
            get {
                return Input.GetKeyDown(_hotkeys.Unload);
            }
        }

        public bool Load {
            get {
                return Input.GetKeyDown(_hotkeys.Load);
            }
        }

        public bool FirePos {
            get {
                return Input.GetKeyDown(_hotkeys.FirePos);
            }
        }
        public bool ReverseMove {
            get {
                return Input.GetKeyDown(_hotkeys.ReverseMove);
            }
        }

        public bool AttackMove {
            get {
                return Input.GetKeyDown(_hotkeys.AttackMove);
            }
        }

        public bool FastMove {
            get {
                return Input.GetKeyDown(_hotkeys.FastMove);
            }
        }

        public bool Split {
            get {
                return Input.GetKeyDown(_hotkeys.Split);
            }
        }

        public bool VisionTool {
            get {
                return Input.GetKeyDown(_hotkeys.VisionTool);
            }
        }

        public bool ToggleMenu {
            get {
                return Input.GetKeyDown(_hotkeys.MenuToggle);
            }
        }
    }
}
