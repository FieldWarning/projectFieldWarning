using UnityEngine;
using System.Collections;

public class HealthBarBehaviour : SelectableBehavior {

    UnitBehaviour unit;
    GameObject bar;
	void Start () {
        bar = transform.GetChild(0).gameObject;
        bar.AddComponent<SelectableBehavior>();
		setHealth(unit.data.maxHealth);
	}
	
	// Update is called once per frame
	void Update () {
		setHealth(unit.getHealth() / unit.data.maxHealth);
	}
    public void setUnit(UnitBehaviour o)
    {
        
        unit = o;
        
    }
	void setHealth(float h)
    {
        float health = Mathf.Clamp01(h);


        //bar.transform.localScale = new Vector3(health, 1, 1);
        //var offset = bar.GetComponent<Renderer>().bounds.extents.x ;
        //bar.transform.localPosition = new Vector3(offset-0.5f, 0, -.01f);
        bar.GetComponent<Renderer>().material.color = getColor(health);

		//Debug.Log("-- hp : " + (1f - health));
		bar.GetComponent<Renderer>().material.SetFloat("_Cutoff", 1f - health);
    }
    private Color getColor(float h)
    {
        Color c = Color.green;
        if (h < 0.5f) c = Color.yellow;
        if (h < 0.25f) c = Color.red;
        return c;
    }
}
