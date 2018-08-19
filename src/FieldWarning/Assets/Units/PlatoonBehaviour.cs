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
using Pfw.Ingame.Prototype;

using PFW.Ingame.UI;
using PFW.Model.Game;

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
    public List<UnitBehaviour> Units = new List<UnitBehaviour>();
    public List<PlatoonModule> Modules = new List<PlatoonModule>();
    public bool IsInitialized = false;

    public static readonly float UNIT_DISTANCE = 4;

    public Player Owner { get; private set; }

    public void Update()
    {
        var pos = new Vector3();

        Units.ForEach(x => pos += x.transform.position);
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

    public void Initialize(UnitType t, Player owner, int n)
    {
        Type = t;
        Owner = owner;

        Owner.Session.RegisterPlatoonBirth(this);

        var iconInstance = Instantiate(Resources.Load<GameObject>("Icon"), transform);
        Icon = iconInstance.GetComponent<IconBehaviour>();
        Icon.BaseColor = Owner.Team.Color;

        var unitInstance = UnitFactory.GetUnit(t);

        for (int i = 0; i < n; i++) {
            iconInstance = Instantiate(unitInstance);
            var unitBehaviour = iconInstance.GetComponent<UnitBehaviour>();
            unitBehaviour.SetPlatoon(this);
            Units.Add(unitBehaviour);

            var collider = iconInstance.GetComponentInChildren<BoxCollider>();
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

    public void Spawn(Vector3 pos)
    {
        transform.position = pos;

        var heading = GhostPlatoon.GetComponent<GhostPlatoonBehaviour>().FinalHeading;
        Vector3 forward = new Vector3(Mathf.Cos(heading), 0, Mathf.Sin(heading));
        float spawndistance = 2;

        for (int i = 0; i < Units.Count; i++)
            Units[i].SetOriginalOrientation(pos + i * spawndistance * forward, Quaternion.FromToRotation(Vector3.forward, forward));

        Movement.BeginQueueing(false);
        Movement.GetDestinationFromGhost();
        Movement.EndQueueing();
        GhostPlatoon.SetVisible(false);
    }

    public void SetSelected(bool selected)
    {
        Icon?.SetSelected(selected);
    }

    public void SetEnabled(bool enabled)
    {
        this.enabled = enabled;
        Icon?.SetVisible(enabled);
    }

    public void SendFirePosOrder(Vector3 position)
    {
        foreach (var unit in Units) {
            var weapons = unit.GetComponents<Weapon>();

            foreach (var weapon in weapons)
                weapon.setTarget(position);
        }
    }

    public void Destroy()
    {
        foreach (var p in Units)
            Destroy(p.gameObject);

        Owner.Session.RegisterPlatoonDeath(this);
        Destroy(gameObject);
    }
}