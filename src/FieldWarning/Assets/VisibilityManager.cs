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
        Application.targetFrameRate = -1;
        n = Mathf.CeilToInt(mapSize / maxViewDistance);

        teamMembersBlue = new List<VisibleBehavior>();
        teamMembersRed = new List<VisibleBehavior>();
        visionCellsBlue = new List<VisibleBehavior>[n, n];
        visionCellsRed = new List<VisibleBehavior>[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                visionCellsBlue[i, j] = new List<VisibleBehavior>();
                visionCellsRed[i, j] = new List<VisibleBehavior>();
            }
        }

        //Debug.Log(getVisionCells(Team.Blue));
        visTest();
    }

    // Update is called once per frame
    void Update()
    {
        updateVision();
    }
    private void visTest()
    {
        var go = Resources.Load<GameObject>("VisTest");
        var friend = GameObject.Instantiate<GameObject>(go);
        var friendBehaviour = friend.GetComponent<VisibleBehavior>();
        friendBehaviour.initialize(Team.Red);
        var n = 100;
        foreach (var t in new Team[] { Team.Blue, Team.Red })
        {
            Team o = Team.Red;
            var off = 50;
            if (t == Team.Red) off = -off;
            var offset = new Vector3(off, 0, 0);
            for (int i = -n; i <= n; i++)
            {

                var enemy = GameObject.Instantiate<GameObject>(go);
                //enemy.transform.parent = friend.transform;
                var behaviour = enemy.GetComponent<VisibleBehavior>();
                behaviour.initialize(t);
                //hostileTeam.Add(behaviour);
                var pos = 200 * UnityEngine.Random.insideUnitCircle;
                behaviour.transform.position = new Vector3(pos.x, 0, pos.y) + offset;
                //getTeamMembers(t).Add(behaviour);

            }
        }
        //teamMembersBlue.ForEach(x => x.setHostileTeam(teamMembersRed));
        //teamMembersRed.ForEach(x => x.setHostileTeam(teamMembersBlue));
    }
    public static void addVisibleBehaviour(VisibleBehavior b)
    {
        var members=getTeamMembers(b.team);
        if (!members.Contains(b)) members.Add(b);
        var t=Team.Blue;
        if (t == b.team) t = Team.Red;
        getTeamMembers(t).ForEach(x=>x.addHostile(b));
    }
    public void updateVision()
    {
        //Debug.Log(teamMembersBlue.Count );
        //Debug.Log(teamMembersRed.Count);
        //detectedHostile.RemoveAll(x => detected(x));
        turn++;
        foreach (var t in new Team[] { Team.Blue, Team.Red })
        {
            Team o = Team.Red;
            if (t == Team.Red) o = Team.Blue;
            var team=getTeamMembers(t);
            for (var k = turn % groups; k < team.Count;k+=groups )
            {
                var unit = team[k];
                var region = unit.getRegion();
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {

                        var x = region.x + i;
                        var y = region.y + j;
                        if (x < 0 || y < 0 || x >= n || y >= n) continue;
                        //Debug.Log(string.Format("searched: {0}, {1}", x,y));
                        //visionCells[x, y].RemoveAll(vis=> notDetected(vis, unit));
                        foreach (var enemy in getVisionCells(o)[x, y])
                        {
                            if (TerrainData.visionScore(unit.transform, enemy.transform, maxViewDistance))
                            {
                                unit.setSpotting(enemy, true);
                                enemy.setSpottedBy(unit, true);
                            }
                            else
                            {
                                unit.setSpotting(enemy, false);
                                enemy.setSpottedBy(unit, false);
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
        var currentPoint = unit.getRegion();
        getVisionCells(unit.team)[currentPoint.x, currentPoint.y].Remove(unit);
        getVisionCells(unit.team)[newRegion.x, newRegion.y].Add(unit);
    }
    public static Point getRegion(Transform transform)
    {
        var x = Mathf.FloorToInt((mapSize / 2 + transform.position.x) / maxViewDistance);
        var y = Mathf.FloorToInt((mapSize / 2 + transform.position.z) / maxViewDistance);
        return new Point(x, y);
    }
    public static List<VisibleBehavior> getTeamMembers(Team t)
    {
        if (t == Team.Blue)
        {
            return teamMembersBlue;
        }
        else
        {
            return teamMembersRed;
        }
    }
    private static List<VisibleBehavior>[,] getVisionCells(Team t)
    {
        if (t == Team.Blue)
        {
            return visionCellsBlue;
        }
        else
        {
            return visionCellsRed;
        }
    }
    public static void updateTeamBelonging()
    {
        teamMembersBlue.ForEach(x => x.updateTeamBelonging());
        teamMembersRed.ForEach(x => x.updateTeamBelonging());
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
    
}
