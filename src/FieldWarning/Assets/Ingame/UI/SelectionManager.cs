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

using Assets.Model.Game;
using static Assets.Ingame.UI.UIManagerBehaviour;
using System.Linq;

using static Assets.Ingame.UI.Constants;

namespace Assets.Ingame.UI
{
    public class SelectionManager : MonoBehaviour
    {
        public List<PlatoonBehaviour> Selection { get; private set; }

        private Vector3 _mouseStart;
        private Vector3 _mouseEnd;
        private Texture2D _texture;
        private Texture2D _borderTexture;
        private Color _selectionBoxColor = Color.red;
        private bool _active;

        private ClickManager _clickManager;

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

        public void Awake()
        {
            Selection = new List<PlatoonBehaviour>();
            _clickManager = new ClickManager(0, MOUSE_DRAG_THRESHOLD, StartBoxSelection, OnSelectShortClick, EndDrag, UpdateBoxSelection);

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
            if (Session.UIManager.CurMouseMode != MouseMode.normal
                && Session.UIManager.CurMouseMode != MouseMode.firePos)
                return;

            _clickManager.Update();

            if (Session.UIManager.CurMouseMode == MouseMode.firePos && Input.GetMouseButtonDown(0)) {
                RaycastHit hit;
                Util.GetTerrainClickLocation(out hit);

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
            if (Session.UIManager.CurMouseMode == MouseMode.firePos)
                return;

            List<PlatoonBehaviour> newSelection = Session.AllPlatoons.Where(x => IsInside(x)).ToList();
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
    }
}