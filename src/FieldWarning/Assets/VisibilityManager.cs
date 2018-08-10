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
public class VisibilityManager : MonoBehaviour
{
    int turn = 0;
    int groups = 10;
    //public static Team currentTeam = Team.Red;
    static List<VisibleBehavior> teamMembersBlue = new List<VisibleBehavior>();
    static List<VisibleBehavior> teamMembersRed = new List<VisibleBehavior>();
    static List<VisibleBehavior>[,] visionCellsBlue;
    static List<VisibleBehavior>[,] visionCellsRed;

    int n;
    static float mapSize = 1000;
    static float maxViewDistance = 50;
    public static float minViewDistance = 10;
    // Use this for initialization
    void Start()
    {
        n = Mathf.CeilToInt(mapSize / maxViewDistance);

        teamMembersBlue = new List<VisibleBehavior>();
        teamMembersRed = new List<VisibleBehavior>();
        visionCellsBlue = new List<VisibleBehavior>[n, n];
        visionCellsRed = new List<VisibleBehavior>[n, n];

        //Debug.Log(getVisionCells(Team.Blue));
    }

    // Update is called once per frame
    void Update()
    {
        updateVision();
    }

    public static void addVisibleBehaviour(VisibleBehavior b)
    {
        var members = getTeamMembers(b.Team);
        if (!members.Contains(b)) members.Add(b);
        var t = Team.Blue;
        if (t == b.Team) t = Team.Red;
        getTeamMembers(t).ForEach(x => x.AddHostile(b));
    }

    public void updateVision()
    {
        //Debug.Log(teamMembersBlue.Count );
        //Debug.Log(teamMembersRed.Count);
        //detectedHostile.RemoveAll(x => detected(x));
        turn++;
        foreach (var t in new Team[] { Team.Blue, Team.Red }) {
            Team o = Team.Red;
            if (t == Team.Red) o = Team.Blue;
            var team = getTeamMembers(t);
            for (var k = turn % groups; k < team.Count; k += groups) {
                var unit = team[k];
                var region = unit.GetRegion();
                for (int i = -1; i < 2; i++) {
                    for (int j = -1; j < 2; j++) {

                        var x = region.x + i;
                        var y = region.y + j;
                        if (x < 0 || y < 0 || x >= n || y >= n) continue;
                        //Debug.Log(string.Format("searched: {0}, {1}", x,y));
                        //visionCells[x, y].RemoveAll(vis=> notDetected(vis, unit));
                        foreach (var enemy in getVisionCells(o)[x, y]) {
                            if (TerrainData.visionScore(unit.transform, enemy.transform, maxViewDistance)) {
                                unit.SetSpotting(enemy, true);
                                enemy.SetSpottedBy(unit, true);
                            } else {
                                unit.SetSpotting(enemy, false);
                                enemy.SetSpottedBy(unit, false);
                            }
                        }
                    }
                }
            }
        }
    }
    /*private bool notDetected(VisibleBehavior enemy, VisibleBehavior unit)
    {
        
        if (TerrainBuilder.visionScore(unit.transform, enemy.transform,maxViewDistance))
        {
            detectedHostile.Add(enemy);
            enemy.setDetected(unit);
            return true;
        }
        else
        {
            enemy.setDetected(null);
            return false;
        }
    }*/
    /*private bool detected(VisibleBehavior vis)
    {
        if (TerrainBuilder.visionScore(vis.getDetectedBy().transform, vis.transform, maxViewDistance))
        {
            return false;
        }
        else
        {
            var region=vis.getRegion();
            visionCells[region.x, region.y].Add(vis);
            return true;
        }
    }*/
    public static void updateUnitRegion(VisibleBehavior unit, Point newRegion)
    {
        var currentPoint = unit.GetRegion();
        getVisionCells(unit.Team)[currentPoint.x, currentPoint.y].Remove(unit);
        getVisionCells(unit.Team)[newRegion.x, newRegion.y].Add(unit);
    }
    public static Point getRegion(Transform transform)
    {
        var x = Mathf.FloorToInt((mapSize / 2 + transform.position.x) / maxViewDistance);
        var y = Mathf.FloorToInt((mapSize / 2 + transform.position.z) / maxViewDistance);
        return new Point(x, y);
    }
    public static List<VisibleBehavior> getTeamMembers(Team t)
    {
        if (t == Team.Blue) {
            return teamMembersBlue;
        } else {
            return teamMembersRed;
        }
    }
    private static List<VisibleBehavior>[,] getVisionCells(Team t)
    {
        if (t == Team.Blue) {
            return visionCellsBlue;
        } else {
            return visionCellsRed;
        }
    }
    public static void updateTeamBelonging()
    {
        teamMembersBlue.ForEach(x => x.UpdateTeamBelonging());
        teamMembersRed.ForEach(x => x.UpdateTeamBelonging());
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
