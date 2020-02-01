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
using System.Linq;
using UnityEngine;

using PFW.Model.Game;
using static PFW.UI.Ingame.InputManager;
using PFW.Units;
using PFW.Units.Component.Movement;

namespace PFW.UI.Ingame
{

    /**
     * Responsible for the set of selected units.
     *
     * Tracks which units are currently selected, adds and removes
     * units from the selection, dispatches orders to the selected units.
     */
    public class SelectionManager
    {
        private List<PlatoonBehaviour> _selection;

        public bool Empty => _selection.Count == 0;

        private Vector3 _mouseStart;
        private Vector3 _mouseEnd;
        private Texture2D _texture;
        private Texture2D _borderTexture;
        private Color _selectionBoxColor = Color.red;
        private bool _active;

        private ClickManager _clickManager;

        // State for managing move order previews:
        private Vector3 _previewPosition;
        private bool _makePreviewVisible;
        // END state for managing move order previews:

        private MouseMode _mouseMode;

        public ICollection<PlatoonBehaviour> AllPlatoons { get; }
            = new List<PlatoonBehaviour>();

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

        private void Start()
        {
            
        }

        public void UpdateMouseMode(MouseMode mouseMode)
        {
            _mouseMode = mouseMode;

            if (_mouseMode == MouseMode.NORMAL)
                _clickManager.Update();
        }

        public void DispatchFirePosCommand()
        {
            if (!Util.GetTerrainClickLocation(out RaycastHit hit))
                return;

            bool shouldQueue = Input.GetKey(KeyCode.LeftShift);
            foreach (var platoon in _selection)
            {
                platoon.SendFirePosOrder(hit.point, shouldQueue);
            }
        }

        public void DispatchUnloadCommand()
        {
            // TODO
        }

        public void DispatchLoadCommand()
        {
            // TODO
        }

        /**
         * Send a movement command to the currently selected platoons.
         *
         * \param useGhostHeading If true, the platoons will move to their sillhouettes
         * (e.g. the command was previewed using mouse drag and the units should move to
         * the positions that were shown in the preview). If false, the platoons
         * will just pick their destinations based on where the cursor is.
         */
        public void DispatchMoveCommand(bool useGhostHeading, MoveCommandType moveMode)
        {
            if (Empty)
                return;

            bool shouldQueue = Input.GetKey(KeyCode.LeftShift);

            _selection.ForEach(x => x.SetDestination(
                x.GhostPlatoon.transform.position,
                useGhostHeading ? 
                    x.GhostPlatoon.FinalHeading : MovementComponent.NO_HEADING,
                moveMode,
                shouldQueue));

            // A random platoon in selection plays the move command voice line
            int randInt = Random.Range(0, _selection.Count);
            _selection[randInt].PlayMoveCommandVoiceline();

            MaybeDropSelectionAfterOrder();
        }

        /**
         * Send a split command to all currently selected platoons
         */
        public void DispatchSplitCommand(PlayerData owner)
        {
            // Work on a shallow copy, because the actual selection gets changed
            // every time a platoon in it is destroyed (split):
            List<PlatoonBehaviour> selectionCopy = new List<PlatoonBehaviour>(_selection);

            UnselectAll(_selection, false);

            selectionCopy.ForEach(p => p.gameObject.GetComponentInParent<PlatoonRoot>().Split());
        }

        public void MaybeDropSelectionAfterOrder()
        {
            if (!Input.GetKey(KeyCode.LeftShift) && !Options.StickySelection)
                UnselectAll(_selection, false);
        }

        private void StartBoxSelection()
        {
            _mouseStart = Input.mousePosition;
            _active = false;
        }

        private void UpdateBoxSelection()
        {
            _mouseEnd = Input.mousePosition;
            UpdateSelection(false);
            _active = true;
        }

        private void EndDrag()
        {
            _active = false;
            UpdateSelection(true);
        }

        private void OnSelectShortClick()
        {
            UnselectAll(_selection, false);

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Selectable"), QueryTriggerInteraction.Ignore)) {
                var go = hit.transform.gameObject;
                var selectable = go.GetComponent<SelectableBehavior>();

                if (selectable != null) {
                    var selectedPlatoon = selectable.Platoon;
                    selectedPlatoon.PlaySelectionVoiceline();
                    _selection.Add(selectedPlatoon);
                }
            }
            SetSelected(_selection, false);
        }

        private void UpdateSelection(bool finalizeSelection)
        {
            if (_mouseMode == MouseMode.FIRE_POS
                || _mouseMode == MouseMode.FAST_MOVE
                || _mouseMode == MouseMode.REVERSE_MOVE)
                return;

            List<PlatoonBehaviour> newSelection = AllPlatoons.Where(x => IsInside(x)).ToList();
            if (!Input.GetKey(KeyCode.LeftShift) && _selection != null && _selection.Count != 0) {
                List<PlatoonBehaviour> old = _selection.Except(newSelection).ToList();
                UnselectAll(old, !finalizeSelection);
            }
            SetSelected(newSelection, !finalizeSelection);
            _selection = newSelection;
        }

        private bool IsInside(PlatoonBehaviour obj)
        {
            var platoon = obj.GetComponent<PlatoonBehaviour>();
            if (!platoon.IsInitialized)
                return false;

            bool inside = false;
            inside |= platoon.Units.Any(x => IsInside(x.Transform.position));

            // TODO: This checks if the center of the icon is within the selection box. 
            // It should instead check if any of the four corners of the icon are within the box:
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

        private void UnselectAll(List<PlatoonBehaviour> selectedPlatoons, bool justPreviewing)
        {
            selectedPlatoons.ForEach(x => x.SetSelected(false, justPreviewing));

            selectedPlatoons.Clear();
        }

        private void SetSelected(List<PlatoonBehaviour> selectedPlatoons, bool justPreviewing)
        {
            selectedPlatoons.ForEach(x => x.SetSelected(true, justPreviewing));

            // Randomly choose one platoon to play a selected voiceline
            if (selectedPlatoons.Count != 0 && !justPreviewing) {
                int randInt = Random.Range(0, selectedPlatoons.Count);
                selectedPlatoons[randInt].PlaySelectionVoiceline();
            }
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

            //Prevent short clicks from displaying preview by only showing it on the first call to RotateMoveOrderPreview call. Should maybe move the logic to UIManager, since it should be responsible for recognizing hold clicks, not this code.
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
                ghosts[i].SetPositionAndOrientation(positions[i], heading);
            }

            if (makeVisible) {
                ghosts.ForEach(x => x.SetVisible(true));
            }
        }

        public void RegisterPlatoonBirth(PlatoonBehaviour platoon)
        {
            AllPlatoons.Add(platoon);
        }

        public void RegisterPlatoonDeath(PlatoonBehaviour platoon)
        {
            _selection.Remove(platoon);
            AllPlatoons.Remove(platoon);
        }
    }
}