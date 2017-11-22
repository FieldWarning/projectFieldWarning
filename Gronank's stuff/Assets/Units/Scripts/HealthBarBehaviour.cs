using UnityEngine;
using System.Collections;

public class HealthBarBehaviour : SelectableBehavior {

    UnitBehaviour unit;
    GameObject bar;
    float health=1;
	void Start () {
        bar = transform.GetChild(0).gameObject;
        bar.AddComponent<SelectableBehavior>();
        setHealth(1);
	}
	
	// Update is called once per frame
	void Update () {
	    setHealth(Mathf.PingPong(Time.time/2,1));
	}
    public void setUnit(UnitBehaviour o)
    {
        
        unit = o;
        
    }
    void setHealth(float h)
    {
        health = Mathf.Clamp01(h);
        //bar.transform.localScale = new Vector3(health, 1, 1);
        //var offset = bar.GetComponent<Renderer>().bounds.extents.x ;
        //bar.transform.localPosition = new Vector3(offset-0.5f, 0, -.01f);
        bar.GetComponent<Renderer>().material.color = getColor(health);
        bar.GetComponent<Renderer>().material.SetFloat("_Cutoff", 1-health);
    }
    private Color getColor(float h)
    {
        Color c = Color.green;
        if (h < 0.5f) c = Color.yellow;
        if (h < 0.25f) c = Color.red;
        return c;
    }
}
