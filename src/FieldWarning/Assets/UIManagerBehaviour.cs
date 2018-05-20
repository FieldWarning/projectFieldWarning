using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class UIManagerBehaviour : MonoBehaviour {
    [SerializeField] private float mouseDragThreshold = 10.0f;
    [SerializeField] private Texture2D firePosReticle;

    // Use this for initialization
    public Player owner;
    private Vector3 destination;
    private Vector3 boxSelectStart;
    public static Dictionary<Team, List<SpawnPointBehaviour>> spawnPointList = new Dictionary<Team, List<SpawnPointBehaviour>>();
    List<GhostPlatoonBehaviour> spawnList = new List<GhostPlatoonBehaviour>();
    private bool spawningUnits = false;
    private bool enteringSpawning = false;
    private ClickManager rightClickManager;
    private static SelectionManager selectionManager;

    private enum OrderMode {normal, spawning, firePos};
    private OrderMode mouseMode = OrderMode.normal;

    void Start() {
        if (firePosReticle == null)
            throw new Exception("No fire pos reticle specified!");

        selectionManager = new SelectionManager(this, 0, mouseDragThreshold);
        
        rightClickManager = new ClickManager(1, mouseDragThreshold, onOrderStart, onOrderShortClick, onOrderLongClick, onOrderHold);
    }
    
    void Update() {
        if (spawningUnits) {

            RaycastHit hit;
            if (Input.GetMouseButtonUp(0) && enteringSpawning) {
                enteringSpawning = false;
            } else if (getTerrainClickLocation(out hit)
                && hit.transform.gameObject.name.Equals("Terrain")) {

                arrangeToBeSpawned(hit.point);

                if (Input.GetMouseButtonUp(0) && !enteringSpawning) {
                    var spawnPoint = getClosestSpawn(hit.point);
                    var realPlatoons = spawnList.ConvertAll(x => x.GetComponent<GhostPlatoonBehaviour>().getRealPlatoon());

                    spawnPoint.updateQueue(realPlatoons);
                    if (Input.GetKey(KeyCode.LeftShift)) {
                        replaceSpawnList();
                    } else {
                        spawnList.Clear();
                        spawningUnits = false;
                    }
                }
            }

            if (Input.GetMouseButton(1))
                destroySpawning();

        } else {
            processHotkeys();
            if (selectionManager != null)
                selectionManager.Update();

            switch (mouseMode) { 
                case OrderMode.normal:
                    rightClickManager.Update();
                    break;

                case OrderMode.firePos:
                    if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) {
                        leaveFirePosMode();
                    }

                    break;

                default:
                    throw new Exception("impossible state");
            }
        }
    }

    void OnGUI() {
        if (selectionManager != null)
            selectionManager.OnGui();
    }

    void onOrderStart() {
        var selected = selectionManager.selection;

        RaycastHit hit;
        if (getTerrainClickLocation(out hit)) {
            Vector3 com = selected.ConvertAll(x => x as MonoBehaviour).getCenterOfMass();
            List<GhostPlatoonBehaviour> ghosts = selected.ConvertAll<GhostPlatoonBehaviour>(x => x.ghostPlatoon);
            arrangeGhosts(hit.point, 2 * hit.point - com, ghosts);
            destination = hit.point;
        }
    }

    void onOrderHold() {
        var selected = selectionManager.selection;

        RaycastHit hit;
        if (getTerrainClickLocation(out hit)) {

            List<GhostPlatoonBehaviour> ghosts = selected.ConvertAll(x => x.ghostPlatoon);
            ghosts.ForEach(x => x.setVisible(true));
            arrangeGhosts(destination, hit.point, ghosts);
        }
    }

    void onOrderShortClick() {
        var selected = selectionManager.selection;

        var destinations = selected.ConvertAll(x => x.ghostPlatoon.transform.position);
        var shift = Input.GetKey(KeyCode.LeftShift);
        selected.ForEach(x => x.movement.beginQueueing(shift));
        selected.ConvertAll(x => x.movement as Matchable<Vector3>).Match(destinations);
        selected.ForEach(x => x.movement.getHeadingFromGhost());
        selected.ForEach(x => x.movement.endQueueing());
        //selected.ForEach(x => x.ghostPlatoon.setVisible(false));
        /*var destinations=selected.ConvertAll(x=>x.ghostPlatoon);
        foreach (var go in selected)
        {
            var behaviour = go.GetComponent<SelectableBehavior>().getPlatoon();
            behaviour.getDestinationFromGhost();
        }*/

        selectionManager.changeSelectionAfterOrder();
    }

    void onOrderLongClick() {
        var selected = selectionManager.selection;

        RaycastHit hit;
        if (getTerrainClickLocation(out hit)) {
            var shift = Input.GetKey(KeyCode.LeftShift);
            selected.ForEach(x => x.movement.beginQueueing(shift));
            var destinations = selected.ConvertAll(x => x.ghostPlatoon.transform.position);
            selected.ConvertAll(x => x.movement as Matchable<Vector3>).Match(destinations);
            selected.ForEach(x => x.movement.getHeadingFromGhost());
            selected.ForEach(x => x.movement.endQueueing());
            /*foreach (var go in selected)
            {
                go.GetComponent<SelectableBehavior>().getDestinationFromGhost();
                go.GetComponent<PlatoonBehaviour>().ghostPlatoon.GetComponent<GhostPlatoonBehaviour>().setVisible(false);
            }*/

            selectionManager.changeSelectionAfterOrder();
        }
    }

    public void buildTanks() {
        buildUnit(UnitType.Tank);
    }

    public void buildInfantry() {
        buildUnit(UnitType.Infantry);
    }

    public void buildAFV() {
        buildUnit(UnitType.AFV);
    }

    public void buildUnit(UnitType t) {
        var behaviour = GhostPlatoonBehaviour.build(t, owner, 4);
        addSpawn(behaviour);
    }

    public static void registerPlatoonBirth(PlatoonBehaviour platoon) {
        selectionManager.allUnits.Add(platoon);
    }
    public static void registerPlatoonDeath(PlatoonBehaviour platoon) {
        selectionManager.allUnits.Remove(platoon);
        selectionManager.selection.Remove(platoon);
    }

    private void addSpawn(GhostPlatoonBehaviour g) {
        spawningUnits = true;
        enteringSpawning = true;
        spawnList.Add(g);
    }

    private void replaceSpawnList() {
        var count = spawnList.Count;
        spawnList.Clear();
        for (int i = 0; i < count; i++) {
            buildTanks();
        }
        enteringSpawning = false;
    }

    private void arrangeToBeSpawned(Vector3 point) {
        arrangeGhosts(point, 2 * point - getClosestSpawn(point).transform.position, spawnList);
        /*if (spawnList.Count == 0) return;
        Vector3 forward = (point - getClosestSpawn(point).transform.position);
        forward.y = 0;
        forward.Normalize();
        float heading = Mathf.Atan2(forward.z, forward.x);

        int formationWidth = spawnList.Count;// Mathf.CeilToInt(2 * Mathf.Sqrt(spawnList.Count));
        float unitDistance=4 * PlatoonBehaviour.baseDistance;
        var right = Vector3.Cross(forward, Vector3.up);
        var pos = point + unitDistance * (formationWidth-1) * right / 2f;
        for (var i = 0; i < formationWidth; i++)
        {            
            spawnList[i].GetComponent<GhostPlatoonBehaviour>().setOrientation(pos - i*unitDistance * right,heading);
        }*/
    }

    private static void arrangeGhosts(Vector3 position, Vector3 facingPoint, List<GhostPlatoonBehaviour> units) {

        var diff = facingPoint - position;
        arrangeGhosts(position, diff.getRadianAngle(), units);
    }

    private static void arrangeGhosts(Vector3 position, float heading, List<GhostPlatoonBehaviour> units) {

        Vector3 forward = new Vector3(Mathf.Cos(heading), 0, Mathf.Sin(heading));
        int formationWidth = units.Count;// Mathf.CeilToInt(2 * Mathf.Sqrt(spawnList.Count));
        float unitDistance = 4 * PlatoonBehaviour.baseDistance;
        var right = Vector3.Cross(forward, Vector3.up);
        var pos = position + unitDistance * (formationWidth - 1) * right / 2f;
        for (var i = 0; i < formationWidth; i++)
            units[i].GetComponent<GhostPlatoonBehaviour>().setOrientation(pos - i * unitDistance * right, heading);        
    }

    private void destroySpawning() {
        foreach (var p in spawnList) {
            p.GetComponent<GhostPlatoonBehaviour>().destroy();
        }
        spawnList.Clear();
    }

    private SpawnPointBehaviour getClosestSpawn(Vector3 p) {
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

    public static void addSpawnPoint(SpawnPointBehaviour s) {
        if (!spawnPointList.ContainsKey(s.team)) {
            spawnPointList.Add(s.team, new List<SpawnPointBehaviour>());
        }
        spawnPointList[s.team].Add(s);
    }

    public void processHotkeys() {
        var selected = selectionManager.selection;

        if (Commands.unload()) {
            foreach (var t in selected.ConvertAll(x => x.transporter).Where((x, i) => x != null)) {
                t.beginQueueing(Input.GetKey(KeyCode.LeftShift));
                t.unload();
                t.endQueueing();
            }
        } else if (Commands.load()) {

            var transporters = selected.ConvertAll(x => x.transporter).Where((x, i) => x != null).Where(x => x.transported == null).ToList();
            var infantry = selected.ConvertAll(x => x.transportable).Where((x, i) => x != null).ToList();

            transporters.ForEach(x => x.beginQueueing(Input.GetKey(KeyCode.LeftShift)));
            transporters.ConvertAll(x => x as Matchable<PlatoonBehaviour.TransportableModule>).Match(infantry);
            transporters.ForEach(x => x.endQueueing());

        } else if (Commands.firePos()) {
            mouseMode = OrderMode.firePos;
            Cursor.SetCursor(firePosReticle, Vector2.zero, CursorMode.Auto);
        }
    }

    private void enterFirePosMode() {
        mouseMode = OrderMode.firePos;
        Cursor.SetCursor(firePosReticle, Vector2.zero, CursorMode.Auto);
    }

    private void leaveFirePosMode() {
        mouseMode = OrderMode.normal;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    static bool getTerrainClickLocation(out RaycastHit hit) {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Terrain"), QueryTriggerInteraction.Ignore);
    }

    private class SelectionManager {
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

        public SelectionManager(UIManagerBehaviour outer, int button, float mouseDragThreshold) {
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

        public void Update() {
            clickManager.Update();

            if (outer.mouseMode == OrderMode.firePos && Input.GetMouseButtonDown(0)) {
                RaycastHit hit;
                getTerrainClickLocation(out hit);

                foreach (var platoon in selection) {
                    platoon.sendFirePosOrder(hit.point);
                }
            }
        }

        public void changeSelectionAfterOrder() {
            if (!Input.GetKey(KeyCode.LeftShift) && !Options.StickySelection)
                unselectAll(selection);
        }

        private void startBoxSelection() {
            mouseStart = Input.mousePosition;
            active = false;
        }

        private void updateBoxSelection() {
            Debug.Log(outer.mouseMode);
            if (outer.mouseMode != OrderMode.normal)
                return;

            mouseEnd = Input.mousePosition;
            updateSelection();
            active = true;
        }

        private void endDrag() {
            active = false;
            updateSelection();
        }

        private void onSelectShortClick() {
            unselectAll(selection);

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Selectable"), QueryTriggerInteraction.Ignore)) {
                var go = hit.transform.gameObject;
                var selectable = go.GetComponent<SelectableBehavior>();

                if (selectable != null)
                    selection.Add(selectable.getPlatoon());
            }

            setSelected(selection);
        }

        private void updateSelection() {
            List<PlatoonBehaviour> newSelection = allUnits.Where(x => isInside(x)).ToList();
            if (!Input.GetKey(KeyCode.LeftShift) && selection != null) {
                List<PlatoonBehaviour> old = selection.Except(newSelection).ToList();
                unselectAll(old);
            }
            setSelected(newSelection);
            selection = newSelection;
        }

        private bool isInside(PlatoonBehaviour obj) {
            var platoon = obj.GetComponent<PlatoonBehaviour>();
            if (!platoon.initialized)
                return false;

            bool inside = false;
            inside |= platoon.units.Any(x => isInside(x.transform.position));

            // TODO: This checks if the center of the icon is within the selection box. It should instead check if any of the four corners of the icon are within the box:
            inside |= isInside(platoon.icon.transform.GetChild(0).position);
            return inside;
        }

        private bool isInside(Vector3 t) {
            Vector3 test = Camera.main.WorldToScreenPoint(t);
            bool insideX = (test.x - mouseStart.x) * (test.x - mouseEnd.x) < 0;
            bool insideY = (test.y - mouseStart.y) * (test.y - mouseEnd.y) < 0;
            return insideX && insideY;
        }

        private void unselectAll(List<PlatoonBehaviour> l) {
            l.ForEach(x => x.setSelected(false));
            l.Clear();
        }

        private void setSelected(List<PlatoonBehaviour> l) {
            l.ForEach(x => x.setSelected(true));
        }

        // Responsible for drawing the selection rectangle
        public void OnGui() {
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

public class Commands {
    public static bool unload() {
        return Input.GetKeyDown(Hotkeys.Unload);
    }

    public static bool load() {
        return Input.GetKeyDown(Hotkeys.Load);
    }

    public static bool firePos() {
        return Input.GetKeyDown(Hotkeys.FirePos);
    }
}

public class Hotkeys {
    public static KeyCode Unload = KeyCode.U;
    public static KeyCode Load = KeyCode.L;
    public static KeyCode FirePos = KeyCode.T;
}


