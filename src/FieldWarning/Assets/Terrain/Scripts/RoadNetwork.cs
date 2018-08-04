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
using Priority_Queue;
using System;

public class RoadNetwork : MonoBehaviour
{
    const float roadNodeStep = 10;
    const int rays = 20;
    const int invalidSector = 35;
    const int connectionLimit = 4;
    static SimplePriorityQueue<BranchCandidate> branchingQueue = new SimplePriorityQueue<BranchCandidate>();
    static List<RoadNode> roadNodes = new List<RoadNode>();
    static List<RoadStretch> roadStretches = new List<RoadStretch>();
    static float[,] population;

    public static void BuildNetwork()
    {
        var origin = new RoadNode();
        //TODO:set origin position
        origin.BuildBranchCandidate();
        /*while (branchingQueue.Count > 0)
        {
            branchingQueue.Dequeue().acceptBranchCandidate();
        }*/
    }

    void Update()
    {
        if (branchingQueue.Count == 0) return;
        if (Input.GetKeyDown(KeyCode.Space)) {
            branchingQueue.Dequeue().AcceptBranchCandidate();
            Debug.Log("branch accepted");
        }
    }

    public static RoadNode ResolveRoadNodePosition(Vector3 origin, float heading)
    {
        RoadNode outNode;
        //check segment crossing
        //check node proximity
        //otherwise
        outNode = new RoadNode();
        outNode.position = origin + Quaternion.AngleAxis(heading, Vector3.up) * (roadNodeStep * Vector3.forward);
        return outNode;
    }

    class BranchCandidate
    {
        public float heading;
        public float score;
        public RoadNode origin;

        public BranchCandidate(RoadNode origin)
        {
            this.origin = origin;
        }

        public void AcceptBranchCandidate()
        {
            if (origin.GetHeadingScore(heading) == score && origin.Valid(heading)) {
                var newNode = ResolveRoadNodePosition(origin.position, heading);

                newNode.connections.Add(origin);
                newNode.BuildBranchCandidate();
                origin.connections.Add(newNode);
                //debug
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.position = newNode.position;
            }
            //origin.buildBranchCandidate();
        }
    }

    public class RoadNode
    {
        public Vector3 position;
        public List<RoadNode> connections = new List<RoadNode>();
        Dictionary<RoadNode, RoadStretch> roadConnections = new Dictionary<RoadNode, RoadStretch>();
        BranchCandidate branchCandidate;
        public RoadNode()
        {
            roadNodes.Add(this);
        }
        public void BuildBranchCandidate()
        {
            var bestHeading = 0;
            var bestHeadingScore = Single.NegativeInfinity;
            for (int i = 0; i < rays; i++) {
                var heading = (360 * i) / rays;

                if (!Valid(heading)) continue;
                float score = GetHeadingScore(heading);

                if (score > bestHeadingScore) {
                    bestHeadingScore = score;
                    bestHeading = heading;
                }
            }
            if (bestHeadingScore != Single.NegativeInfinity) {
                branchCandidate = new BranchCandidate(this);

                branchCandidate.heading = bestHeading;
                branchCandidate.score = bestHeadingScore;
                branchingQueue.Enqueue(branchCandidate, bestHeadingScore);
            }
        }

        public float GetHeadingScore(float heading)
        {
            var resolution = .5f;
            var offset = Quaternion.AngleAxis(heading, Vector3.up) * (resolution * Vector3.forward);

            return TerrainData.populationScore(position, offset);

        }

        public bool Valid(float heading)
        {
            if (connections.Count >= connectionLimit) return false;
            foreach (var connection in connections) {
                var angle = (connection.position - position).getDegreeAngle();
                if (Mathf.Abs((angle - heading).unwrapDegree()) < invalidSector) {
                    return false;
                }
            }
            return true;
        }

    }

    class RoadStretch
    {
        RoadNode node1;
        RoadNode node2;
    }
}
