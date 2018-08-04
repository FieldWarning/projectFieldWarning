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
using Assets.Ingame.UI;
using UnityEngine.EventSystems;
using Pfw.Ingame.Prototype;

public class UIManagerBehaviour : MonoBehaviour
{
    [SerializeField]
    private float mouseDragThreshold = 10.0f;
    [SerializeField]
    private Texture2D firePosReticle;

    // Use this for initialization
    public Player owner;
    private Vector3 destination;
    private Vector3 boxSelectStart;
    public static Dictionary<Team, List<SpawnPointBehaviour>> spawnPointList = new Dictionary<Team, List<SpawnPointBehaviour>>();
    private ClickManager rightClickManager;
    private static SelectionManager selectionManager;

    private enum MouseMode { normal, purchasing, firePos };
    private MouseMode mouseMode = MouseMode.normal;

    private BuyTransaction _currentBuyTransaction;


    void Start()
    {
        if (firePosReticle == null)
            throw new Exception("No fire pos reticle specified!");

        selectionManager = new SelectionManager(this, 0, mouseDragThreshold);

        rightClickManager = new ClickManager(1, mouseDragThreshold, onOrderStart, onOrderShortClick, onOrderLongClick, onOrderHold);
    }

    void Update()
    {
        switch (mouseMode) {

        case MouseMode.purchasing:

            RaycastHit hit;
            if (getTerrainClickLocation(out hit)
                && hit.transform.gameObject.name.Equals("Terrain")) {

                showGhostUnits(hit);
                maybePurchaseGhostUnits(hit);
            }

            maybeExitPurchasingMode();
            break;

        case MouseMode.normal:
            applyHotkeys();
            selectionManager?.Update();
            rightClickManager.Update();
            break;

        case MouseMode.firePos:
            applyHotkeys();
            selectionManager?.Update();
            if (Input.GetMouseButtonDown(0)
                || Input.GetMouseButtonDown(1))
                exitFirePosMode();
            break;

        default:
            throw new Exception("impossible state");
        }
    }

    private void showGhostUnits(RaycastHit terrainHover)
    {
        // Show ghost units under mouse:
        SpawnPointBehaviour closestSpawn = getClosestSpawn(terrainHover.point);
        positionGhostUnits(
            terrainHover.point,
            2 * terrainHover.point - closestSpawn.transform.position,
            _currentBuyTransaction.GhostUnits);
    }

    private void maybePurchaseGhostUnits(RaycastHit terrainHover)
    {
        if (Input.GetMouseButtonUp(0)) {
            bool noUIcontrolsInUse = EventSystem.current.currentSelectedGameObject == null;

            if (!noUIcontrolsInUse)
                return;

            if (_currentBuyTransaction == null)
                return;

            // TODO we already get closest spawn above, reuse it
            SpawnPointBehaviour closestSpawn = getClosestSpawn(terrainHover.point);
            closestSpawn.buyUnits(_currentBuyTransaction.GhostUnits);

            if (Input.GetKey(KeyCode.LeftShift)) {
                // We turned the current ghosts into real units, so:
                _currentBuyTransaction = _currentBuyTransaction.Clone();
            } else {
                exitPurchasingMode();
            }
        }
    }

    private void maybeExitPurchasingMode()
    {
        if (Input.GetMouseButton(1)) {
            foreach (var g in _currentBuyTransaction.GhostUnits)
                g.GetComponent<GhostPlatoonBehaviour>().Destroy();

            exitPurchasingMode();
        }
    }

    void OnGUI()
    {
        selectionManager?.OnGui();
    }

    void onOrderStart()
    {
        var selected = selectionManager.selection;

        RaycastHit hit;
        if (getTerrainClickLocation(out hit)) {
            Vector3 com = selected.ConvertAll(x => x as MonoBehaviour).getCenterOfMass();
            List<GhostPlatoonBehaviour> ghosts = selected.ConvertAll<GhostPlatoonBehaviour>(x => x.GhostPlatoon);
            positionGhostUnits(hit.point, 2 * hit.point - com, ghosts);
            destination = hit.point;
        }
    }

    void onOrderHold()
    {
        var selected = selectionManager.selection;

        RaycastHit hit;
        if (getTerrainClickLocation(out hit)) {

            List<GhostPlatoonBehaviour> ghosts = selected.ConvertAll(x => x.GhostPlatoon);
            ghosts.ForEach(x => x.SetVisible(true));
            positionGhostUnits(destination, hit.point, ghosts);
        }
    }

    void onOrderShortClick()
    {
        var selected = selectionManager.selection;

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

        selectionManager.changeSelectionAfterOrder();
    }

    void onOrderLongClick()
    {
        var selected = selectionManager.selection;

        RaycastHit hit;
        if (getTerrainClickLocation(out hit)) {
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

            selectionManager.changeSelectionAfterOrder();
        }
    }

    public void tankButtonCallback()
    {
        if (_currentBuyTransaction == null)
            _currentBuyTransaction = new BuyTransaction(UnitType.Tank, owner);
        else
            _currentBuyTransaction.AddUnit();

        //buildUnit(UnitType.Tank);
        mouseMode = MouseMode.purchasing;
    }

    public void infantryButtonCallback()
    {
        buildUnit(UnitType.Infantry);
        mouseMode = MouseMode.purchasing;
    }

    public void afvButtonCallback()
    {
        buildUnit(UnitType.AFV);
        mouseMode = MouseMode.purchasing;
    }

    public void buildUnit(UnitType t)
    {
        var behaviour = GhostPlatoonBehaviour.Build(t, owner, 4);
        _currentBuyTransaction.GhostUnits.Add(behaviour);
    }

    public static void registerPlatoonBirth(PlatoonBehaviour platoon)
    {
        selectionManager.allUnits.Add(platoon);
    }

    public static void registerPlatoonDeath(PlatoonBehaviour platoon)
    {
        selectionManager.allUnits.Remove(platoon);
        selectionManager.selection.Remove(platoon);
    }

    private static void positionGhostUnits(Vector3 position, Vector3 facingPoint, List<GhostPlatoonBehaviour> units)
    {
        var diff = facingPoint - position;
        positionGhostUnits(position, diff.getRadianAngle(), units);
    }

    private static void positionGhostUnits(Vector3 position, float heading, List<GhostPlatoonBehaviour> units)
    {
        Vector3 forward = new Vector3(Mathf.Cos(heading), 0, Mathf.Sin(heading));
        int formationWidth = units.Count;// Mathf.CeilToInt(2 * Mathf.Sqrt(spawnList.Count));
        float unitDistance = 4 * PlatoonBehaviour.BaseDistance;
        var right = Vector3.Cross(forward, Vector3.up);
        var pos = position + unitDistance * (formationWidth - 1) * right / 2f;
        for (var i = 0; i < formationWidth; i++)
            units[i].GetComponent<GhostPlatoonBehaviour>().SetOrientation(pos - i * unitDistance * right, heading);
    }

    private void exitPurchasingMode()
    {
        _currentBuyTransaction.GhostUnits.Clear();

        _currentBuyTransaction = null;

        mouseMode = MouseMode.normal;
    }

    private SpawnPointBehaviour getClosestSpawn(Vector3 p)
    {
        var pointList = spawnPointList[owner.getTeam()];
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

    public static void addSpawnPoint(SpawnPointBehaviour s)
    {
        if (!spawnPointList.ContainsKey(s.team)) {
            spawnPointList.Add(s.team, new List<SpawnPointBehaviour>());
        }
        spawnPointList[s.team].Add(s);
    }

    public void applyHotkeys()
    {
        var selected = selectionManager.selection;

        if (Commands.unload()) {
            foreach (var t in selected.ConvertAll(x => x.Transporter).Where((x, i) => x != null)) {
                t.BeginQueueing(Input.GetKey(KeyCode.LeftShift));
                t.Unload();
                t.EndQueueing();
            }

        } else if (Commands.load()) {

            var transporters = selected.ConvertAll(x => x.Transporter).Where((x, i) => x != null).Where(x => x.transported == null).ToList();
            var infantry = selected.ConvertAll(x => x.Transportable).Where((x, i) => x != null).ToList();

            transporters.ForEach(x => x.BeginQueueing(Input.GetKey(KeyCode.LeftShift)));
            transporters.ConvertAll(x => x as Matchable<TransportableModule>).Match(infantry);
            transporters.ForEach(x => x.EndQueueing());

        } else if (Commands.firePos() && selected.Count != 0) {
            mouseMode = MouseMode.firePos;
            Cursor.SetCursor(firePosReticle, Vector2.zero, CursorMode.Auto);
        }
    }

    private void enterFirePosMode()
    {
        mouseMode = MouseMode.firePos;
        Cursor.SetCursor(firePosReticle, Vector2.zero, CursorMode.Auto);
    }

    private void exitFirePosMode()
    {
        mouseMode = MouseMode.normal;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    static bool getTerrainClickLocation(out RaycastHit hit)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Terrain"), QueryTriggerInteraction.Ignore);
    }

    private class SelectionManager
    {
        public List<PlatoonBehaviour> allUnits = new List<PlatoonBehaviour>();
        public List<PlatoonBehaviour> selection { get; private set; }

        private Vector3 mouseStart;
        private Vector3 mouseEnd;
        private Texture2D texture;
        private Texture2D borderTexture;
        private Color selectionBoxColor = Color.red;
        private bool active;

        private ClickManager clickManager;
        private UIManagerBehaviour outer;

        public SelectionManager(UIManagerBehaviour outer, int button, float mouseDragThreshold)
        {
            this.outer = outer;
            selection = new List<PlatoonBehaviour>();
            clickManager = new ClickManager(button, mouseDragThreshold, startBoxSelection, onSelectShortClick, endDrag, updateBoxSelection);

            if (texture == null) {
                var areaTransparency = .95f;
                var borderTransparency = .75f;
                texture = new Texture2D(1, 1);
                texture.wrapMode = TextureWrapMode.Repeat;
                texture.SetPixel(0, 0, selectionBoxColor - areaTransparency * Color.black);
                texture.Apply();
                borderTexture = new Texture2D(1, 1);
                borderTexture.wrapMode = TextureWrapMode.Repeat;
                borderTexture.SetPixel(0, 0, selectionBoxColor - borderTransparency * Color.black);
                borderTexture.Apply();
            }
        }

        public void Update()
        {
            clickManager.Update();

            if (outer.mouseMode == MouseMode.firePos && Input.GetMouseButtonDown(0)) {
                RaycastHit hit;
                getTerrainClickLocation(out hit);

                foreach (var platoon in selection) {
                    platoon.SendFirePosOrder(hit.point);
                }
            }
        }

        public void changeSelectionAfterOrder()
        {
            if (!Input.GetKey(KeyCode.LeftShift) && !Options.StickySelection)
                unselectAll(selection);
        }

        private void startBoxSelection()
        {
            mouseStart = Input.mousePosition;
            active = false;
        }

        private void updateBoxSelection()
        {
            mouseEnd = Input.mousePosition;
            updateSelection();
            active = true;
        }

        private void endDrag()
        {
            active = false;
            updateSelection();
        }

        private void onSelectShortClick()
        {
            unselectAll(selection);

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Selectable"), QueryTriggerInteraction.Ignore)) {
                var go = hit.transform.gameObject;
                var selectable = go.GetComponent<SelectableBehavior>();

                if (selectable != null)
                    selection.Add(selectable.GetPlatoon());
            }

            setSelected(selection);
        }

        private void updateSelection()
        {
            if (outer.mouseMode == MouseMode.firePos)
                return;

            List<PlatoonBehaviour> newSelection = allUnits.Where(x => isInside(x)).ToList();
            if (!Input.GetKey(KeyCode.LeftShift) && selection != null) {
                List<PlatoonBehaviour> old = selection.Except(newSelection).ToList();
                unselectAll(old);
            }
            setSelected(newSelection);
            selection = newSelection;
        }

        private bool isInside(PlatoonBehaviour obj)
        {
            var platoon = obj.GetComponent<PlatoonBehaviour>();
            if (!platoon.IsInitialized)
                return false;

            bool inside = false;
            inside |= platoon.Units.Any(x => isInside(x.transform.position));

            // TODO: This checks if the center of the icon is within the selection box. It should instead check if any of the four corners of the icon are within the box:
            inside |= isInside(platoon.Icon.transform.GetChild(0).position);
            return inside;
        }

        private bool isInside(Vector3 t)
        {
            Vector3 test = Camera.main.WorldToScreenPoint(t);
            bool insideX = (test.x - mouseStart.x) * (test.x - mouseEnd.x) < 0;
            bool insideY = (test.y - mouseStart.y) * (test.y - mouseEnd.y) < 0;
            return insideX && insideY;
        }

        private void unselectAll(List<PlatoonBehaviour> l)
        {
            l.ForEach(x => x.SetSelected(false));
            l.Clear();
        }

        private void setSelected(List<PlatoonBehaviour> l)
        {
            l.ForEach(x => x.SetSelected(true));
        }

        // Responsible for drawing the selection rectangle
        public void OnGui()
        {
            if (active) {
                float lineWidth = 3;
                float startX = mouseStart.x;
                float endX = mouseEnd.x;
                float startY = Screen.height - mouseStart.y;
                float endY = Screen.height - mouseEnd.y;

                Rect leftEdge = new Rect(startX - lineWidth / 2, startY + lineWidth / 2, lineWidth, endY - startY - lineWidth);
                Rect rightEdge = new Rect(endX - lineWidth / 2, startY + lineWidth / 2, lineWidth, endY - startY - lineWidth);
                Rect topEdge = new Rect(startX + lineWidth / 2, startY - lineWidth / 2, endX - startX - lineWidth, lineWidth);
                Rect bottomEdge = new Rect(startX + lineWidth / 2, endY - lineWidth / 2, endX - startX - lineWidth, lineWidth);
                Rect area = new Rect(startX + lineWidth / 2, startY + lineWidth / 2, endX - startX - lineWidth, endY - startY - lineWidth);
                GUI.DrawTexture(area, texture, ScaleMode.StretchToFill, true);
                GUI.DrawTexture(leftEdge, borderTexture, ScaleMode.StretchToFill, true);
                GUI.DrawTexture(rightEdge, borderTexture, ScaleMode.StretchToFill, true);
                GUI.DrawTexture(topEdge, borderTexture, ScaleMode.StretchToFill, true);
                GUI.DrawTexture(bottomEdge, borderTexture, ScaleMode.StretchToFill, true);
            }
        }
    }

}

public class Commands
{
    public static bool unload()
    {
        return Input.GetKeyDown(Hotkeys.Unload);
    }

    public static bool load()
    {
        return Input.GetKeyDown(Hotkeys.Load);
    }

    public static bool firePos()
    {
        return Input.GetKeyDown(Hotkeys.FirePos);
    }
}

public class Hotkeys
{
    public static KeyCode Unload = KeyCode.U;
    public static KeyCode Load = KeyCode.L;
    public static KeyCode FirePos = KeyCode.T;
}


