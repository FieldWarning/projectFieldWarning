using UnityEngine;

public abstract class UnitBehaviour : SelectableBehavior, Matchable<Vector3>
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

	public Vector3 destination;
	protected float finalHeading;
	PlatoonBehaviour platoon;
	private Terrain terrain;
	private float health;
	public float reloadTimeLeft;
	public bool IsAlive { get;  private set; }
	public Pathfinder pathfinder;

	public Transform turret;
	public Transform barrel;
	public Transform shotStartingPosition;
	public ParticleSystem shotEffect;

    [SerializeField]
    private AudioClip shotSound;
	private AudioSource source;
	private float shotVolume = 1.0F;

    public const string UNIT_TAG = "Unit";

	// Use this for initialization
	public void Start ()
	{		
		destination = new Vector3 (100, 0, -100);
		transform.position = 100 * Vector3.down;
		enabled = false;
		data = UnitData.GenericUnit ();
		reloadTimeLeft = (float)data.weapon.ReloadTime;
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
		GameObject target = FindClosestEnemy ();
		if (RotateTurret (target))
			TryFireWeapon (target);
	}
    
	public void HandleHit (int receivedDamage)
	{
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

	bool RotateTurret (GameObject target)
	{
		bool aimed = false;
		float targetTurretAngle = 0f;
		float targetBarrelAngle = 0f;

		if (target != null) {
			aimed = true;
			shotStartingPosition.LookAt (target.transform);

			Vector3 directionToTarget = target.transform.position - turret.position;
			Quaternion rotationToTarget = Quaternion.LookRotation (transform.InverseTransformDirection (directionToTarget));

			targetTurretAngle = rotationToTarget.eulerAngles.y.unwrapDegree();
			if (Mathf.Abs (targetTurretAngle) > data.weapon.ArcHorizontal) {
				targetTurretAngle = 0f;
				aimed = false;
			}

			targetBarrelAngle = rotationToTarget.eulerAngles.x.unwrapDegree();
			if (targetBarrelAngle < -data.weapon.ArcUp || targetBarrelAngle > data.weapon.ArcDown) {
				targetBarrelAngle = 0f;
				aimed = false;
			}
		}

		float turretAngle = turret.localEulerAngles.y;
		float barrelAngle = barrel.localEulerAngles.x;
		float turn = Time.deltaTime * data.weapon.RotationRate;
		float deltaAngle;

		deltaAngle = (targetTurretAngle - turretAngle).unwrapDegree();
		if (Mathf.Abs (deltaAngle) > turn) {
			turretAngle += (deltaAngle > 0 ? 1 : -1) * turn;
			aimed = false;
		} else {
			turretAngle = targetTurretAngle;
		}
			
		deltaAngle = (targetBarrelAngle - barrelAngle).unwrapDegree();
		if (Mathf.Abs (deltaAngle) > turn) {
			barrelAngle += (deltaAngle > 0 ? 1 : -1) * turn;
			aimed = false;
		} else {
			barrelAngle = targetBarrelAngle;
		}

		turret.localEulerAngles = new Vector3 (0, turretAngle, 0);
		barrel.localEulerAngles = new Vector3 (barrelAngle, 0, 0);
		//shotStartingPosition.localEulerAngles = new Vector3 (barrelAngle, turretAngle, 0);

		return aimed;
	}

    public bool FireWeapon(GameObject target) 
    {
        // sound
        source.PlayOneShot(shotSound, shotVolume);
        // particle
        shotEffect.Play();


        System.Random rnd = new System.Random();
        int roll = rnd.Next(1, 100);

        // HIT
        if (roll < data.weapon.Accuracy) {
            target.GetComponent<UnitBehaviour>()
                    .HandleHit(data.weapon.Damage);
            return true;
        }

        // MISS
        return false;
    }


	public bool TryFireWeapon (GameObject target)
	{
		reloadTimeLeft -= Time.deltaTime;
        if (reloadTimeLeft > 0)
            return false;
        
        reloadTimeLeft = (float)data.weapon.ReloadTime;        
		return FireWeapon(target);
	}

	public GameObject FindClosestEnemy ()
	{
		GameObject[] units = GameObject.FindGameObjectsWithTag (UNIT_TAG);
		GameObject Target = null;
		Team thisTeam = this.platoon.team;
        
		for (int i = 0; i < (int)units.Length; i++) {
			// Filter out friendlies:
			if (units [i].GetComponent<UnitBehaviour> ().platoon.team == thisTeam)
				continue;

			// See if they are in range of weapon:
			var distance = Vector3.Distance (units [i].transform.position, transform.position);
			if (distance < data.weapon.FireRange) {
				return units [i];
			}
		}
		return Target;
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

public enum MoveCommandType
{
	Fast,
	Slow
}

