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
        private Vector3 _destination;
        private Vector3 _boxSelectStart;
        public static List<SpawnPointBehaviour> SpawnPointList = new List<SpawnPointBehaviour>();
        private ClickManager _rightClickManager;

        public enum MouseMode { normal, purchasing, firePos };
        public MouseMode CurMouseMode { get; private set; } = MouseMode.normal;

        private BuyTransaction _currentBuyTransaction;

        private GameSession _session;
        public GameSession Session {
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

                    ShowGhostUnits(hit);
                    MaybePurchaseGhostUnits(hit);
                }

                MaybeExitPurchasingMode();
                break;

            case MouseMode.normal:
                ApplyHotkeys();
                _rightClickManager.Update();
                break;

            case MouseMode.firePos:
                ApplyHotkeys();
                if (Input.GetMouseButtonDown(0)
                    || Input.GetMouseButtonDown(1))
                    ExitFirePosMode();
                break;

            default:
                throw new Exception("impossible state");
            }
        }

        private void ShowGhostUnits(RaycastHit terrainHover)
        {
            // Show ghost units under mouse:
            SpawnPointBehaviour closestSpawn = GetClosestSpawn(terrainHover.point);
            PositionGhostUnits(
                terrainHover.point,
                2 * terrainHover.point - closestSpawn.transform.position,
                _currentBuyTransaction.GhostUnits);
        }

        private void MaybePurchaseGhostUnits(RaycastHit terrainHover)
        {
            if (Input.GetMouseButtonUp(0)) {
                bool noUIcontrolsInUse = EventSystem.current.currentSelectedGameObject == null;

                if (!noUIcontrolsInUse)
                    return;

                if (_currentBuyTransaction == null)
                    return;

                // TODO we already get closest spawn above, reuse it
                SpawnPointBehaviour closestSpawn = GetClosestSpawn(terrainHover.point);
                closestSpawn.BuyUnits(_currentBuyTransaction.GhostUnits);

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
                foreach (var g in _currentBuyTransaction.GhostUnits)
                    g.GetComponent<GhostPlatoonBehaviour>().Destroy();

                ExitPurchasingMode();
            }
        }

        void OnOrderStart()
        {
            var selected = Session.SelectionManager.Selection;

            RaycastHit hit;
            if (Util.GetTerrainClickLocation(out hit)) {
                Vector3 com = selected.ConvertAll(x => x as MonoBehaviour).getCenterOfMass();
                List<GhostPlatoonBehaviour> ghosts = selected.ConvertAll<GhostPlatoonBehaviour>(x => x.GhostPlatoon);
                PositionGhostUnits(hit.point, 2 * hit.point - com, ghosts);
                _destination = hit.point;
            }
        }

        void OnOrderHold()
        {
            var selected = Session.SelectionManager.Selection;

            RaycastHit hit;
            if (Util.GetTerrainClickLocation(out hit)) {

                List<GhostPlatoonBehaviour> ghosts = selected.ConvertAll(x => x.GhostPlatoon);
                ghosts.ForEach(x => x.SetVisible(true));
                PositionGhostUnits(_destination, hit.point, ghosts);
            }
        }

        void OnOrderShortClick()
        {
            var selected = Session.SelectionManager.Selection;

            var destinations = selected.ConvertAll(x => x.GhostPlatoon.transform.position);
            var shift = Input.GetKey(KeyCode.LeftShift);
            selected.ForEach(x => x.Movement.BeginQueueing(shift));
            selected.ConvertAll(x => x.Movement as Matchable<Vector3>).Match(destinations);
            selected.ForEach(x => x.Movement.GetHeadingFromGhost());
            selected.ForEach(x => x.Movement.EndQueueing());
            //selected.ForEach(x => x.ghostPlatoon.setVisible(false));
            /*var destinations=selected.ConvertAll(x=>x.ghostPlatoon);
            foreach (var go in selected)
            {
                var behaviour = go.GetComponent<SelectableBehavior>().getPlatoon();
                behaviour.getDestinationFromGhost();
            }*/

            Session.SelectionManager.ChangeSelectionAfterOrder();
        }

        void OnOrderLongClick()
        {
            var selected = Session.SelectionManager.Selection;

            RaycastHit hit;
            if (Util.GetTerrainClickLocation(out hit)) {
                var shift = Input.GetKey(KeyCode.LeftShift);
                selected.ForEach(x => x.Movement.BeginQueueing(shift));
                var destinations = selected.ConvertAll(x => x.GhostPlatoon.transform.position);
                selected.ConvertAll(x => x.Movement as Matchable<Vector3>).Match(destinations);
                selected.ForEach(x => x.Movement.GetHeadingFromGhost());
                selected.ForEach(x => x.Movement.EndQueueing());
                /*foreach (var go in selected)
                {
                    go.GetComponent<SelectableBehavior>().getDestinationFromGhost();
                    go.GetComponent<PlatoonBehaviour>().ghostPlatoon.GetComponent<GhostPlatoonBehaviour>().setVisible(false);
                }*/

                Session.SelectionManager.ChangeSelectionAfterOrder();
            }
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
            _currentBuyTransaction.GhostUnits.Add(behaviour);
        }

        private static void PositionGhostUnits(Vector3 position, Vector3 facingPoint, List<GhostPlatoonBehaviour> units)
        {
            var diff = facingPoint - position;
            PositionGhostUnits(position, diff.getRadianAngle(), units);
        }

        private static void PositionGhostUnits(Vector3 position, float heading, List<GhostPlatoonBehaviour> units)
        {
            Vector3 forward = new Vector3(Mathf.Cos(heading), 0, Mathf.Sin(heading));
            int formationWidth = units.Count;// Mathf.CeilToInt(2 * Mathf.Sqrt(spawnList.Count));
            float unitDistance = 4 * PlatoonBehaviour.BaseDistance;
            var right = Vector3.Cross(forward, Vector3.up);
            var pos = position + unitDistance * (formationWidth - 1) * right / 2f;
            for (var i = 0; i < formationWidth; i++)
                units[i].GetComponent<GhostPlatoonBehaviour>().SetOrientation(pos - i * unitDistance * right, heading);
        }

        private void ExitPurchasingMode()
        {
            _currentBuyTransaction.GhostUnits.Clear();

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
            var selected = Session.SelectionManager.Selection;

            if (Commands.Unload()) {
                foreach (var t in selected.ConvertAll(x => x.Transporter).Where((x, i) => x != null)) {
                    t.BeginQueueing(Input.GetKey(KeyCode.LeftShift));
                    t.Unload();
                    t.EndQueueing();
                }

            } else if (Commands.Load()) {

                var transporters = selected.ConvertAll(x => x.Transporter).Where((x, i) => x != null).Where(x => x.transported == null).ToList();
                var infantry = selected.ConvertAll(x => x.Transportable).Where((x, i) => x != null).ToList();

                transporters.ForEach(x => x.BeginQueueing(Input.GetKey(KeyCode.LeftShift)));
                transporters.ConvertAll(x => x as Matchable<TransportableModule>).Match(infantry);
                transporters.ForEach(x => x.EndQueueing());

            } else if (Commands.FirePos() && selected.Count != 0) {
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
        public static bool Unload()
        {
            return Input.GetKeyDown(Hotkeys.Unload);
        }

        public static bool Load()
        {
            return Input.GetKeyDown(Hotkeys.Load);
        }

        public static bool FirePos()
        {
            return Input.GetKeyDown(Hotkeys.FirePos);
        }
    }
}