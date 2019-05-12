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

using PFW.UI.Prototype;
using PFW.UI.Ingame;
using PFW.Model.Game;
using PFW.Units;

using PFW.Units.Component.Movement;
using PFW.Model.Armory;

public partial class PlatoonBehaviour : MonoBehaviour
{
    public Unit Unit;
    public IconBehaviour Icon;
    public MovementModule Movement;
    public Waypoint ActiveWaypoint;
    public TransporterModule Transporter;
    public TransportableModule Transportable;
    public GhostPlatoonBehaviour GhostPlatoon;
    public Queue<Waypoint> Waypoints = new Queue<Waypoint>();
    public List<UnitDispatcher> Units = new List<UnitDispatcher>();
    private List<PlatoonModule> _modules = new List<PlatoonModule>();
    public bool IsInitialized = false;

    public static readonly float UNIT_DISTANCE = 40*TerrainConstants.MAP_SCALE;

    public PlayerData Owner { get; private set; }

    public void Update()
    {
        Vector3 pos = new Vector3();

        Units.ForEach(x => pos += x.Transform.position);
        transform.position = pos / Units.Count;
        _modules.ForEach(x => x.Update());

        if (ActiveWaypoint == null || ActiveWaypoint.OrderComplete()) {
            if (Waypoints.Any()) {
                ActiveWaypoint = Waypoints.Dequeue();
                ActiveWaypoint.ProcessWaypoint();
            } else {
                ActiveWaypoint = null;
                //units.ForEach (x => x.gotDestination = false);
            }
            //setFinalOrientation(waypoint.destination,waypoint.heading);
        }
    }

    public void BuildModules()
    {
        Movement = new MovementModule(this);

        //if (t == UnitType.AFV) {
        //    Transporter = new TransporterModule(this);
        //    _modules.Add(Transporter);
        //}

        //if (t == UnitType.Infantry) {
        //    Transportable = new TransportableModule(this);
        //    _modules.Add(Transportable);
        //}
    }

    public void Initialize(Unit unit, PlayerData owner, int n)
    {
        Unit = unit;
        Owner = owner;

        var iconInstance = Instantiate(Resources.Load<GameObject>("Icon"), transform);
        Icon = iconInstance.GetComponent<IconBehaviour>();
        Icon.BaseColor = Owner.Team.Color;

        var unitPrefab = Unit.Prefab;

        for (int i = 0; i < n; i++) {
            var unitInstance =
                Owner.Session.Factory.MakeUnit(unitPrefab, Owner.Team.Color);
            var unitBehaviour = unitInstance.GetComponent<MovementComponent>();
            UnitDispatcher unitDispatcher =
                    new UnitDispatcher(unitBehaviour, this);
            Units.Add(unitDispatcher);

            var collider = unitInstance.GetComponentInChildren<BoxCollider>();
            collider.enabled = true;
        }

        BuildModules();

        //if (t == UnitType.AFV) {
        //    var ghost = GhostPlatoonBehaviour.Build(UnitType.Infantry, owner, n);
        //    Transporter.SetTransported(ghost.GetRealPlatoon());
        //    ghost.SetOrientation(100 * Vector3.down, 0);
        //    ghost.SetVisible(false);
        //}

        Movement.SetDestination(Vector3.forward);

        Icon.SetSource(Units);

        IsInitialized = true;
    }


    public void SetGhostPlatoon(GhostPlatoonBehaviour obj)
    {
        GhostPlatoon = obj;
    }

    public void Spawn(Vector3 center)
    {
        transform.position = center;
        var heading = GhostPlatoon.GetComponent<GhostPlatoonBehaviour>().FinalHeading;

        var positions = Formations.GetLineFormation(center, heading, Units.Count);
        for (int i = 0; i < Units.Count; i++)
            Units[i].SetOriginalOrientation(positions[i], heading - Mathf.PI/2);

        Movement.BeginQueueing(false);
        Movement.GetDestinationFromGhost();
        Movement.EndQueueing();
        GhostPlatoon.SetVisible(false);

        Owner.Session.RegisterPlatoonBirth(this);
    }

    // Call when splitting a platoon
    public void InitializeAfterSplit(
            Unit unit,
            PlayerData owner,
            UnitDispatcher unitDispatcher,
            MoveWaypoint destination)
    {
        Unit = unit;
        Owner = owner;

        var iconInstance = Instantiate(Resources.Load<GameObject>("Icon"), transform);
        Icon = iconInstance.GetComponent<IconBehaviour>();
        Icon.BaseColor = Owner.Team.Color;

        unitDispatcher.Platoon = this;
        Units.Add(unitDispatcher);

        BuildModules();

        Movement.BeginQueueing(false);
        Movement.SetDestination(destination.Destination);
        Movement.EndQueueing();

        Icon.SetSource(Units);

        GhostPlatoon.SetVisible(false);

        Owner.Session.RegisterPlatoonBirth(this);
        IsInitialized = true;
    }

    // Create new platoons for all units
    public void Split(PlayerData owner)
    {
        foreach (UnitDispatcher unit in Units) {
            var ghost = Instantiate(Resources.Load<GameObject>("GhostPlatoon"));
            var platoon = Instantiate(Resources.Load<GameObject>("Platoon"));

            var pBehavior = platoon.GetComponent<PlatoonBehaviour>();
            var gBehavior = ghost.GetComponent<GhostPlatoonBehaviour>();

            pBehavior.GhostPlatoon = gBehavior;

            gBehavior.InitializeAfterSplit(Unit, owner);

            pBehavior.InitializeAfterSplit(Unit, owner, unit, Movement.Waypoint);
        }

        Destroy(GhostPlatoon);
        DestroyWithoutUnits();
    }

    // Called when a platoon enters or leaves the player's selection.
    // justPreviewing - true when the unit should be shaded as if selected, but the
    //                  actual selected set has not been changed yet
    public void SetSelected(bool selected, bool justPreviewing)
    {
        Icon?.SetSelected(selected);
        Units.ForEach(unit => unit.SetSelected(selected, justPreviewing));
    }

    public void SetEnabled(bool enabled)
    {
        this.enabled = enabled;
        Icon?.SetVisible(enabled);
    }

    public void SendFirePosOrder(Vector3 position)
    {
        Units.ForEach(u => u.SendFirePosOrder(position));
        PlayAttackCommandVoiceline();
    }

    /// <summary>
    /// Destroy just the platoon object, without touching its units.
    /// </summary>
    private void DestroyWithoutUnits()
    {
        Owner.Session.RegisterPlatoonDeath(this);
        Destroy(gameObject);
    }

    /// <summary>
    /// Destroy the platoon and all units in it.
    /// </summary>
    public void Destroy()
    {
        foreach (var p in Units)
            Destroy(p.GameObject);

        DestroyWithoutUnits();
    }

#region PlayVoicelines
// For the time being, always play the voiceline of the first unit
// Until we agree on a default unit in platoon that plays
    public void PlaySelectionVoiceline()
    {
        Units[0].PlaySelectionVoiceline();
    }

    public void PlayMoveCommandVoiceline()
    {
        Units[0].PlayMoveCommandVoiceline();
    }

    public void PlayAttackCommandVoiceline()
    {
        Units[0].PlayAttackCommandVoiceline();
    }
#endregion
}