using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class VisibleBehavior : MonoBehaviour {
    private Point currentRegion;
    //List<VisibleBehavior> hostileTeam;
    private Dictionary<VisibleBehavior, bool> spottedBy = new Dictionary<VisibleBehavior, bool>();
    int spottedByCount;
    private Dictionary<VisibleBehavior, bool> spotting = new Dictionary<VisibleBehavior, bool>();
    private bool hostile;
    public Team team;
	// Use this for initialization
	void Start () {
        
        /*if (hostile)
        {
            setDetected(false);
        }
        else
        {
            GetComponent<Renderer>().material.color = Color.blue;
        }*/
	}
	
	// Update is called once per frame
	void Update () {
        var newRegion = VisibilityManager.getRegion(transform);
        if (currentRegion != newRegion)
        {
                //Debug.Log("region updated");
                VisibilityManager.updateUnitRegion(this, newRegion);
                currentRegion = newRegion;
        }
	}
    public void initialize(Team t)
    {
        
        team = t;
        var o = Team.Blue;
        if(t==Team.Blue)o=Team.Red;
        setHostileTeam(VisibilityManager.getTeamMembers(o));
        VisibilityManager.addVisibleBehaviour(this);
        if (team==Team.Red)
        {
            GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            GetComponent<Renderer>().material.color = Color.blue;
        }
        updateTeamBelonging();
        currentRegion = VisibilityManager.getRegion(transform);
        VisibilityManager.updateUnitRegion(this, currentRegion);
    }

    public void setDetected(bool detected)
    {
        if (!hostile) return;
        if (detected)
        {
            GetComponent<Renderer>().enabled = true;
            //GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            GetComponent<Renderer>().enabled = false;
            //GetComponent<Renderer>().material.color = Color.black;
        }
    }
    public Point getRegion()
    {
        return currentRegion;
    }

    internal void setSpotting(VisibleBehavior enemy, bool p)
    {
        if (enemy == this) Debug.LogError("error");
        if (p && !spotting[enemy])
        {
            spotting[enemy] = true;
        }
        else
        {
            spotting[enemy] = false;
        }
        
    }

    internal void setSpottedBy(VisibleBehavior unit, bool p)
    {
        if (p && !spottedBy[unit])
        {
            if (spottedByCount == 0)
            {
                setDetected(true);
            }
            spottedByCount += 1;
            spottedBy[unit] = true; ;
            
        }
        if (!p && spottedBy[unit])
        {
            spottedByCount -= 1;
            if (spottedByCount == 0)
            {
                setDetected(false);
            }
            spottedBy[unit] = false;
        }
    }

    internal void setHostileTeam(List<VisibleBehavior> hostileTeam)
    {
        foreach (var h in hostileTeam.Where(x=>!spotting.Keys.Contains(x)))
        {
            spotting.Add(h, false);
            spottedBy.Add(h, false);
        }
        
    }
    /*public override int GetHashCode()
    {
        return hashCode;
    }
    public bool Equals(VisibleBehavior obj)
    {
        return obj != null && obj.hashCode == this.hashCode;
    }*/



    internal void addHostile(VisibleBehavior b)
    {
        if (!spottedBy.ContainsKey(b))
        {
            spottedBy.Add(b, false);
        }
        if (!spotting.ContainsKey(b))
        {
            spotting.Add(b, false);
        }
    }

    internal void updateTeamBelonging()
    {
        //hostile = team != UIManagerBehaviour.owner.team;
        //if (!hostile || spottedByCount > 0)
        //{
        //    GetComponent<Renderer>().enabled = true;
        //}
        //else
        //{
        //    GetComponent<Renderer>().enabled = false;
        //}
    }
}
