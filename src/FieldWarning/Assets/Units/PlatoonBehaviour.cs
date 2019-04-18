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

using PFW.Ingame.Prototype;
using PFW.Ingame.UI;
using PFW.Model.Game;
using PFW.Units.Component.Weapon;
using PFW.Units;

public partial class PlatoonBehaviour : MonoBehaviour
{
    public UnitType Type;
    public IconBehaviour Icon;
    public MovementModule Movement;
    public Waypoint ActiveWaypoint;
    public TransporterModule Transporter;
    public TransportableModule Transportable;
    public GhostPlatoonBehaviour GhostPlatoon;
    public Queue<Waypoint> Waypoints = new Queue<Waypoint>();
    public List<UnitDispatcher> Units = new List<UnitDispatcher>();
    public List<PlatoonModule> Modules = new List<PlatoonModule>();
    public bool IsInitialized = false;

    public static readonly float UNIT_DISTANCE = 40*TerrainConstants.MAP_SCALE;

    public PlayerData Owner { get; private set; }

    public void Update()
    {
        var pos = new Vector3();

        Units.ForEach(x => pos += x.Transform.position);
        transform.position = pos / Units.Count;
        Modules.ForEach(x => x.Update());

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

    public void BuildModules(UnitType t)
    {
        Movement = new MovementModule(this);
        Modules.Add(Movement);

        if (t == UnitType.AFV) {
            Transporter = new TransporterModule(this);
            Modules.Add(Transporter);
        }

        if (t == UnitType.Infantry) {
            Transportable = new TransportableModule(this);
            Modules.Add(Transportable);
        }
    }

    public void Initialize(UnitType t, PlayerData owner, int n)
    {
        Type = t;
        Owner = owner;

        var iconInstance = Instantiate(Resources.Load<GameObject>("Icon"), transform);
        Icon = iconInstance.GetComponent<IconBehaviour>();
        Icon.BaseColor = Owner.Team.Color;

        var unitPrefab = Owner.Session.Factory.FindPrefab(t);

        for (int i = 0; i < n; i++) {
            var unitInstance =
                Owner.Session.Factory.MakeUnit(unitPrefab, Owner.Team.Color);
            var unitBehaviour = unitInstance.GetComponent<UnitBehaviour>();
            unitBehaviour.SetPlatoon(this);
            UnitDispatcher unit = new UnitDispatcher(unitBehaviour);
            Units.Add(unit);

            var collider = unitInstance.GetComponentInChildren<BoxCollider>();
            collider.enabled = true;
        }

        BuildModules(t);

        if (t == UnitType.AFV) {
            var ghost = GhostPlatoonBehaviour.Build(UnitType.Infantry, owner, n);
            Transporter.SetTransported(ghost.GetRealPlatoon());
            ghost.SetOrientation(100 * Vector3.down, 0);
            ghost.SetVisible(false);
        }

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
    public void SplitInitialize(UnitType t, PlayerData owner, UnitDispatcher u)
    {
        Type = t;
        Owner = owner;

        var iconInstance = Instantiate(Resources.Load<GameObject>("Icon"), transform);
        Icon = iconInstance.GetComponent<IconBehaviour>();
        Icon.BaseColor = Owner.Team.Color;

        u.SetPlatoon(this);
        Units.Add(u);

        BuildModules(t);

        Movement.SetDestination(Vector3.forward);

        Icon.SetSource(Units);


        IsInitialized = true;
    }

    // Create new platoons for all units
    public void Split(PlayerData owner)
    {
        foreach (var unit in Units) {
            var ghost = Instantiate(Resources.Load<GameObject>("GhostPlatoon"));
            var plat = Instantiate(Resources.Load<GameObject>("Platoon"));

            var pBehavior = plat.GetComponent<PlatoonBehaviour>();
            var gBehavior = ghost.GetComponent<GhostPlatoonBehaviour>();

            pBehavior.SplitInitialize(Type, owner, unit);

            pBehavior.GhostPlatoon = gBehavior;
            gBehavior.SplitInitialize(Type, owner, unit.GameObject);
        }
        Destroy(gameObject);
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

    public void Destroy()
    {
        foreach (var p in Units)
            Destroy(p.GameObject);

        Owner.Session.RegisterPlatoonDeath(this);
        Destroy(gameObject);
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