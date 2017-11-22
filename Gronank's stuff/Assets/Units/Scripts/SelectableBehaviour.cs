using UnityEngine;
using System.Collections;

public class SelectableBehavior : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    /*public virtual void setDestination(Vector3 v)
    {
        transform.GetComponentInParent<SelectableBehavior>().setDestination(v);
    }
    public virtual void setFinalHeading(Vector3 v)
    {
        transform.GetComponentInParent<SelectableBehavior>().setFinalHeading(v);
    }*/
    public virtual PlatoonBehaviour getPlatoon()
    {
        return transform.parent.GetComponent<SelectableBehavior>().getPlatoon();
    }
    /*public virtual void getDestinationFromGhost()
    {
        transform.GetComponentInParent<SelectableBehavior>().getDestinationFromGhost();
    }*/
}
