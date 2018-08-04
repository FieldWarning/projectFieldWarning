﻿/**
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

namespace Assets.Ingame.UI
{
    public class UIManagerBehaviour : MonoBehaviour
    {
        [SerializeField]
        private float _mouseDragThreshold = 10.0f;
        [SerializeField]
        private Texture2D _firePosReticle;

        // Use this for initialization
        public Player Owner;
        private Vector3 _destination;
        private Vector3 _boxSelectStart;
        public static Dictionary<Team, List<SpawnPointBehaviour>> SpawnPointList = new Dictionary<Team, List<SpawnPointBehaviour>>();
        private ClickManager _rightClickManager;
        private static SelectionManager _selectionManager;

        private enum MouseMode { normal, purchasing, firePos };
        private MouseMode _mouseMode = MouseMode.normal;

        private BuyTransaction _currentBuyTransaction;


        void Start()
        {
            if (_firePosReticle == null)
                throw new Exception("No fire pos reticle specified!");

            _selectionManager = new SelectionManager(this, 0, _mouseDragThreshold);

            _rightClickManager = new ClickManager(1, _mouseDragThreshold, OnOrderStart, OnOrderShortClick, OnOrderLongClick, OnOrderHold);
        }

        void Update()
        {
            switch (_mouseMode) {

            case MouseMode.purchasing:

                RaycastHit hit;
                if (GetTerrainClickLocation(out hit)
                    && hit.transform.gameObject.name.Equals("Terrain")) {

                    ShowGhostUnits(hit);
                    MaybePurchaseGhostUnits(hit);
                }

                MaybeExitPurchasingMode();
                break;

            case MouseMode.normal:
                ApplyHotkeys();
                _selectionManager?.Update();
                _rightClickManager.Update();
                break;

            case MouseMode.firePos:
                ApplyHotkeys();
                _selectionManager?.Update();
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
                closestSpawn.buyUnits(_currentBuyTransaction.GhostUnits);

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

        void OnGUI()
        {
            _selectionManager?.OnGui();
        }

        void OnOrderStart()
        {
            var selected = _selectionManager.Selection;

            RaycastHit hit;
            if (GetTerrainClickLocation(out hit)) {
                Vector3 com = selected.ConvertAll(x => x as MonoBehaviour).getCenterOfMass();
                List<GhostPlatoonBehaviour> ghosts = selected.ConvertAll<GhostPlatoonBehaviour>(x => x.GhostPlatoon);
                PositionGhostUnits(hit.point, 2 * hit.point - com, ghosts);
                _destination = hit.point;
            }
        }

        void OnOrderHold()
        {
            var selected = _selectionManager.Selection;

            RaycastHit hit;
            if (GetTerrainClickLocation(out hit)) {

                List<GhostPlatoonBehaviour> ghosts = selected.ConvertAll(x => x.GhostPlatoon);
                ghosts.ForEach(x => x.SetVisible(true));
                PositionGhostUnits(_destination, hit.point, ghosts);
            }
        }

        void OnOrderShortClick()
        {
            var selected = _selectionManager.Selection;

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

            _selectionManager.ChangeSelectionAfterOrder();
        }

        void OnOrderLongClick()
        {
            var selected = _selectionManager.Selection;

            RaycastHit hit;
            if (GetTerrainClickLocation(out hit)) {
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

                _selectionManager.ChangeSelectionAfterOrder();
            }
        }

        public void TankButtonCallback()
        {
            if (_currentBuyTransaction == null)
                _currentBuyTransaction = new BuyTransaction(UnitType.Tank, Owner);
            else
                _currentBuyTransaction.AddUnit();

            //buildUnit(UnitType.Tank);
            _mouseMode = MouseMode.purchasing;
        }

        public void InfantryButtonCallback()
        {
            BuildUnit(UnitType.Infantry);
            _mouseMode = MouseMode.purchasing;
        }

        public void AFVButtonCallback()
        {
            BuildUnit(UnitType.AFV);
            _mouseMode = MouseMode.purchasing;
        }

        public void BuildUnit(UnitType t)
        {
            var behaviour = GhostPlatoonBehaviour.Build(t, Owner, 4);
            _currentBuyTransaction.GhostUnits.Add(behaviour);
        }

        public static void RegisterPlatoonBirth(PlatoonBehaviour platoon)
        {
            _selectionManager.AllUnits.Add(platoon);
        }

        public static void RegisterPlatoonDeath(PlatoonBehaviour platoon)
        {
            _selectionManager.AllUnits.Remove(platoon);
            _selectionManager.Selection.Remove(platoon);
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

            _mouseMode = MouseMode.normal;
        }

        private SpawnPointBehaviour GetClosestSpawn(Vector3 p)
        {
            var pointList = SpawnPointList[Owner.getTeam()];
            SpawnPointBehaviour go = pointList[0];
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
            if (!SpawnPointList.ContainsKey(s.team)) {
                SpawnPointList.Add(s.team, new List<SpawnPointBehaviour>());
            }
            SpawnPointList[s.team].Add(s);
        }

        public void ApplyHotkeys()
        {
            var selected = _selectionManager.Selection;

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
                _mouseMode = MouseMode.firePos;
                Cursor.SetCursor(_firePosReticle, Vector2.zero, CursorMode.Auto);
            }
        }

        private void EnterFirePosMode()
        {
            _mouseMode = MouseMode.firePos;
            Cursor.SetCursor(_firePosReticle, Vector2.zero, CursorMode.Auto);
        }

        private void ExitFirePosMode()
        {
            _mouseMode = MouseMode.normal;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        static bool GetTerrainClickLocation(out RaycastHit hit)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Terrain"), QueryTriggerInteraction.Ignore);
        }

        private class SelectionManager
        {
            public List<PlatoonBehaviour> AllUnits = new List<PlatoonBehaviour>();
            public List<PlatoonBehaviour> Selection { get; private set; }

            private Vector3 _mouseStart;
            private Vector3 _mouseEnd;
            private Texture2D _texture;
            private Texture2D _borderTexture;
            private Color _selectionBoxColor = Color.red;
            private bool _active;

            private ClickManager _clickManager;
            private UIManagerBehaviour _outer;

            public SelectionManager(UIManagerBehaviour outer, int button, float mouseDragThreshold)
            {
                _outer = outer;
                Selection = new List<PlatoonBehaviour>();
                _clickManager = new ClickManager(button, mouseDragThreshold, StartBoxSelection, OnSelectShortClick, EndDrag, UpdateBoxSelection);

                if (_texture == null) {
                    var areaTransparency = .95f;
                    var borderTransparency = .75f;
                    _texture = new Texture2D(1, 1);
                    _texture.wrapMode = TextureWrapMode.Repeat;
                    _texture.SetPixel(0, 0, _selectionBoxColor - areaTransparency * Color.black);
                    _texture.Apply();
                    _borderTexture = new Texture2D(1, 1);
                    _borderTexture.wrapMode = TextureWrapMode.Repeat;
                    _borderTexture.SetPixel(0, 0, _selectionBoxColor - borderTransparency * Color.black);
                    _borderTexture.Apply();
                }
            }

            public void Update()
            {
                _clickManager.Update();

                if (_outer._mouseMode == MouseMode.firePos && Input.GetMouseButtonDown(0)) {
                    RaycastHit hit;
                    GetTerrainClickLocation(out hit);

                    foreach (var platoon in Selection) {
                        platoon.SendFirePosOrder(hit.point);
                    }
                }
            }

            public void ChangeSelectionAfterOrder()
            {
                if (!Input.GetKey(KeyCode.LeftShift) && !Options.StickySelection)
                    UnselectAll(Selection);
            }

            private void StartBoxSelection()
            {
                _mouseStart = Input.mousePosition;
                _active = false;
            }

            private void UpdateBoxSelection()
            {
                _mouseEnd = Input.mousePosition;
                UpdateSelection();
                _active = true;
            }

            private void EndDrag()
            {
                _active = false;
                UpdateSelection();
            }

            private void OnSelectShortClick()
            {
                UnselectAll(Selection);

                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Selectable"), QueryTriggerInteraction.Ignore)) {
                    var go = hit.transform.gameObject;
                    var selectable = go.GetComponent<SelectableBehavior>();

                    if (selectable != null)
                        Selection.Add(selectable.GetPlatoon());
                }

                SetSelected(Selection);
            }

            private void UpdateSelection()
            {
                if (_outer._mouseMode == MouseMode.firePos)
                    return;

                List<PlatoonBehaviour> newSelection = AllUnits.Where(x => IsInside(x)).ToList();
                if (!Input.GetKey(KeyCode.LeftShift) && Selection != null) {
                    List<PlatoonBehaviour> old = Selection.Except(newSelection).ToList();
                    UnselectAll(old);
                }
                SetSelected(newSelection);
                Selection = newSelection;
            }

            private bool IsInside(PlatoonBehaviour obj)
            {
                var platoon = obj.GetComponent<PlatoonBehaviour>();
                if (!platoon.IsInitialized)
                    return false;

                bool inside = false;
                inside |= platoon.Units.Any(x => IsInside(x.transform.position));

                // TODO: This checks if the center of the icon is within the selection box. It should instead check if any of the four corners of the icon are within the box:
                inside |= IsInside(platoon.Icon.transform.GetChild(0).position);
                return inside;
            }

            private bool IsInside(Vector3 t)
            {
                Vector3 test = Camera.main.WorldToScreenPoint(t);
                bool insideX = (test.x - _mouseStart.x) * (test.x - _mouseEnd.x) < 0;
                bool insideY = (test.y - _mouseStart.y) * (test.y - _mouseEnd.y) < 0;
                return insideX && insideY;
            }

            private void UnselectAll(List<PlatoonBehaviour> l)
            {
                l.ForEach(x => x.SetSelected(false));
                l.Clear();
            }

            private void SetSelected(List<PlatoonBehaviour> l)
            {
                l.ForEach(x => x.SetSelected(true));
            }

            // Responsible for drawing the selection rectangle
            public void OnGui()
            {
                if (_active) {
                    float lineWidth = 3;
                    float startX = _mouseStart.x;
                    float endX = _mouseEnd.x;
                    float startY = Screen.height - _mouseStart.y;
                    float endY = Screen.height - _mouseEnd.y;

                    Rect leftEdge = new Rect(startX - lineWidth / 2, startY + lineWidth / 2, lineWidth, endY - startY - lineWidth);
                    Rect rightEdge = new Rect(endX - lineWidth / 2, startY + lineWidth / 2, lineWidth, endY - startY - lineWidth);
                    Rect topEdge = new Rect(startX + lineWidth / 2, startY - lineWidth / 2, endX - startX - lineWidth, lineWidth);
                    Rect bottomEdge = new Rect(startX + lineWidth / 2, endY - lineWidth / 2, endX - startX - lineWidth, lineWidth);
                    Rect area = new Rect(startX + lineWidth / 2, startY + lineWidth / 2, endX - startX - lineWidth, endY - startY - lineWidth);
                    GUI.DrawTexture(area, _texture, ScaleMode.StretchToFill, true);
                    GUI.DrawTexture(leftEdge, _borderTexture, ScaleMode.StretchToFill, true);
                    GUI.DrawTexture(rightEdge, _borderTexture, ScaleMode.StretchToFill, true);
                    GUI.DrawTexture(topEdge, _borderTexture, ScaleMode.StretchToFill, true);
                    GUI.DrawTexture(bottomEdge, _borderTexture, ScaleMode.StretchToFill, true);
                }
            }
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