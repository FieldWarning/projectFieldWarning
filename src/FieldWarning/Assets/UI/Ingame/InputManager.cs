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

using PFW.Model.Game;
using PFW.Model.Armory;
using PFW.Units;

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

        private List<SpawnPointBehaviour> _spawnPointList = new List<SpawnPointBehaviour>();
        private ClickManager _rightClickManager;

        public enum MouseMode {
            NORMAL,       //< Left click selects, right click orders normal movement or attack.
            PURCHASING,   //< Left click purchases platoon, right click cancels.
            FIRE_POS,     //< Left click orders fire position, right click cancels.
            REVERSE_MOVE, //< Left click reverse moves to cursor, right click cancels.
            FAST_MOVE,    //< Left click fast moves to cursor, right click cancels.
            SPLIT         //< Left click splits the platoon, right click cancels.
        };

        public MouseMode CurMouseMode { get; private set; } = MouseMode.NORMAL;

        private BuyTransaction _currentBuyTransaction;

        private MatchSession _session;
        public MatchSession Session {
            get {
                return _session;
            }

            set {
                if (_session == null)
                    _session = value;
            }
        }

        private SelectionManager _selectionManager;

        private PlayerData _localPlayer {
            get {
                return Session.LocalPlayer.Data;
            }
        }

        private Vector3 _boxSelectStart;

        private void Awake()
        {
            _selectionManager = new SelectionManager();
            _selectionManager.Awake();
        }

        private void Start()
        {
            _firePosReticle = (Texture2D)Resources.Load("FirePosTestTexture");
            if (_firePosReticle == null)
                throw new Exception("No fire pos reticle specified!");

            _primedReticle = (Texture2D)Resources.Load("PrimedCursor");
            if (_primedReticle == null)
                throw new Exception("No primed reticle specified!");

            _rightClickManager = new ClickManager(1, MoveGhostsToMouse, OnOrderShortClick, OnOrderLongClick, OnOrderHold);
        }

        private void Update()
        {
            _selectionManager.Update(CurMouseMode);

            switch (CurMouseMode) {

            case MouseMode.PURCHASING:

                RaycastHit hit;
                if (Util.GetTerrainClickLocation(out hit)) {
                    ShowGhostUnitsAndMaybePurchase(hit);
                }

                MaybeExitPurchasingModeAndRefund();
                break;

            case MouseMode.NORMAL:
                ApplyHotkeys();
                _rightClickManager.Update();
                break;

            case MouseMode.FIRE_POS:
                ApplyHotkeys();

                if (Input.GetMouseButtonDown(0))
                    _selectionManager.DispatchFirePosCommand();

                if ((Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftShift))
                    || Input.GetMouseButtonDown(1))
                    EnterNormalMode();

                break;

            case MouseMode.REVERSE_MOVE:
                ApplyHotkeys();
                if (Input.GetMouseButtonDown(0)) {
                    MoveGhostsToMouse();
                    _selectionManager.DispatchMoveCommand(
                            false, MoveWaypoint.MoveMode.reverseMove);
                }

                if ((Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftShift))
                    || Input.GetMouseButtonDown(1))
                    EnterNormalMode();
                break;

            case MouseMode.FAST_MOVE:
                ApplyHotkeys();
                if (Input.GetMouseButtonDown(0)) {
                    MoveGhostsToMouse();
                    _selectionManager.DispatchMoveCommand(
                            false, MoveWaypoint.MoveMode.fastMove);
                }

                if ((Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftShift))
                    || Input.GetMouseButtonDown(1))
                    EnterNormalMode();
                break;

            case MouseMode.SPLIT:
                ApplyHotkeys();
                if (Input.GetMouseButtonDown(0)) {
                    _selectionManager.DispatchSplitCommand(_localPlayer);
                }

                if ((Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftShift))
                    || Input.GetMouseButtonDown(1))
                    EnterNormalMode();
                break;
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

        /**
         * Purchase units if there is a buy selection.
         */
        private void MaybePurchaseGhostUnits(SpawnPointBehaviour closestSpawn)
        {
            if (Input.GetMouseButtonUp(0)) {
                bool noUIcontrolsInUse = EventSystem.current.currentSelectedGameObject == null;

                if (!noUIcontrolsInUse)
                    return;

                if (_currentBuyTransaction == null)
                    return;

                closestSpawn.BuyPlatoons(_currentBuyTransaction.PreviewPlatoons);

                if (Input.GetKey(KeyCode.LeftShift)) {
                    // We turned the current ghosts into real units, so:
                    _currentBuyTransaction = _currentBuyTransaction.Clone();
                } else {
                    ExitPurchasingMode();
                }
            }
        }

        private void MaybeExitPurchasingModeAndRefund()
        {
            if (Input.GetMouseButton(1)) {
                foreach (var g in _currentBuyTransaction.PreviewPlatoons) {
                    g.Destroy();
                }

                int unitPrice = _currentBuyTransaction.Unit.Price;
                Session.LocalPlayer.Refund(unitPrice * _currentBuyTransaction.UnitCount);

                ExitPurchasingMode();
            }
        }

        /**
         * The ghost units are used to briefly hold the destination
         * for a move order, so they need to be moved to the cursor
         * if a move order click is issued.
         */
        void MoveGhostsToMouse()
        {
            RaycastHit hit;
            if (Util.GetTerrainClickLocation(out hit))
                _selectionManager.PrepareMoveOrderPreview(hit.point);
        }

        void OnOrderHold()
        {
            RaycastHit hit;
            if (Util.GetTerrainClickLocation(out hit))
                _selectionManager.RotateMoveOrderPreview(hit.point);
        }

        void OnOrderShortClick()
        {
            if (!_selectionManager.Empty) {
                DisplayOrderFeedback();
            }

            _selectionManager.DispatchMoveCommand(false, MoveWaypoint.MoveMode.normalMove);
        }

        void OnOrderLongClick()
        {
            _selectionManager.DispatchMoveCommand(true, MoveWaypoint.MoveMode.normalMove);
        }

        // Show a Symbol at the position where a move order was issued:
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

        /**
         * Called when a unit card from the buy menu is pressed.
         */
        public void BuyCallback(Unit unit)
        {
            bool paid = Session.LocalPlayer.TryPay(unit.Price);
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
            if (!_session.isChatFocused) {
            if (Commands.Unload) {
                _selectionManager.DispatchUnloadCommand();

            } else if (Commands.Load) {
                _selectionManager.DispatchLoadCommand();

            } else if (Commands.FirePos && !_selectionManager.Empty) {
                EnterFirePosMode();

            } else if (Commands.ReverseMove && !_selectionManager.Empty) {
                EnterReverseMoveMode();

            } else if (Commands.FastMove && !_selectionManager.Empty) {
                EnterFastMoveMode();
            } else if (Commands.Split && !_selectionManager.Empty) {
                EnterSplitMode();
            }
        }
        }

        private void EnterFirePosMode()
        {
            CurMouseMode = MouseMode.FIRE_POS;
            Vector2 hotspot = new Vector2(_firePosReticle.width / 2, _firePosReticle.height / 2);
            Cursor.SetCursor(_firePosReticle, hotspot, CursorMode.Auto);
        }

        private void EnterNormalMode()
        {
            CurMouseMode = MouseMode.NORMAL;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        private void EnterFastMoveMode()
        {
            CurMouseMode = MouseMode.FAST_MOVE;
            Cursor.SetCursor(_primedReticle, Vector2.zero, CursorMode.Auto);
        }

        private void EnterReverseMoveMode()
        {
            CurMouseMode = MouseMode.REVERSE_MOVE;
            Cursor.SetCursor(_primedReticle, Vector2.zero, CursorMode.Auto);
        }

        private void EnterSplitMode()
        {
            CurMouseMode = MouseMode.SPLIT;
            Cursor.SetCursor(_primedReticle, Vector2.zero, CursorMode.Auto);
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
        public static bool Unload {
            get {
                return Input.GetKeyDown(Hotkeys.Unload);
            }
        }

        public static bool Load {
            get {
                return Input.GetKeyDown(Hotkeys.Load);
            }
        }

        public static bool FirePos {
            get {
                return Input.GetKeyDown(Hotkeys.FirePos);
            }
        }

        public static bool ReverseMove {
            get {
                return Input.GetKeyDown(Hotkeys.ReverseMove);
            }
        }

        public static bool FastMove {
            get {
                return Input.GetKeyDown(Hotkeys.FastMove);
            }
        }

        public static bool Split {
            get {
                return Input.GetKeyDown(Hotkeys.Split);
            }
        }
    }
}