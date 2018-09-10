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

using PFW.Model.Game;
using static PFW.Ingame.UI.InputManager;
using System.Linq;

namespace PFW.Ingame.UI
{
    public class SelectionManager : MonoBehaviour
    {
        private List<PlatoonBehaviour> _selection;
        public bool Empty {
            get {
                return _selection.Count == 0;
            }
        }

        private Vector3 _mouseStart;
        private Vector3 _mouseEnd;
        private Texture2D _texture;
        private Texture2D _borderTexture;
        private Color _selectionBoxColor = Color.red;
        private bool _active;

        private ClickManager _clickManager;

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

        // State for managing move order previews:
        private Vector3 _previewPosition;
        private bool _makePreviewVisible;
        // END state for managing move order previews:

        public void Awake()
        {
            _selection = new List<PlatoonBehaviour>();
            _clickManager = new ClickManager(0, StartBoxSelection, OnSelectShortClick, EndDrag, UpdateBoxSelection);

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
            // disgustingly tight coupling:
            if (Session.CurrentMouseMode != MouseMode.normal
                && Session.CurrentMouseMode != MouseMode.firePos)
                return;

            _clickManager.Update();
        }

        public void DispatchFirePosCommand()
        {
            RaycastHit hit;
            if (!Util.GetTerrainClickLocation(out hit))
                return;

            foreach (var platoon in _selection)
                platoon.SendFirePosOrder(hit.point);
        }

        public void DispatchUnloadCommand()
        {
            foreach (var t in _selection.ConvertAll(x => x.Transporter).Where((x, i) => x != null)) {
                t.BeginQueueing(Input.GetKey(KeyCode.LeftShift));
                t.Unload();
                t.EndQueueing();
            }
        }

        public void DispatchLoadCommand()
        {
            var transporters = _selection.ConvertAll(x => x.Transporter).Where((x, i) => x != null).Where(x => x.transported == null).ToList();
            var infantry = _selection.ConvertAll(x => x.Transportable).Where((x, i) => x != null).ToList();

            transporters.ForEach(x => x.BeginQueueing(Input.GetKey(KeyCode.LeftShift)));
            transporters.ConvertAll(x => x as Matchable<TransportableModule>).Match(infantry);
            transporters.ForEach(x => x.EndQueueing());
        }

        public void DispatchMoveCommand(bool useGhostHeading)
        {
            PrepareDestination();

            if (useGhostHeading) {
                _selection.ForEach(x => x.Movement.GetHeadingFromGhost());
            } else {
                _selection.ForEach(x => x.Movement.UseDefaultHeading());
            }

            _selection.ForEach(x => x.Movement.EndQueueing());

            MaybeDropSelectionAfterOrder();
        }

        public void PrepareDestination() {
            var destinations = _selection.ConvertAll(x => x.GhostPlatoon.transform.position);
            bool shouldQueue = Input.GetKey(KeyCode.LeftShift);
            _selection.ForEach(x => x.Movement.BeginQueueing(shouldQueue));
            _selection.ConvertAll(x => x.Movement as Matchable<Vector3>).Match(destinations);
        }

        public void MaybeDropSelectionAfterOrder()
        {
            if (!Input.GetKey(KeyCode.LeftShift) && !Options.StickySelection)
                UnselectAll(_selection);
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
            UnselectAll(_selection);

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Selectable"), QueryTriggerInteraction.Ignore)) {
                var go = hit.transform.gameObject;
                var selectable = go.GetComponent<SelectableBehavior>();

                if (selectable != null) {
                    var p = selectable.GetPlatoon();
                    if (p != null)
                        _selection.Add(selectable.GetPlatoon());
                }
            }

            SetSelected(_selection);
        }

        private void UpdateSelection()
        {
            if (Session.CurrentMouseMode == MouseMode.firePos)
                return;

            List<PlatoonBehaviour> newSelection = Session.AllPlatoons.Where(x => IsInside(x)).ToList();
            if (!Input.GetKey(KeyCode.LeftShift) && _selection != null) {
                List<PlatoonBehaviour> old = _selection.Except(newSelection).ToList();
                UnselectAll(old);
            }
            SetSelected(newSelection);
            _selection = newSelection;
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
        public void OnGUI()
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

        public void PrepareMoveOrderPreview(Vector3 position)
        {            
            Vector3 centerMass = _selection.ConvertAll(x => x as MonoBehaviour).getCenterOfMass();
            _previewPosition = position;
            PositionGhostUnits(2 * _previewPosition - centerMass, false);

            // Prevent short clicks from displaying preview by only showing it on the first call to RotateMoveOrderPreview call. Should maybe move the logic to UIManager, since it should be responsible for recognizing hold clicks, not this code.
            _makePreviewVisible = true;
        }
        
        public void RotateMoveOrderPreview(Vector3 facingPoint)
        {
            PositionGhostUnits(facingPoint, _makePreviewVisible);

            if (_makePreviewVisible)
                _makePreviewVisible = false;
        }

        private void PositionGhostUnits(Vector3 facingPoint, bool makeVisible)
        {
            Vector3 diff = facingPoint - _previewPosition;
            float heading = diff.getRadianAngle();

            /*Vector3 forward = new Vector3(Mathf.Cos(heading), 0, Mathf.Sin(heading));
            int formationWidth = _selection.Count;// Mathf.CeilToInt(2 * Mathf.Sqrt(spawnList.Count));
            float platoonDistance = 4 * PlatoonBehaviour.UNIT_DISTANCE;
            var right = Vector3.Cross(forward, Vector3.up);
            var pos = _previewPosition + platoonDistance * (formationWidth - 1) * right / 2f;*/

            var positions = Formations.GetLineFormation(_previewPosition, heading + Mathf.PI / 2, _selection.Count);
            List<GhostPlatoonBehaviour> ghosts = _selection.ConvertAll(x => x.GhostPlatoon);
            for (var i = 0; i < _selection.Count; i++) {
                ghosts[i].SetOrientation(positions[i], heading);
            }

            if (makeVisible) {
                ghosts.ForEach(x => x.SetVisible(true));
            }
        }

        public void RegisterPlatoonDeath(PlatoonBehaviour platoon) {
            _selection.Remove(platoon);
        }
    }
}