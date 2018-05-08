using UnityEngine;
using PFW.Weapons;

public abstract class UnitBehaviour : SelectableBehavior, Matchable<Vector3>
{
	public const string UNIT_TAG = "Unit";

	public UnitData data;
	public Vector3 destination;
	public PlatoonBehaviour platoon { get; private set; }
	public bool IsAlive { get; private set; }
	public Pathfinder pathfinder { get; private set; }
	public AudioSource source { get; private set; }

	protected TerrainCollider Ground {
		get {
			if (_Ground == null) {
				_Ground = GameObject.Find ("Terrain").GetComponent<TerrainCollider> ();
			}
			return _Ground;
		}
	}

	protected float finalHeading;

	private Terrain terrain;
	private TerrainCollider _Ground;
	private float health;


	// Use this for initialization
	public void Start ()
	{		
		destination = new Vector3 (100, 0, -100);
		transform.position = 100 * Vector3.down;
		enabled = false;
		data = UnitData.GenericUnit ();
		health = data.maxHealth; //set the health to 10 (from UnitData.cs)
		IsAlive = true;
		setVisible (false);
        this.tag = UNIT_TAG;

        source = GetComponent<AudioSource> ();

		pathfinder = new Pathfinder (this, PathfinderData.singleton);

	}

	// Update is called once per frame
	public void Update ()
	{
		doMovement ();
		updateMapOrientation ();

	}
    
	public void HandleHit (float receivedDamage)
	{
		if (health <= 0)
			return;

		health -= receivedDamage; 
		if (health <= 0) {
			IsAlive = false;
			platoon.units.Remove (this);
		
			Destroy (this.gameObject);
            platoon.ghostPlatoon.handleRealUnitDestroyed ();
			if (platoon.units.Count == 0) {
				Destroy (platoon.gameObject);
				// TODO remove from selection if part of one
			}

			return;
		}
	}

	public abstract void updateMapOrientation ();

	public void setPlatoon (PlatoonBehaviour p)
	{
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

	// Sets the unit's destination location, with a default heading value
	public void setUnitDestination (Vector3 v)
	{

		var diff = (v - transform.position).normalized;
		setFinalOrientation (v, diff.getRadianAngle ());
	}

	// Sets the unit's destination location, with a specific given heading value
	public void setFinalOrientation (Vector3 d, float heading)
	{
		destination = d;
		setUnitFinalHeading (heading);
		pathfinder.SetPath (destination, MoveCommandType.Fast);
	}

	// Updates the unit's final heading so that it faces the specified location
	public void setUnitFinalFacing (Vector3 v)
	{
		var diff = (v - destination).normalized;
		setUnitFinalHeading (diff.getRadianAngle ());
	}

	// Updates the unit's final heading to the specified value 
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
		foreach (Weapon weapon in gameObject.GetComponents<Weapon>())
			weapon.WakeUp ();
	}

	public abstract bool ordersComplete ();

}

public enum MoveCommandType
{
	Fast,
	Slow
}

