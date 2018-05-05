using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CompoundHealthbarBehaviour : SelectableBehavior {

	// Use this for initialization
    List<GameObject> objects;
	void Start () {
        transform.localScale = new Vector3(0.85f, 1, 1);
        transform.localPosition = new Vector3(0.116f, 0.441f, 0);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public void setSource(List<UnitBehaviour> o)
    {
        destroyChilden();
        float scale = 1f / o.Count;
        for (int i = 0; i < o.Count; i++)
        {
            var obj = GameObject.Instantiate(Resources.Load<GameObject>("HealthbarContainer"));
            obj.GetComponent<HealthBarBehaviour>().setUnit(o[i]);
            obj.transform.parent = transform;
            
            obj.transform.localScale = new Vector3(scale, .08f, 1);
            float offset = scale * (1 / 2 + i)-0.5f;
            obj.transform.localPosition = new Vector3(offset, 0, -.01f);
        }
    }
    private void destroyChilden()
    {
        for (int i = transform.childCount-1; i >=0 ; i--)
        {
            Object.Destroy(transform.GetChild(i));
        }
    }
}
