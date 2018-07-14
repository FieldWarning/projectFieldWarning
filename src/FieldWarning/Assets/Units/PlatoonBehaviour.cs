/**
 * Copyright (c) 2017-present, PFW Contributors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in
 * compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software distributed under the License is
 * distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See
 * the License for the specific language governing permissions and limitations under the License.
 */

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PFW.Weapons;

public class PlatoonBehaviour : SelectableBehavior {
    public UnitType type;
    public List<PlatoonModule> modules = new List<PlatoonModule>();
    public List<UnitBehaviour> units = new List<UnitBehaviour>();
    public Queue<Waypoint> waypoints = new Queue<Waypoint>();
    private Waypoint activeWaypoint;
    public MovementModule movement;
    public TransporterModule transporter;
    public TransportableModule transportable;
    public GhostPlatoonBehaviour ghostPlatoon;
    public IconBehaviour icon;
    public static float baseDistance = 4;
    public Player owner { get; private set; }
	public bool initialized = false;

	// Use this for initialization
	void Start() {
	}

	void Update() {

		if (!icon.isInitiated)
			icon.setSource(units);
		var pos = new Vector3();
		units.ForEach(x => pos += x.transform.position);
		transform.position = pos / units.Count;
		//Debug.Log("Unit count of platoon " + GetInstanceID() + " is " + units.Count());
		modules.ForEach(x => x.Update());

		if (activeWaypoint == null || activeWaypoint.orderComplete()) {
			if (waypoints.Count > 0) {
				activeWaypoint = waypoints.Dequeue ();
				activeWaypoint.processWaypoint ();
			} else {
				activeWaypoint = null;
				//units.ForEach (x => x.gotDestination = false);
			}
			//setFinalOrientation(waypoint.destination,waypoint.heading);
		}
	}

	public void buildModules(UnitType t) {
		movement = new MovementModule(this);
		modules.Add(movement);

		if (t == UnitType.AFV) {
			transporter = new TransporterModule(this);
			modules.Add(transporter);
		}

		if (t == UnitType.Infantry) {
			transportable = new TransportableModule(this);
			modules.Add(transportable);
		}
	}

	public void setMembers(UnitType t, Player owner, int n) {
        var go = GameObject.Instantiate(Resources.Load<GameObject>("Icon"));
        go.transform.parent = transform;
        icon = go.GetComponent<IconBehaviour>();
        icon.setPlatoon(this);

        UIManagerBehaviour.registerPlatoonBirth(this);
		type = t;
		this.owner = owner;
        icon.setTeam(owner.getTeam());

        var g = Units.getUnit(t);
		for (int i = 0; i < n; i++) {
			go = GameObject.Instantiate(g);
			var unitBehaviour = go.GetComponent<UnitBehaviour>();
			unitBehaviour.setPlatoon(this);
			units.Add(unitBehaviour);

            var collider = go.GetComponentInChildren<BoxCollider>();
            collider.enabled = true;
        }

		buildModules(t);

		if (t == UnitType.AFV) {
			var ghost = GhostPlatoonBehaviour.build(UnitType.Infantry, owner, n);            
			transporter.setTransported(ghost.getRealPlatoon());
			ghost.setOrientation(100 * Vector3.down, 0);
			ghost.setVisible(false);
		}
		movement.setDestination(Vector3.forward);

        initialized = true;
    }

	public void setGhostPlatoon(GhostPlatoonBehaviour obj)	{
		ghostPlatoon = obj;
	}

	public override PlatoonBehaviour getPlatoon() {
		return this;
	}

	public void spawn(Vector3 pos) {
		enabled = true;
		transform.position = pos;
		var heading = ghostPlatoon.GetComponent<GhostPlatoonBehaviour> ().finalHeading;
		Vector3 forward = new Vector3 (Mathf.Cos (heading), 0, Mathf.Sin (heading));
		float spawndistance = 2;
		for (int i = 0; i < units.Count; i++) {
			units [i].setOriginalOrientation (pos + i * spawndistance * forward, Quaternion.FromToRotation (Vector3.forward, forward));
		}
		movement.beginQueueing (false);
		movement.getDestinationFromGhost ();
		movement.endQueueing ();
		ghostPlatoon.setVisible (false);
	}

	public void setSelected (bool selected)
	{
		icon.setSelected (selected);
	}

	public void setEnabled (bool enabled)
	{
		this.enabled = enabled;
		if (icon != null)
			icon.setVisible (enabled);
        
	}

    public void sendFirePosOrder(Vector3 position) {
        foreach (var unit in units) {
            var weapons = unit.GetComponents<Weapon>();

            foreach (var weapon in weapons) 
                weapon.setTarget(position);
        }
    }

	public abstract class PlatoonModule
	{
		public PlatoonBehaviour platoon;

		protected Waypoint newWaypoint {
			get {
				if (_newWaypoint == null) {
					_newWaypoint = getModuleWaypoint ();
				}
				return _newWaypoint;
			}
			set {
				_newWaypoint = value;
			}
		}

		private Waypoint _newWaypoint;
		protected bool queueing = false;

		public PlatoonModule (PlatoonBehaviour p)
		{
			platoon = p;
		}

		public virtual void Update ()
		{

		}

		public void beginQueueing (bool queue)
		{
			queueing = queue;
			if (!queue) {
				platoon.waypoints.Clear ();
			}
			newWaypoint = getModuleWaypoint ();

		}

		public void endQueueing ()
		{
			if (queueing || (platoon.activeWaypoint != null && !platoon.activeWaypoint.interrupt ())) {
				platoon.waypoints.Enqueue (newWaypoint);
			} else {
				platoon.activeWaypoint = newWaypoint;
				newWaypoint.processWaypoint ();
			}
		}

		protected abstract Waypoint getModuleWaypoint ();
	}

	public class MovementModule : PlatoonModule, Matchable<Vector3>	{
		MoveWaypoint newWaypoint {
			get {
				return base.newWaypoint as MoveWaypoint;
			}
		}

		Vector3 finalHeading;

		public MovementModule (PlatoonBehaviour p)
			: base (p)
		{

		}

		public override void Update ()
		{
            
		}

		public void setDestination (Vector3 v)
		{
			var finalHeading = v - getFunctionalPosition ();
			setFinalOrientation (v, finalHeading.getRadianAngle ());
		}

		public void getDestinationFromGhost ()
		{
			var heading = platoon.ghostPlatoon.GetComponent<GhostPlatoonBehaviour> ().finalHeading;
			setFinalOrientation (platoon.ghostPlatoon.transform.position, heading);
		}

		public void getHeadingFromGhost ()
		{
			var heading = platoon.ghostPlatoon.GetComponent<GhostPlatoonBehaviour> ().finalHeading;
			setFinalOrientation (newWaypoint.destination, heading);
		}

		private Vector3 getFunctionalPosition ()
		{
			var moveWaypoint = platoon.waypoints.Where (x => x is MoveWaypoint);
			if (moveWaypoint.Count () > 0) {
				return (moveWaypoint.Last () as MoveWaypoint).destination;
			} else {
				return platoon.transform.position;
			}

		}

		public void setFinalOrientation (Vector3 v, float h)
		{
			newWaypoint.destination = v;
			newWaypoint.heading = h;
		}

		public void setMatch (Vector3 match)
		{
			platoon.ghostPlatoon.setVisible (false);
			setDestination (match);
		}

		public float getScore (Vector3 matchees)
		{
			Vector3 pos = getFunctionalPosition ();

			return (matchees - pos).magnitude;
		}

		protected override Waypoint getModuleWaypoint ()
		{
			return new MoveWaypoint (platoon);
		}
        
        
	}

	public class TransporterModule : PlatoonModule, Matchable<PlatoonBehaviour.TransportableModule>
	{
		public PlatoonBehaviour transported;

		public TransporterWaypoint newWaypoint {
			get {
				return base.newWaypoint as TransporterWaypoint;
			}
		}

		public TransporterModule (PlatoonBehaviour p) : base (p)
		{
		}

		public void load ()
		{
			newWaypoint.loading = true;
		}

		public void unload ()
		{
			newWaypoint.loading = false;
		}

		public void setTransported (PlatoonBehaviour p)
		{
			transported = p;
			for (int i = 0; i < platoon.units.Count; i++) {
                
				if (p != null) {
					if (i == p.units.Count)
						break;
					platoon.units [i].GetComponent<TransporterBehaviour> ().transported = p.units [i] as InfantryBehaviour;
				} else
					platoon.units [i].GetComponent<TransporterBehaviour> ().transported = null;
			}
		}

		protected override Waypoint getModuleWaypoint ()
		{
			return new TransporterWaypoint (platoon, this);
		}

		public void setMatch (PlatoonBehaviour.TransportableModule match)
		{
			newWaypoint.loading = true;
			match.beginQueueing (queueing);
			match.setTransport (this);
			newWaypoint.transportableWaypoint = match.newWaypoint;
			match.endQueueing ();
		}

		public float getScore (PlatoonBehaviour.TransportableModule matchees)
		{
			return platoon.movement.getScore (matchees.platoon.transform.position);
		}
	}
	/*public enum Modules
    {
        MovementModule
    }*/
	public class TransportableModule : PlatoonModule
	{
		public TransportableWaypoint newWaypoint {
			get {
				return base.newWaypoint as TransportableWaypoint;
			}
		}

		public TransportableModule (PlatoonBehaviour p) : base (p)
		{
		}

		protected override Waypoint getModuleWaypoint ()
		{
			return new TransportableWaypoint (platoon);
		}

		public void setTransport (PlatoonBehaviour.TransporterModule transport)
		{
            
			newWaypoint.transporterWaypoint = transport.newWaypoint;
            
		}
	}
}

public abstract class Waypoint
{
	public bool interrupted;
	public PlatoonBehaviour platoon;

	public Waypoint (PlatoonBehaviour p)
	{
		platoon = p;
	}

	public abstract void processWaypoint ();

	public abstract bool orderComplete ();

	public abstract bool interrupt ();
}

public class MoveWaypoint : Waypoint
{
	public Vector3 destination;
	public float heading;

	public MoveWaypoint (PlatoonBehaviour p) : base (p)
	{
	}

	public override void processWaypoint ()
	{
		Vector3 v = new Vector3 (Mathf.Cos (heading), 0, Mathf.Sin (heading));
		var left = new Vector3 (-v.z, 0, v.x);

		var pos = destination + (platoon.units.Count - 1) * (PlatoonBehaviour.baseDistance / 2) * left;
		var destinations = new List<Vector3> ();
		for (int i = 0; i < platoon.units.Count; i++) {
			destinations.Add (pos - PlatoonBehaviour.baseDistance * i * left);
		}

		platoon.units.ConvertAll (x => x as Matchable<Vector3>).Match (destinations);
		platoon.units.ForEach (x => x.setUnitFinalHeading (heading));
	}

	public override bool orderComplete ()
	{
		return platoon.units.All (x => x.ordersComplete ());
	}

	public override bool interrupt ()
	{
		//platoon.units.ForEach (x => x.gotDestination = false);
		return true;
	}
}

public class TransporterWaypoint : Waypoint
{
	public bool loading;
	PlatoonBehaviour.TransporterModule module;
	public TransportableWaypoint transportableWaypoint;
	//public PlatoonBehaviour target;
	public TransporterWaypoint (PlatoonBehaviour p, PlatoonBehaviour.TransporterModule m) : base (p)
	{
		module = m;
	}

	public override void processWaypoint ()
	{
		if (loading) {
			if (transportableWaypoint == null)
				return;
			for (int i = 0; i < transportableWaypoint.platoon.units.Count; i++) {
				platoon.units [i].GetComponent<TransporterBehaviour> ().load (transportableWaypoint.platoon.units [i] as InfantryBehaviour);
			}
		} else {
			if (module.transported == null)
				return;
			module.transported.setEnabled (true);
			module.transported = null;
			platoon.units.ForEach (x => x.GetComponent<TransporterBehaviour> ().unload ());
		}
	}

	public override bool orderComplete ()
	{
		if (transportableWaypoint != null && transportableWaypoint.interrupted) {
			platoon.units.ForEach (x => x.GetComponent<TransporterBehaviour> ().target = null);
			return true;
		}
		if (loading) {
			if (transportableWaypoint.orderComplete ()) {
				module.setTransported (transportableWaypoint.platoon);
				platoon.units.ForEach (x => x.GetComponent<TransporterBehaviour> ().target = null);
				transportableWaypoint.platoon.setEnabled (false);
				return true;
			} else {
				return false;
			}
            //platoon.units.All(x => x.GetComponent<TransporterBehaviour>().loadingComplete());//premature true
		} else {
			if (platoon.units.All (x => x.GetComponent<TransporterBehaviour> ().unloadingComplete ())) {
				module.setTransported (null);
				return true;
			} else {
				return false;
			}
		}
	}

	public override bool interrupt ()
	{
		if (transportableWaypoint != null && transportableWaypoint.interrupt ()) {
			platoon.units.ForEach (x => x.GetComponent<TransporterBehaviour> ().target = null);
			Debug.Log ("transport interupted");
			interrupted = true;
			return true;
		} else {
			return false;
		}
        
	}
}

public class TransportableWaypoint : Waypoint
{
	public TransporterWaypoint transporterWaypoint;

	public TransportableWaypoint (PlatoonBehaviour p)
		: base (p)
	{

	}

	public override void processWaypoint ()
	{
		for (int i = 0; i < platoon.units.Count; i++) {
			(platoon.units [i] as InfantryBehaviour).setTransportTarget (transporterWaypoint.platoon.units [i].GetComponent<TransporterBehaviour> ());
		}
        
	}

	public override bool orderComplete ()
	{
		if (transporterWaypoint.interrupted) {
			platoon.units.ForEach (x => (x as InfantryBehaviour).setTransportTarget (null));
			return true;
		} else {
			if (!platoon.units.Any (x => (x as InfantryBehaviour).interactsWithTransport (false))) {
                
				return true;
			} else {
				return false;
			}
		}
		//return transporterWaypoint.interrupted || platoon.units.All(x => !(x as InfantryBehaviour).interactsWithTransport(true));
	}

	public override bool interrupt ()
	{
		if (!platoon.units.Any (x => (x as InfantryBehaviour).interactsWithTransport (true))) {
            
			interrupted = true;
			return true;
		} else {
			return false;
		}
        
	}
}
