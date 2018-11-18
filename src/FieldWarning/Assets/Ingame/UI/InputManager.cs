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

using PFW.Ingame.Prototype;
using PFW.Model.Game;

namespace PFW.Ingame.UI
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

        public enum MouseMode { normal, purchasing, firePos, reverseMove, fastMove };
        public MouseMode CurMouseMode { get; private set; } = MouseMode.normal;

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

        void Awake()
        {
            _selectionManager = new SelectionManager();
            _selectionManager.Awake();
        }

        void Start()
        {
            _firePosReticle = (Texture2D)Resources.Load("FirePosTestTexture");
            if (_firePosReticle == null)
                throw new Exception("No fire pos reticle specified!");

            _primedReticle = (Texture2D)Resources.Load("PrimedCursor");
            if (_primedReticle == null)
                throw new Exception("No primed reticle specified!");

            _rightClickManager = new ClickManager(1, MoveGhostsToMouse, OnOrderShortClick, OnOrderLongClick, OnOrderHold);
        }

        void Update()
        {
            _selectionManager.Update(CurMouseMode);

            switch (CurMouseMode) {

            case MouseMode.purchasing:

                RaycastHit hit;
                if (Util.GetTerrainClickLocation(out hit)
                    && hit.transform.gameObject.name.Equals("Terrain")) {

                    ShowGhostUnitsAndMaybePurchase(hit);
                }

                MaybeExitPurchasingMode();
                break;

            case MouseMode.normal:
                ApplyHotkeys();
                _rightClickManager.Update();
                break;

            case MouseMode.firePos:
                ApplyHotkeys();

                if (Input.GetMouseButtonDown(0))
                    _selectionManager.DispatchFirePosCommand();

                if ((Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftShift))
                    || Input.GetMouseButtonDown(1))
                    EnterNormalMode();

                break;

            case MouseMode.reverseMove:
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

            case MouseMode.fastMove:
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

            default:
                throw new Exception("impossible state");
            }
        }

        public void OnGUI()
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

        private void MaybePurchaseGhostUnits(SpawnPointBehaviour closestSpawn)
        {
            if (Input.GetMouseButtonUp(0)) {
                bool noUIcontrolsInUse = EventSystem.current.currentSelectedGameObject == null;

                if (!noUIcontrolsInUse)
                    return;

                if (_currentBuyTransaction == null)
                    return;

                closestSpawn.BuyPlatoons(_currentBuyTransaction.GhostPlatoons);

                if (Input.GetKey(KeyCode.LeftShift)) {
                    // We turned the current ghosts into real units, so:
                    _currentBuyTransaction = _currentBuyTransaction.Clone();
                } else {
                    ExitPurchasingMode();
                }
            }
        }

        private void MaybeExitPurchasingMode()
        {
            if (Input.GetMouseButton(1)) {
                foreach (var g in _currentBuyTransaction.GhostPlatoons)
                    g.GetComponent<GhostPlatoonBehaviour>().Destroy();

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
            _selectionManager.DispatchMoveCommand(false, MoveWaypoint.MoveMode.normalMove);
        }

        void OnOrderLongClick()
        {
            _selectionManager.DispatchMoveCommand(true, MoveWaypoint.MoveMode.normalMove);
        }

        /**
         * Called when the tank button is pressed in the buy menu.
         */ 
        public void TankButtonCallback()
        {
            if (_currentBuyTransaction == null)
                _currentBuyTransaction = new BuyTransaction(UnitType.Tank, _localPlayer);
            else
                _currentBuyTransaction.AddUnit();

            //buildUnit(UnitType.Tank);
            CurMouseMode = MouseMode.purchasing;
        }

        /**
         * Called when the arty button is pressed in the buy menu.
         */
        public void ArtyButtonCallback()
        {
            if (_currentBuyTransaction == null)
                _currentBuyTransaction = new BuyTransaction(UnitType.Arty, _localPlayer);
            else
                _currentBuyTransaction.AddUnit();
            CurMouseMode = MouseMode.purchasing;
        }

        /**
         * Called when infantry button is pressed in the buy menu.
         */
        public void InfantryButtonCallback()
        {
            BuildUnit(UnitType.Infantry);
            CurMouseMode = MouseMode.purchasing;
        }

        /**
         * Called when the afv button is pressed in the buy menu.
         */
        public void AFVButtonCallback()
        {
            BuildUnit(UnitType.AFV);
            CurMouseMode = MouseMode.purchasing;
        }

        public void BuildUnit(UnitType t)
        {
            var behaviour = GhostPlatoonBehaviour.Build(t, _localPlayer, 4);
            _currentBuyTransaction.GhostPlatoons.Add(behaviour);
        }

        private void ExitPurchasingMode()
        {
            _currentBuyTransaction.GhostPlatoons.Clear();

            _currentBuyTransaction = null;

            CurMouseMode = MouseMode.normal;
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

        public void AddSpawnPoint(SpawnPointBehaviour s)
        {
            if (!_spawnPointList.Contains(s))
                _spawnPointList.Add(s);
        }

        public void ApplyHotkeys()
        {
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
            }
        }

        private void EnterFirePosMode()
        {
            CurMouseMode = MouseMode.firePos;
            Vector2 hotspot = new Vector2(_firePosReticle.width / 2, _firePosReticle.height / 2);
            Cursor.SetCursor(_firePosReticle, hotspot, CursorMode.Auto);
        }

        private void EnterNormalMode()
        {
            CurMouseMode = MouseMode.normal;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        private void EnterFastMoveMode()
        {
            CurMouseMode = MouseMode.fastMove;
            Vector2 hotspot = new Vector2(0, 0);
            Cursor.SetCursor(_primedReticle, hotspot, CursorMode.Auto);
        }

        private void EnterReverseMoveMode()
        {
            CurMouseMode = MouseMode.reverseMove;
            Vector2 hotspot = new Vector2(0, 0);
            Cursor.SetCursor(_primedReticle, hotspot, CursorMode.Auto);
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
    }
}