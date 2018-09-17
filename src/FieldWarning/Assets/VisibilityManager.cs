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

using PFW.Model.Game;

// This class currently does not do anything.
public class VisibilityManager : MonoBehaviour
{
    public MatchSession Session;

    public Team LocalTeam;

    private List<VisibleBehavior> AllyUnits = new List<VisibleBehavior>();
    private List<VisibleBehavior> EnemyUnits = new List<VisibleBehavior>();

    // Old code:

    private static readonly float MAP_SIZE = 1000;
    private static readonly float MAX_VIEW_DISTANCE = 50;
    private static readonly int REGIONS_COUNT = Mathf.CeilToInt(MAP_SIZE / MAX_VIEW_DISTANCE);
    public static readonly float MIN_VIEW_DISTANCE = 10;

    void Update()
    {
        foreach (var unit in AllyUnits) 
            unit.ScanForEnemies();

        foreach (var unit in EnemyUnits) {
            unit.ScanForEnemies();
            unit.MaybeHideFromEnemies();
        }

        // Potential optimizations:
        // - Keep a table of distances and only update on moving units
        // - Keep a table of regions and only update when units enter/leave regions

        // TODO get this legacy code to work:
        //foreach (var unit in AllyUnits) {
        //    foreach (var enemy in EnemyUnits) {

        //        if (TerrainData.visionScore(unit.transform, enemy.transform, MAX_VIEW_DISTANCE)) {
        //            unit.SetSpotting(enemy, true);
        //            enemy.SetSpottedBy(unit, true);
        //        } else {
        //            unit.SetSpotting(enemy, false);
        //            enemy.SetSpottedBy(unit, false);
        //        }
        //    }
        //}
    }

    public void RegisterUnitBirth(UnitBehaviour unit)
    {
        VisibleBehavior visibleBehavior = unit.VisibleBehavior;

        if (unit.Platoon.Owner.Team == LocalTeam) {
            AllyUnits.Add(visibleBehavior);

        } else {
            EnemyUnits.Add(visibleBehavior);
            visibleBehavior.ToggleUnitVisibility(false);
        }
    }

    public void RegisterUnitDeath(UnitBehaviour unit)
    {
        VisibleBehavior visibleBehavior = unit.VisibleBehavior;
        if (visibleBehavior == null)
            return;


        if (unit.Platoon.Owner.Team == LocalTeam)
            AllyUnits.Remove(visibleBehavior);
        else
            EnemyUnits.Remove(visibleBehavior);
    }

    public static void UpdateUnitRegion(VisibleBehavior unit, Point newRegion)
    {
        //var currentPoint = unit.GetRegion();
    }

    public static Point GetRegion(Transform transform)
    {
        var x = Mathf.FloorToInt((MAP_SIZE / 2 + transform.position.x) / MAX_VIEW_DISTANCE);
        var y = Mathf.FloorToInt((MAP_SIZE / 2 + transform.position.z) / MAX_VIEW_DISTANCE);
        return new Point(x, y);
    }

    public void UpdateTeamBelonging(Team newTeam)
    {
        var tmp = AllyUnits;
        AllyUnits = EnemyUnits;
        EnemyUnits = tmp;

        AllyUnits.ForEach(u => u.ToggleUnitVisibility(true));
        EnemyUnits.ForEach(u => u.ToggleUnitVisibility(false));
        AllyUnits.ForEach(u => u.ScanForEnemies());

        LocalTeam = newTeam;
    }
}

public struct Point
{
    public int x;
    public int y;
    public Point(int x = 0, int y = 0)
    {
        this.x = x;
        this.y = y;
    }
    public static bool operator ==(Point p1, Point p2)
    {
        return (p1.x == p2.x && p1.y == p2.y);
    }
    public static bool operator !=(Point p1, Point p2)
    {
        return !(p1.x == p2.x && p1.y == p2.y);
    }
    public override string ToString()
    {
        return string.Format("[{0}, {1}]", x, y);
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Point)) {
            return false;
        }

        var point = (Point)obj;
        return x == point.x &&
               y == point.y;
    }

    public override int GetHashCode()
    {
        var hashCode = 1502939027;
        hashCode = hashCode * -1521134295 + x.GetHashCode();
        hashCode = hashCode * -1521134295 + y.GetHashCode();
        return hashCode;
    }
}
