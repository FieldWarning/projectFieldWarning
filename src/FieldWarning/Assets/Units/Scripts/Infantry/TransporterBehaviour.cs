using UnityEngine;
using System.Collections;

public class TransporterBehaviour : MonoBehaviour {

	// Use this for initialization
    public InfantryBehaviour transported;
    public InfantryBehaviour target;
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        
        if (target != null)
        {
            if (target.interactsWithTransport(true))
            {
                GetComponent<UnitBehaviour>().setUnitDestination(transform.position);
            }
            //target.setRally(getRallyPoint(), transform.position);//???????
			else if (GetComponent<UnitBehaviour>().pathfinder.HasDestination())
            {
                GetComponent<UnitBehaviour>().setUnitDestination(target.transform.position);
            }
        }
	}
    public void unload()
    {
        var pos=transform.position;
        var rallyPoint = getRallyPoint();
        transported.unload(pos, rallyPoint);
        //Debug.Log("unload");
    }

    public Vector3 getRallyPoint()
    {
        var rallyDistance = 2;
        var rallyPoint = transform.position - rallyDistance * transform.forward;
        return rallyPoint;
    }
    public void load(InfantryBehaviour t)
    {
        this.target = t;
		//GetComponent<UnitBehaviour>().gotDestination = true;
        
    }
    public bool loadingComplete()
    {
        if (target.interactsWithTransport(false))
        {
            transported = target;
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool unloadingComplete()
    {
        if (transported == null || !transported.interactsWithTransport(false))
        {
            transported=null;
            return true;
        }else{
            return false;
        }
        
    }
    public bool interrupt()
    {
        if (transported == null)
        {
            return true;
        }
        else if (!target.interactsWithTransport(true))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
