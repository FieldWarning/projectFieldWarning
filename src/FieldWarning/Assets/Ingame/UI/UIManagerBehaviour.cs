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
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.EventSystems;
using Pfw.Ingame.Prototype;

using PFW.Model.Game;

namespace PFW.Ingame.UI
{
    public class UIManagerBehaviour : MonoBehaviour
    {
        private Texture2D _firePosReticle;

        // Use this for initialization
        public Player Owner;
        private Vector3 _boxSelectStart;
        public static List<SpawnPointBehaviour> SpawnPointList = new List<SpawnPointBehaviour>();
        private ClickManager _rightClickManager;

        public enum MouseMode { normal, purchasing, firePos };
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

        void Start()
        {
            _firePosReticle = (Texture2D)Resources.Load("FirePosTestTexture");

            if (_firePosReticle == null)
                throw new Exception("No fire pos reticle specified!");

            _rightClickManager = new ClickManager(1, OnOrderStart, OnOrderShortClick, OnOrderLongClick, OnOrderHold);
        }

        void Update()
        {
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
                    Session.SelectionManager.DispatchFirePosCommand();

                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                    ExitFirePosMode();

                break;

            default:
                throw new Exception("impossible state");
            }
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
                
                closestSpawn.BuyUnits(_currentBuyTransaction.GhostPlatoons);

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

        void OnOrderStart()
        {
            RaycastHit hit;
            if (Util.GetTerrainClickLocation(out hit)) 
                Session.SelectionManager.PrepareMoveOrderPreview(hit.point);            
        }

        void OnOrderHold()
        {
            RaycastHit hit;
            if (Util.GetTerrainClickLocation(out hit)) 
                Session.SelectionManager.RotateMoveOrderPreview(hit.point);
        }

        void OnOrderShortClick()
        {
            Session.SelectionManager.DispatchMoveCommand();
        }

        void OnOrderLongClick()
        {
            Session.SelectionManager.DispatchMoveCommand();
        }

        public void TankButtonCallback()
        {
            if (_currentBuyTransaction == null)
                _currentBuyTransaction = new BuyTransaction(UnitType.Tank, Owner);
            else
                _currentBuyTransaction.AddUnit();

            //buildUnit(UnitType.Tank);
            CurMouseMode = MouseMode.purchasing;
        }

        public void InfantryButtonCallback()
        {
            BuildUnit(UnitType.Infantry);
            CurMouseMode = MouseMode.purchasing;
        }

        public void AFVButtonCallback()
        {
            BuildUnit(UnitType.AFV);
            CurMouseMode = MouseMode.purchasing;
        }

        public void BuildUnit(UnitType t)
        {
            var behaviour = GhostPlatoonBehaviour.Build(t, Owner, 4);
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
            var pointList = SpawnPointList.Where(x => x.Team == Owner.Team).ToList();

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

        public static void AddSpawnPoint(SpawnPointBehaviour s)
        {
            if (!SpawnPointList.Contains(s))
                SpawnPointList.Add(s);
        }

        public void ApplyHotkeys()
        {
            if (Commands.Unload) {
                Session.SelectionManager.DispatchUnloadCommand();

            } else if (Commands.Load) {
                Session.SelectionManager.DispatchLoadCommand();

            } else if (Commands.FirePos && !Session.SelectionManager.Empty) {
                EnterFirePosMode();
            }
        }

        private void EnterFirePosMode()
        {
            CurMouseMode = MouseMode.firePos;
            Vector2 hotspot = new Vector2(_firePosReticle.width / 2, _firePosReticle.height / 2);
            Cursor.SetCursor(_firePosReticle, hotspot, CursorMode.Auto);
        }

        private void ExitFirePosMode()
        {
            CurMouseMode = MouseMode.normal;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
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
    }
}