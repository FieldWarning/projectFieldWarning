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

using PFW.Ingame.UI;

public abstract class UnitBehaviour : SelectableBehavior, Matchable<Vector3>
{
    public const string UNIT_TAG = "Unit";

    public UnitData Data = UnitData.GenericUnit();
    public PlatoonBehaviour platoon { get; private set; }
    public bool IsAlive { get; private set; }
    public Pathfinder pathfinder { get; private set; }
    public AudioSource source { get; private set; }

    protected TerrainCollider Ground {
        get {
            if (_Ground == null) {
                _Ground = GameObject.Find("Terrain").GetComponent<TerrainCollider>();
            }
            return _Ground;
        }
    }

    protected float finalHeading;

    private Terrain _terrain;
    private TerrainCollider _Ground;
    private float _health;
    
    public virtual void Start()
    {
        transform.position = 100 * Vector3.down;
        enabled = false;
        _health = Data.maxHealth; //set the health to 10 (from UnitData.cs)
        IsAlive = true;
        SetVisible(false);
        tag = UNIT_TAG;

        source = GetComponent<AudioSource>();

        pathfinder = new Pathfinder(this, PathfinderData.singleton);

    }
    
    public virtual void Update()
    {
        if (IsMoving())
            UpdateMapOrientation();
        DoMovement();
    }

    public void HandleHit(float receivedDamage)
    {
        if (_health <= 0)
            return;

        _health -= receivedDamage;
        if (_health <= 0) {
            IsAlive = false;
            platoon.Units.Remove(this);

            Destroy(this.gameObject);
            platoon.GhostPlatoon.HandleRealUnitDestroyed();
            if (platoon.Units.Count == 0) {
                Destroy(platoon.gameObject);
                platoon.Owner.Session.RegisterPlatoonDeath(platoon);
            }

            return;
        }
    }

    public abstract void UpdateMapOrientation();

    public void SetPlatoon(PlatoonBehaviour p)
    {
        platoon = p;
    }

    public float GetHealth()
    {
        return _health;
    }

    public void SetHealth(float health)
    {
        this._health = health;
    }

    public override PlatoonBehaviour GetPlatoon()
    {
        return platoon;
    }

    // Sets the unit's destination location, with a default heading value
    public void SetUnitDestination(Vector3 v)
    {
        var diff = (v - transform.position);
        SetFinalOrientation(v, diff.getRadianAngle());
    }

    // Sets the unit's destination location, with a specific given heading value
    public void SetFinalOrientation(Vector3 d, float heading)
    {
        if (pathfinder.SetPath(d, MoveCommandType.Fast) < Pathfinder.Forever) {
            SetUnitFinalHeading(heading);
        }
    }

    // Updates the unit's final heading so that it faces the specified location
    public void SetUnitFinalFacing(Vector3 v)
    {
        Vector3 diff;
        if (pathfinder.HasDestination()) {
            diff = v - pathfinder.GetDestination();
        } else {
            diff = v - transform.position;
        }
        SetUnitFinalHeading(diff.getRadianAngle());
    }

    // Updates the unit's final heading to the specified value 
    public virtual void SetUnitFinalHeading(float heading)
    {
        finalHeading = heading;
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
        foreach (var r in renderers) {
            r.enabled = vis;
        }

        if (vis) {
            SetLayer(LayerMask.NameToLayer("Selectable"));

        } else {
            SetLayer(LayerMask.NameToLayer("Ignore Raycast"));
        }
    }

    protected float getHeading()
    {
        return (pathfinder.GetDestination() - transform.position).getDegreeAngle();
    }


    public void SetMatch(Vector3 match)
    {
        SetUnitDestination(match);
    }

    public float GetScore(Vector3 matchee)
    {
        return (matchee - transform.position).magnitude;
    }

    public abstract void SetOriginalOrientation(Vector3 pos, Quaternion rotation, bool wake = true);


    protected void WakeUp()
    {
        enabled = true;
        SetVisible(true);
        foreach (Weapon weapon in gameObject.GetComponents<Weapon>())
            weapon.WakeUp();
    }

    public abstract bool OrdersComplete();

    // Returns the unit's speed on the current terrain
    public float GetTerrainSpeed()
    {
        float terrainSpeed = pathfinder.data.GetUnitSpeed(Data.mobility, transform.position, 0f, -transform.forward);
        terrainSpeed = Mathf.Max(terrainSpeed, 0.05f); // Never let the speed to go exactly 0, just so units don't get stuck
        return Data.movementSpeed * terrainSpeed;
    }

}

public enum MoveCommandType
{
    Fast,
    Slow
}

