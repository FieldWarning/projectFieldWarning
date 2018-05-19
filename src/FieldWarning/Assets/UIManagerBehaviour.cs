using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class UIManagerBehaviour : MonoBehaviour {

    // Use this for initialization
    public Player owner;
    private Vector3 destination;
    private Vector3 boxSelectStart;
    public static Dictionary<Team, List<SpawnPointBehaviour>> spawnPointList = new Dictionary<Team, List<SpawnPointBehaviour>>();
    List<GhostPlatoonBehaviour> spawnList = new List<GhostPlatoonBehaviour>();
    Camera cam;
    private bool spawningUnits = false;
    private bool enteringSpawning = false;
    private float clickTime;
    [SerializeField]
    private float mouseDragThreshold = 10.0f;
    private ClickManager orderMode;
    public static SelectionManager selectionManager;


    void Start() {
        selectionManager = new SelectionManager(0, mouseDragThreshold);
        cam = Camera.main.GetComponent<Camera>();
        
        orderMode = new ClickManager(1, mouseDragThreshold, onOrderStart, onOrderShortClick, onOrderLongClick, onOrderHold);
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
            if (selectionManager != null)
                selectionManager.Update();

            orderMode.Update();
            processCommands();
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

    public void processCommands() {
        var selected = selectionManager.selection;

        if (Commands.unload()) {
            foreach (var t in selected.ConvertAll(x => x.transporter).Where((x, i) => x != null)) {
                t.beginQueueing(Input.GetKey(KeyCode.LeftShift));
                t.unload();
                t.endQueueing();
            }
        }

        else if (Commands.load()) {
            
            var transporters = selected.ConvertAll(x => x.transporter).Where((x, i) => x != null).Where(x=>x.transported==null).ToList();
            var infantry = selected.ConvertAll(x => x.transportable).Where((x, i) => x != null).ToList();

            transporters.ForEach(x => x.beginQueueing(Input.GetKey(KeyCode.LeftShift)));
            transporters.ConvertAll(x => x as Matchable<PlatoonBehaviour.TransportableModule>).Match(infantry);
            transporters.ForEach(x => x.endQueueing());
            //transporters.ForEach(x => x.endQueueing());  
        }
    }

    bool getTerrainClickLocation(out RaycastHit hit) {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Terrain"), QueryTriggerInteraction.Ignore);
    }        
}

public class Commands {
    public static bool unload() {
        return Input.GetKeyDown(Hotkeys.Unload);
    }

    public static bool load() {
        return Input.GetKeyDown(Hotkeys.Load);
    }
}

public class Hotkeys {
    public static KeyCode Unload = KeyCode.U;
    public static KeyCode Load = KeyCode.L;
}


