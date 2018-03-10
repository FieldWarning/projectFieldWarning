using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GhostPlatoonBehaviour : MonoBehaviour {

	// Use this for initialization
    bool init = false;
    bool initIcon = false;
    bool raycastIgnore;
    bool raycastIgnoreChange = false;
    public float finalHeading;
    GameObject icon;
    GameObject baseUnit;
    UnitType unitType;
    GameObject realPlatoon;
    PlatoonBehaviour platoonBehaviour;
    Team team;
    List<GameObject> units = new List<GameObject>();


	void Start () {
        buildRealPlatoon();
        initializeIcon();        
	}
	
	// Update is called once per frame
	void Update () {
        initialize();
        if (raycastIgnoreChange) {
            raycastIgnoreChange = false;
            _setIgnoreRaycast(raycastIgnore);
        }
	}
    public void initializeIcon() {
        if (!initIcon) {
            initIcon = true;
            icon = GameObject.Instantiate(Resources.Load<GameObject>("Icon"));
            //Debug.Log(platoonBehaviour.gameObject);
            icon.GetComponent<IconBehaviour>().setUnit(platoonBehaviour);
            icon.GetComponent<IconBehaviour>().setTeam(team);
            icon.transform.parent = transform;
        }
    }
    public void initialize()
    {
        if (!init) {
            
            init = true;
            gameObject.ApplyShaderRecursively(Shader.Find("Custom/Ghost"));
            icon.GetComponent<IconBehaviour>().setGhost();

            //setMembers(Resources.Load<GameObject>("Unit"), 4);
            setOrientation(new Vector3(), 0);

        }
    }
    public PlatoonBehaviour getRealPlatoon(){
        if (platoonBehaviour == null)
            buildRealPlatoon();
        
        return platoonBehaviour;
    }

    public void buildRealPlatoon() {
        realPlatoon = GameObject.Instantiate(Resources.Load<GameObject>("Platoon"));
        //yield return null;
        platoonBehaviour = realPlatoon.GetComponent<PlatoonBehaviour>();
        platoonBehaviour.setMembers(unitType, team, units.Count);
        platoonBehaviour.setEnabled(false) ;
        platoonBehaviour.setGhostPlatoon(this);
        realPlatoon.transform.position = transform.position+100*Vector3.down;        
    }

    public void setMembers(UnitType t, Team team, int n) {
        this.team = team;
        unitType = t;
        baseUnit = Units.getUnit(t);
        //create Infantry
        for (int i = 0; i < n; i++) {
            GameObject go = GameObject.Instantiate(baseUnit);
            go.GetComponent<UnitBehaviour>().enabled = false;
            var shader = Resources.Load<Shader>("Shaders/Ghost");
            go.ApplyShaderRecursively(shader);
            go.transform.position = 100 * Vector3.down;
            units.Add(go);
            //go.transform.parent = this.transform;
        }
    }

    public void setOrientation(Vector3 position, Vector3 facing) {
        var diff = (facing - position);
        var heading = Mathf.Atan2(diff.y, diff.x);
        
        setOrientation(position, heading);
    }

    public void setOrientation(Vector3 position, float heading) {
        
        finalHeading = heading;
        transform.position = position;
        Vector3 v = new Vector3(Mathf.Cos(heading), 0, Mathf.Sin(heading));
        var left = new Vector3(-v.z, 0, v.x);
        
        var pos = position + (units.Count-1) * (PlatoonBehaviour.baseDistance / 2) * left;
        for (int i = 0; i < units.Count; i++) {
            
            var localPosition = pos - PlatoonBehaviour.baseDistance * i * left;
            var localRotation = Quaternion.Euler(new Vector3(0, -Mathf.Rad2Deg*(heading)+90, 0));
            units[i].GetComponent<UnitBehaviour>().setOriginalOrientation(localPosition, localRotation,false);
            units[i].GetComponent<UnitBehaviour>().updateMapOrientation();          
        }
    }

    public void setFacing(Vector3 facing) {
        setOrientation(transform.position, facing);
    }

    public void setVisible(bool vis) {
        initializeIcon();
        icon.GetComponent<IconBehaviour>().setVisible(vis);
        units.ForEach(x => x.GetComponent<UnitBehaviour>().setVisible(vis));
    }

    public void setIgnoreRaycast(bool ignore) {
        raycastIgnore = ignore;
        raycastIgnoreChange = true;
    }

    public void _setIgnoreRaycast(bool ignore) {
        
        var layer=0;
        if (ignore)
            layer=2;
        
        Debug.Log(layer);
        gameObject.layer = layer;
        icon.layer = layer;
        foreach (var u in units)        
            u.layer = layer;
        
    }

    public void destroy() {
        foreach (var u in units)
            Object.Destroy(u);
        
        Object.Destroy(gameObject);
    }

    public static GhostPlatoonBehaviour build(UnitType t, Team currentTeam,int count) {
        var behaviour = GhostPlatoonBehaviour.build();
        behaviour.setMembers(t, currentTeam, count);
        return behaviour;
    }

    public static GhostPlatoonBehaviour build() {
        GameObject go = GameObject.Instantiate(Resources.Load<GameObject>("GhostPlatoon"));
        var behaviour = go.GetComponent<GhostPlatoonBehaviour>();
        return behaviour;
    }
    
}
