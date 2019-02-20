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
using PFW.Weapons;
using PFW.Units;

public abstract class UnitBehaviour : SelectableBehavior, Matchable<Vector3>
{
    public const string UNIT_TAG = "Unit";
    public const float NO_HEADING = float.MaxValue;
    private const float ORIENTATION_RATE = 5.0f;
    private const float TRANSLATION_RATE = 5.0f;

    public UnitData Data = UnitData.GenericUnit();
    public PlatoonBehaviour Platoon { get; private set; }
    public Pathfinder Pathfinder { get; private set; }
    
    [SerializeField]
    private GameObject _selectionCircle;
    [SerializeField]
    private VoiceComponent _voiceComponent;
    // TODO: This is only held by this class as a way to get it to VisibilityManager. Figure out the best way to do that.
    public VisibleBehavior VisibleBehavior;

    // These are set by the subclass in DoMovement()
    protected Vector3 _position;
    protected Vector3 _rotation;

    // Forward and right directions on the horizontal plane
    protected Vector3 _forward { get; private set; }
    protected Vector3 _right { get; private set; }

    // This is redundant with transform.rotation.localEulerAngles, but it is necessary because
    // the localEulerAngles will sometimes automatically change to some new equivalent angles
    private Vector3 _currentRotation;

    private TerrainCollider _Ground;
    protected TerrainCollider Ground {
        get {
            if (_Ground == null) {
                _Ground = GameObject.Find("Terrain").GetComponent<TerrainCollider>();
            }
            return _Ground;
        }
    }

    protected float _finalHeading;

    private Terrain _terrain;
    private float _health;

    public virtual void Awake()
    {
        if (_selectionCircle == null)
            throw new System.Exception(
                "unitBehaviour: Must have a reference to selection circle object");
    }

    public virtual void Start()
    {
        _health = Data.maxHealth; //set the health to 10 (from UnitData.cs)
        tag = UNIT_TAG;

        Platoon.Owner.Session.RegisterUnitBirth(this);
    }

    public virtual void Update()
    {
        DoMovement();

        if (IsMoving())
            UpdateMapOrientation();

        UpdateCurrentRotation();
        UpdateCurrentPosition();
    }

    private void UpdateCurrentPosition()
    {
        Vector3 diff = (_position - transform.position) * Time.deltaTime;
        Vector3 newPosition = transform.position;
        newPosition.x += TRANSLATION_RATE * diff.x;
        newPosition.y = _position.y;
        newPosition.z += TRANSLATION_RATE * diff.z;

        transform.position = newPosition;
    }

    private void UpdateCurrentRotation()
    {
        Vector3 diff = _rotation - _currentRotation;
        if (diff.sqrMagnitude > 1) {
            _currentRotation = _rotation;
        } else {
            _currentRotation += ORIENTATION_RATE * Time.deltaTime * diff;
        }

        transform.localEulerAngles = Mathf.Rad2Deg * new Vector3(-_currentRotation.x, -_currentRotation.y, _currentRotation.z);
        _forward = new Vector3(-Mathf.Sin(_currentRotation.y), 0f, Mathf.Cos(_currentRotation.y));
        _right = new Vector3(_forward.z, 0f, -_forward.x);
    }

    public void HandleHit(float receivedDamage)
    {
        if (_health <= 0)
            return;

        _health -= receivedDamage;
        if (_health <= 0) 
            Destroy();        
    }

    public abstract void UpdateMapOrientation();

    public void SetPlatoon(PlatoonBehaviour p)
    {
        Platoon = p;
        Pathfinder = new Pathfinder(this, Platoon.Owner.Session.PathfinderData);
    }

    public float GetHealth()
    {
        return _health;
    }

    public void SetHealth(float health)
    {
        _health = health;
    }

    public override PlatoonBehaviour GetPlatoon()
    {
        return Platoon;
    }

    // Waypoint-aware path setting. TODO there are like 5 methods for this,
    // perhaps some can be cut?
    public void SetUnitDestination(MoveWaypoint waypoint)
    {
        MoveCommandType moveType;
        // TODO we have two enums for the same thing, remove one:
        switch (waypoint.moveMode) {
        case MoveWaypoint.MoveMode.fastMove:
            moveType = MoveCommandType.Fast;
            break;
        case MoveWaypoint.MoveMode.normalMove:
            moveType = MoveCommandType.Slow;
            break;
        case MoveWaypoint.MoveMode.reverseMove:
            moveType = MoveCommandType.Reverse;
            break;
        default:
            throw new System.Exception("Impossible state");
        }

        float a = Pathfinder.SetPath(waypoint.Destination, moveType);
        if (a < Pathfinder.Forever)
            SetUnitFinalHeading(waypoint.Heading);
    }

    public void PlayMoveCommandVoiceline()
    {
        _voiceComponent.PlayMoveCommandVoiceline();
    }

    // Sets the unit's destination location, with a default heading value
    public void SetUnitDestination(Vector3 v)
    {
        //var diff = (v - transform.position);
        //SetFinalOrientation(v, diff.getRadianAngle());
        SetFinalOrientation(v, NO_HEADING);
    }

    // Sets the unit's destination location, with a specific given heading value
    public void SetFinalOrientation(Vector3 d, float heading)
    {
        if (Pathfinder.SetPath(d, MoveCommandType.Fast) < Pathfinder.Forever)
            SetUnitFinalHeading(heading);        
    }

    // Updates the unit's final heading so that it faces the specified location
    public void SetUnitFinalFacing(Vector3 v)
    {
        Vector3 diff;
        if (Pathfinder.HasDestination())
            diff = v - Pathfinder.GetDestination();
        else
            diff = v - transform.position;
        
    
        SetUnitFinalHeading(diff.getRadianAngle());
    }

    // Updates the unit's final heading to the specified value 
    public virtual void SetUnitFinalHeading(float heading)
    {
        _finalHeading = heading;
    }

    protected abstract void DoMovement();

    protected abstract bool IsMoving();

    public void SetLayer(int l)
    {
        gameObject.layer = l;
    }

    protected abstract Renderer[] GetRenderers();

    public void SetVisible(bool vis)
    {
        var renderers = GetRenderers();
        foreach (var r in renderers)
            r.enabled = vis;        

        if (vis)
            SetLayer(LayerMask.NameToLayer("Selectable"));
        else 
            SetLayer(LayerMask.NameToLayer("Ignore Raycast"));        
    }

    protected float getHeading()
    {
        return (Pathfinder.GetDestination() - transform.position).getDegreeAngle();
    }


    public void SetMatch(Vector3 match)
    {
        SetUnitDestination(match);
    }

    public float GetScore(Vector3 matchee)
    {
        return (matchee - transform.position).magnitude;
    }

    public abstract void SetOriginalOrientation(Vector3 pos, float heading, bool wake = true);


    protected void WakeUp()
    {
        enabled = true;
        SetVisible(true);
        foreach (Weapon weapon in gameObject.GetComponents<Weapon>())
            weapon.WakeUp();
    }

    public abstract bool OrdersComplete();

    // Returns the unit's speed on the current terrain
    public float GetTerrainSpeedMultiplier()
    {
        float terrainSpeed = Data.mobility.GetUnitSpeed(Pathfinder.data.terrain, Pathfinder.data.map, transform.position, 0f, -transform.forward);
        //terrainSpeed = Mathf.Max(terrainSpeed, 0.5f * TerrainConstants.MAP_SCALE); // Never let the speed to go exactly 0, just so units don't get stuck
        return terrainSpeed;
    }

    public void Destroy()
    {
        Platoon.Owner.Session.RegisterUnitDeath(this);
        
        Platoon.Units.Remove(this);
        Destroy(this.gameObject);

        Platoon.GhostPlatoon.HandleRealUnitDestroyed();

        if (Platoon.Units.Count == 0) {
            Destroy(Platoon.gameObject);
            Platoon.Owner.Session.RegisterPlatoonDeath(Platoon);
        }
    }

    // Called when a unit enters or leaves the player's selection.
    // justPreviewing - true when the unit should be shaded as if selected, but the
    //                  actual selected set has not been changed yet
    public void SetSelected(bool selected, bool justPreviewing)
    {
        _selectionCircle.SetActive(selected);
        if (!justPreviewing) {
            _voiceComponent.PlayUnitSelectionVoiceline(selected);
        }
    }
}

public enum MoveCommandType
{
    Fast,
    Slow,
    Reverse
}

