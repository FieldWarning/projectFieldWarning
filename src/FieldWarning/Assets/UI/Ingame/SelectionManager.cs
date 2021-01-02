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

using static PFW.UI.Ingame.InputManager;
using PFW.Model.Match;
using PFW.Networking;
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
        private PlayerData _localPlayer;
        public PlayerData LocalPlayer {
            set {
                _localPlayer = value;
            }
        }

        private List<PlatoonBehaviour> _selection;

        public bool Empty => _selection.Count == 0;

        private Vector3 _mouseStart;
        private Vector3 _mouseEnd;
        private Texture2D _texture;
        private Texture2D _borderTexture;
        private Color _selectionBoxColor = Color.red;
        private bool _active;

        /// <summary>
        /// A left click usually makes us drops the selection.
        /// Selecting a platoon by clicking on its label
        /// is also a left click that triggers the drop logic;
        /// to avoid dropping a platoon we just selected, we
        /// use this variable.
        /// </summary>
        private bool _justSelected = false;

        private ClickManager _clickManager;

        // State for managing move order previews:
        private Vector3 _previewPosition;
        private bool _makePreviewVisible;
        // END state for managing move order previews:

        private MouseMode _mouseMode;

        public ICollection<PlatoonBehaviour> AllPlatoons { get; }
            = new List<PlatoonBehaviour>();

        private SelectionPane _selectionPane;

        public SelectionManager(SelectionPane selectionPane, PlayerData localPlayer)
        {
            _localPlayer = localPlayer;
            _selectionPane = selectionPane;
            _selection = new List<PlatoonBehaviour>();
            _clickManager = new ClickManager(
                    0, 
                    StartBoxSelection, 
                    OnSelectShortClick, 
                    EndDrag, 
                    UpdateBoxSelection);

            if (_texture == null) 
            {
                float areaTransparency = .95f;
                float borderTransparency = .75f;
                _texture = new Texture2D(1, 1);
                _texture.wrapMode = TextureWrapMode.Repeat;
                _texture.SetPixel(
                        0, 0, _selectionBoxColor - areaTransparency * Color.black);
                _texture.Apply();
                _borderTexture = new Texture2D(1, 1);
                _borderTexture.wrapMode = TextureWrapMode.Repeat;
                _borderTexture.SetPixel(
                        0, 0, _selectionBoxColor - borderTransparency * Color.black);
                _borderTexture.Apply();
            }
        }

        public void UpdateMouseMode(MouseMode mouseMode)
        {
            _mouseMode = mouseMode;

            if (_mouseMode == MouseMode.NORMAL || _mouseMode == MouseMode.NORMAL_COVER)
                _clickManager.Update();
        }

        public void DispatchFirePosCommand()
        {
            if (!Util.GetTerrainClickLocation(out RaycastHit hit))
                return;

            bool shouldQueue = Input.GetKey(KeyCode.LeftShift);
            foreach (PlatoonBehaviour platoon in _selection)
            {
                platoon.SendFirePosOrder(hit.point, shouldQueue);
            }
        }

        public void DispatchUnloadCommand()
        {
            Logger.LogWithoutSubsystem(LogLevel.BUG, "Unload not implemented");
        }

        public void DispatchLoadCommand()
        {
            Logger.LogWithoutSubsystem(LogLevel.BUG, "Load not implemented");
        }

        public void DispatchStopCommand()
        {
            _selection.ForEach(x => CommandConnection.Connection.CmdCancelOrders(
                x.netId
                )
            );
        }

        public void DispatchToggleWeaponsCommand()
        {
            Logger.LogWithoutSubsystem(LogLevel.BUG, "Weapon toggling not implemented");
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

            _selection.ForEach(x => CommandConnection.Connection.CmdOrderMovement(
                x.netId,
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
        public void DispatchSplitCommand()
        {
            // Work on a shallow copy, because the actual selection gets changed
            // every time a platoon in it is destroyed (split):
            List<PlatoonBehaviour> selectionCopy = new List<PlatoonBehaviour>(_selection);

            UnselectAll(_selection, false);

            selectionCopy.ForEach(
                    p => CommandConnection.Connection.CmdSplitPlatoon(p.netId));
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

        /// <summary>
        ///     When a platoon's label is left clicked we need to 
        ///     add it to the selection. These clicks are detected
        ///     by a button callback handler, so the notification
        ///     unfortunately has to come from the outside..
        /// </summary>
        public void PlatoonLabelClicked(PlatoonBehaviour selectedPlatoon)
        {
            if (selectedPlatoon.Owner != _localPlayer)
                return;

            UnselectAll(_selection, false);
            selectedPlatoon.PlaySelectionVoiceline();
            _selection.Add(selectedPlatoon);
            SetSelected(_selection, false);
            _justSelected = true;
        }

        private void OnSelectShortClick()
        {
            if (_justSelected)
            {
                _justSelected = false;
            }
            else
            {
                UnselectAll(_selection, false);
            }
        }

        private void UpdateSelection(bool finalizeSelection)
        {
            if (_mouseMode == MouseMode.FIRE_POS
                || _mouseMode == MouseMode.FAST_MOVE
                || _mouseMode == MouseMode.REVERSE_MOVE)
                return;

            List<PlatoonBehaviour> newSelection = AllPlatoons.Where(
                    x => x.Owner == _localPlayer
                         && IsInsideSelectionBox(x)).ToList();
            if (!Input.GetKey(KeyCode.LeftShift) 
                && _selection != null 
                && _selection.Count != 0)
            {
                List<PlatoonBehaviour> old = _selection.Except(newSelection).ToList();
                UnselectAll(old, !finalizeSelection);
            }
            SetSelected(newSelection, !finalizeSelection);
            _selection = newSelection;
        }

        private bool IsInsideSelectionBox(PlatoonBehaviour obj)
        {
            PlatoonBehaviour platoon = obj.GetComponent<PlatoonBehaviour>();
            if (!platoon.IsInitialized)
                return false;

            Rect selectionBox;
            // Guarantee that the rect has positive width and height:
            float rectX = _mouseStart.x > _mouseEnd.x ? _mouseEnd.x : _mouseStart.x;
            float rectY = _mouseStart.y > _mouseEnd.y ? _mouseEnd.y : _mouseStart.y;
            selectionBox = new Rect(
                    rectX,
                    rectY,
                    Mathf.Abs(_mouseEnd.x - _mouseStart.x),
                    Mathf.Abs(_mouseEnd.y - _mouseStart.y));

            bool inside = false;
            inside |= platoon.Units.Any(
                    x => selectionBox.Contains(
                            Camera.main.WorldToScreenPoint(
                                    x.Transform.position)));

            // This checks if the icon overlaps with the selection box:
            Rect platoonLabel = platoon.SelectableRect.rect;
            // To screen coordinates:
            platoonLabel.center = platoon.SelectableRect.TransformPoint(
                    platoonLabel.center);
            platoonLabel.size = platoon.SelectableRect.TransformVector(
                    platoonLabel.size);
            inside |= selectionBox.Overlaps(platoonLabel);

            return inside;
        }

        public PlatoonBehaviour FindPlatoonAtCursor()
        {
            foreach (PlatoonBehaviour platoon in AllPlatoons)
            {
                // This checks if the icon is under the cursor:
                Rect platoonLabel = platoon.SelectableRect.rect;
                // To screen coordinates:
                platoonLabel.center = platoon.SelectableRect.TransformPoint(
                        platoonLabel.center);
                platoonLabel.size = platoon.SelectableRect.TransformVector(
                        platoonLabel.size);
                if (platoonLabel.Contains(Input.mousePosition))
                    return platoon;
            }
            return null;
        }

        private void UnselectAll(
                List<PlatoonBehaviour> selectedPlatoons, bool justPreviewing)
        {
            selectedPlatoons.ForEach(x => x.SetSelected(false, justPreviewing));

            selectedPlatoons.Clear();
            _selectionPane.OnSelectionCleared();
        }

        private void SetSelected(
                List<PlatoonBehaviour> selectedPlatoons, bool justPreviewing)
        {
            selectedPlatoons.ForEach(x => x.SetSelected(true, justPreviewing));

            if (selectedPlatoons.Count != 0 && !justPreviewing)
            {
                // Randomly choose one platoon to play a selected voiceline
                int randInt = Random.Range(0, selectedPlatoons.Count);
                selectedPlatoons[randInt].PlaySelectionVoiceline();

                _selectionPane.OnSelectionChanged(selectedPlatoons);
            }
        }

        // Responsible for drawing the selection rectangle
        public void OnGUI()
        {
            if (_active) 
            {
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

            // Prevent short clicks from displaying preview by only 
            // showing it on the first call to RotateMoveOrderPreview. 
            // Should maybe move the logic to InputManager, since it should
            // be responsible for recognizing hold clicks, not this code.
            _makePreviewVisible = true;
        }

        public void RotateMoveOrderPreview(Vector3 facingPoint)
        {
            PositionGhostUnits(facingPoint, _makePreviewVisible);

            if (_makePreviewVisible)
                _makePreviewVisible = false;
        }

        public void HideMoveOrderPreview()
        {
            _selection.ForEach(x => x.GhostPlatoon.SetVisible(false));
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

            List<Vector3> positions = Formations.GetLineFormation(
                    _previewPosition, heading + Mathf.PI / 2, _selection.Count);
            List<GhostPlatoonBehaviour> ghosts = _selection.ConvertAll(
                    x => x.GhostPlatoon);
            for (int i = 0; i < _selection.Count; i++) 
            {
                ghosts[i].SetPositionAndOrientation(positions[i], heading);
            }

            if (makeVisible) 
            {
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

        /// <summary>
        /// Place the targeting preview and get the distance
        /// from the closest unit to the target, in meters.
        /// </summary>
        public int PlaceTargetingPreview(Vector3 targetPosition, bool respectMaxRange)
        {
            int minRange = 99999;
            foreach (PlatoonBehaviour platoon in _selection)
            {
                int range = platoon.PlaceTargetingPreview(
                        targetPosition, respectMaxRange);
                if (range < minRange)
                {
                    minRange = range;
                }
            }
            return minRange;
        }

        /// <summary>
        /// The targeting preview is a line drawn from a unit
        /// to the cursor location with range and line of sight hints.
        /// </summary>
        public void ToggleTargetingPreview(bool enabled)
        {
            _selection.ForEach(x => x.ToggleTargetingPreview(enabled));
        }
    }
}
