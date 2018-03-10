using UnityEngine;
using System.Collections;
using System;

public abstract class UnitBehaviour : SelectableBehavior,Matchable<Vector3>
{
	public UnitData data;

	protected TerrainCollider Ground {
		get {
			if (_Ground == null) {
				_Ground = GameObject.Find ("Terrain").GetComponent<TerrainCollider> ();
			}
			return _Ground;
		}
	}

	private TerrainCollider _Ground;
	float height;
	//TODO ask gronak wtf this is
	public Vector3 destination;
	protected float finalHeading;
	PlatoonBehaviour platoon;
	public bool gotDestination = false;
	Terrain terrain;
	float health;
	public float timeLeft;
	bool IsAlive;


	// Use this for initialization
	public void Start () {		
		destination = new Vector3 (100, 0, -100);
		transform.position = 100 * Vector3.down;
		enabled = false;
		height = 0;
		data = UnitData.GenericUnit ();
		timeLeft = (float)data.weapon.ReloadTime;
		health = data.maxHealth; //set the health to 10 (from UnitData.cs)
		IsAlive = true;
		setVisible (false);

        // TODO refactor to explicitly tag enemy units when we add multiplayer
        // TODO maybe refactor the constant out
        this.tag = "Unit";
    }

	// Update is called once per frame
	public void Update() {
		//if (Input.GetKey(KeyCode.Space)) Debug.LogError();

		doMovement();
		updateMapOrientation();
		FireWeapon(FindClosestEnemy());

	}

	/*void Countdown (int reloadtime)
	{
		for (int i = 1; i >= 0; i -= 1) {

			yield return false;
		}
		return false;
	}*/

	public bool GetIsAlive () {
		return IsAlive;
	}
	//used in platoon behaviour to find destroyed unit

	public void SetNewHeathOrDestroy (int receivedDamage) {
		health -= receivedDamage; //This one should be self explanatory
		if (health < 1) {
			IsAlive = false;
            platoon.units.Remove(this);
		
			Destroy(this.gameObject);

			return;
		}
	}


	public bool FireWeapon(GameObject target) {
		timeLeft -= Time.deltaTime;
		//Debug.Log ("Reloadtime left::" + timeLeft); spams the console to much 
		if (timeLeft < 0) {
		
			Debug.Log ("weapon fired");
	

			System.Random rnd = new System.Random();
			int chance = rnd.Next(1, 100);
            //if random number is under the accuracy the shell hits:
            if (chance < data.weapon.Accuracy) {

				Debug.Log("shot fired and hit");
				target.GetComponent<UnitBehaviour>()
                    .SetNewHeathOrDestroy(data.weapon.Damage);
				timeLeft = (float)data.weapon.ReloadTime;
				return true;
			}

			timeLeft = (float)data.weapon.ReloadTime;
			Debug.Log("shot fired and missed");
			return false;

		}

		return false;
	}

	public GameObject FindClosestEnemy() {
        GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");
		GameObject Target = null;

        // TODO filter out friendlies
		for (int i = 0; i < (int)units.Length; i++) {
            // TODO is this C-style null check valid? I think not in C#..
            if (units[i]) {

                // See if they are in range of weapon:
                var distance = Vector3.Distance(units[i].transform.position, transform.position);
                if (distance < data.weapon.FireRange) {
                    return units[i];
				}
			}
        }
        Debug.Log("null");
        return Target;
	}

	public abstract void updateMapOrientation();

	public void setPlatoon(PlatoonBehaviour p) {
		platoon = p;
	}
	/*public override void setDestination(Vector3 v)
    {
        platoon.setDestination(v);
    }
    public override void setFinalHeading(Vector3 v)
    {
        platoon.setFinalHeading(v);
    }
    public override void getDestinationFromGhost()
    {
        platoon.getDestinationFromGhost();
    }*/

	public float getHealth ()
	{
		return health;
	}

	public void setHealth (float health)
	{
		this.health = health;
	}

	public override PlatoonBehaviour getPlatoon ()
	{
		return platoon;
	}

	public void setUnitDestination (Vector3 v)
	{

		var diff = (v - transform.position).normalized;
		setFinalOrientation (v, diff.getRadianAngle ());
	}

	public void setFinalOrientation (Vector3 d, float heading)
	{
		gotDestination = true;
		destination = d;
		setUnitFinalHeading (heading);
        
	}

	public void setUnitFinalFacing (Vector3 v)
	{
		var diff = (v - destination).normalized;
		setUnitFinalHeading (diff.getRadianAngle ());
	}

	public virtual void setUnitFinalHeading (float heading)
	{
		finalHeading = heading;
	}

	protected abstract void doMovement ();

	public void setLayer (int l)
	{
		gameObject.layer = l;
	}

	protected abstract Renderer[] getRenderers ();

	public void setVisible (bool vis)
	{
		var renderers = getRenderers (); 
		foreach (var r in renderers) {
			r.enabled = vis;
		}
		if (vis) {
			setLayer (LayerMask.NameToLayer ("Selectable"));

		} else {
			setLayer (LayerMask.NameToLayer ("Ignore Raycast"));
		}
	}

	protected float unwrap (float f)
	{
		while (f > Mathf.PI)
			f -= 2 * Mathf.PI;
		while (f < -Mathf.PI)
			f += 2 * Mathf.PI;
		return f;
	}

	protected float getHeading ()
	{
		return (destination - transform.position).getDegreeAngle ();
	}


	public void setMatch (Vector3 match)
	{
		setUnitDestination (match);
	}

	public float getScore (Vector3 matchee)
	{

		return (matchee - transform.position).magnitude;
	}

	public abstract void setOriginalOrientation (Vector3 pos, Quaternion rotation, bool wake = true);


	protected void WakeUp ()
	{
		enabled = true;
		setVisible (true);
	}

	public abstract bool ordersComplete ();
}
