using UnityEngine;
using System;
using System.Collections.Generic;
using Priority_Queue;

public class PathfinderData
{
	public static PathfinderData singleton;

	public static int NumMobilityTypes;
	private static PathArc InvalidArc = new PathArc (null, null);
	private const float GraphRadius = 0f;
	private const float SparseGridSpacing = 150f;

	public TerrainData terrain;
	public List<PathNode> graph;
	FastPriorityQueue<PathNode> openSet;

	public PathfinderData (TerrainData terrain)
	{
		NumMobilityTypes = Enum.GetNames (typeof(MobilityType)).Length;
		this.terrain = terrain;
		graph = new List<PathNode> ();
		BuildGraph ();
	}

	private void BuildGraph ()
	{
		graph.Clear ();

		// TODO: Add nodes for terrain features and roads


		// Fill in any big open spaces with a sparse grid in case the above missed anything important
		Vector3 size = TerrainBuilder.size;
		Vector3 newPos = new Vector3 (0f, 0f, 0f);
		for (float x = 0f; x < size.x; x += SparseGridSpacing / 10) {
			for (float z = 0f; z < size.z; z += SparseGridSpacing / 10) {
				newPos.Set (x, 0f, z);

				float minDist = float.MaxValue;
				foreach (PathNode node in graph)
					minDist = Mathf.Min (minDist, Vector3.Distance (newPos, node.position));

				if (minDist > SparseGridSpacing)
					graph.Add (new PathNode (newPos));
			}
		}

		openSet = new FastPriorityQueue<PathNode> (graph.Count + 1);

		// Compute arcs for all pairs of nodes
		for (int i = 0; i < graph.Count; i++) {
			for (int j = i + 1; j < graph.Count; j++) {
				AddArc (graph[i], graph[j]);
			}
		}

		// Remove unnecessary arcs
		// An arc in necessary if for any MobilityType, the direct path between the nodes is at least
		// as good as the optimal global path. This is a brute force approach and it might be too slow
		List<PathNode> path = new List<PathNode> ();
		for (int i = 0; i < graph.Count; i++) {
			for (int j = i + 1; j < graph.Count; j++) {

				PathArc arc = GetArc (graph[i], graph[j]);
				if (arc.Equals (InvalidArc))
					continue;

				bool necessary = false;
				for (int k = 0; k < NumMobilityTypes; k++) {
					if (arc.time[k] == Pathfinder.Forever)
						continue;

					float time = FindPath (path,
						graph[i].position, graph[j].position,
						(MobilityType)k, GraphRadius, MoveCommandType.Fast);
					if (arc.time[k] < time) {
						necessary = true;
						break;
					}
				}

				if (! necessary)
					RemoveArc (graph[i], graph[j]);
			}
		}
	}

	public PathArc GetArc (PathNode node1, PathNode node2)
	{
		for (int i = 0; i < node1.arcs.Count; i++) {
			PathArc arc = node1.arcs[i];
			if (arc.node1 == node2 || arc.node2 == node2)
				return arc;
		}
		return InvalidArc;
	}

	public void AddArc (PathNode node1, PathNode node2)
	{
		PathArc arc = new PathArc (node1, node2);
		node1.arcs.Add (arc);
		node2.arcs.Add (arc);

		// Compute the arc's traversal time for each MobilityType
		for (int i = 0; i < NumMobilityTypes; i++) {
			arc.time[i] = Pathfinder.FindLocalPath (
				this, node1.position, node2.position, (MobilityType)i, GraphRadius);
		}
	}

	public void RemoveArc (PathNode node1, PathNode node2)
	{
		PathArc arc = GetArc (node1, node2);
		node1.arcs.Remove (arc);
		node2.arcs.Remove (arc);
	}

	// Gives the relative speed of a unit with the given MobilityType at the given location
	// Relative speed is 0 if the terrain is impassible and 1 for road, otherwise between 0 and 1
	// If radius > 0, check for units in the way, otherwise just look at terrain
	public float GetUnitSpeed (MobilityType mobility, Vector3 location, float radius)
	{
		if (radius > 0f) {
			GameObject[] units = GameObject.FindGameObjectsWithTag ("Unit");
			foreach (GameObject unit in units) {
				float dist = Vector3.Distance (location, unit.transform.position);
				if (dist < radius + unit.GetComponent<UnitBehaviour> ().data.radius)
					return 0f;
			}
		}

		// TODO: find unit speed on terrain
		return 0.5f;
	}

	// Run the A* algorithm and put the result in path
	// If no path was found, return 'forever' and put only the destination in path
	// Returns the total path time
	public float FindPath (
		List<PathNode> path,
		Vector3 start, Vector3 destination,
		MobilityType mobility, float radius,
		MoveCommandType command)
	{
		path.Clear ();
		path.Add (new PathNode(destination));

		// Initialize with all nodes accessible from the starting point
		// (this can be optimized later by throwing out some from the start)
		openSet.Clear ();
		foreach (PathNode neighbor in graph) {
			neighbor.isClosed = false;
			neighbor.cameFrom = null;
			neighbor.gScore = Pathfinder.Forever;

			float gScoreNew = Pathfinder.FindLocalPath (this, start, neighbor.position, mobility, radius);
			if (gScoreNew < Pathfinder.Forever) {
				neighbor.gScore = gScoreNew;
				float fScoreNew = gScoreNew + TimeHeuristic (neighbor.position, destination, mobility);
				openSet.Enqueue (neighbor, fScoreNew);
			}
		}

		PathNode cameFromDest = null;
		float gScoreDest = Pathfinder.FindLocalPath (this, start, destination, mobility, radius);

		while (openSet.Count > 0) {

			if (command == MoveCommandType.Slow && gScoreDest < Pathfinder.Forever)
				break;

			PathNode current = openSet.Dequeue ();
			current.isClosed = true;

			if (gScoreDest < current.Priority)
				break;

			foreach (PathArc arc in current.arcs) {
				PathNode neighbor = arc.node1 == current ? arc.node2 : arc.node1;

				if (neighbor.isClosed)
					continue;

				float arcTime = arc.time[(int)mobility];
				if (arcTime >= Pathfinder.Forever)
					continue;

				float gScoreNew = current.gScore + arcTime;
				if (gScoreNew >= neighbor.gScore)
					continue;

				float fScoreNew = gScoreNew + TimeHeuristic (neighbor.position, destination, mobility);

				if (! openSet.Contains (neighbor)) {
					openSet.Enqueue (neighbor, fScoreNew);
				} else {
					openSet.UpdatePriority (neighbor, fScoreNew);
					neighbor.gScore = gScoreNew;
					neighbor.cameFrom = current;
				}
			}

			float arcTimeDest = Pathfinder.FindLocalPath (this, current.position, destination, mobility, radius);
			if (arcTimeDest >= Pathfinder.Forever)
				continue;

			float gScoreDestNew = current.gScore + arcTimeDest;
			if (gScoreDestNew < gScoreDest) {
				gScoreDest = gScoreDestNew;
				cameFromDest = current;
			}

		}
		
		// Reconstruct best path
		PathNode node = cameFromDest;
		while (node != null) {
			path.Add (node);
			node = node.cameFrom;
		}
		return gScoreDest;
	}

	private float TimeHeuristic (Vector3 pos1, Vector3 pos2, MobilityType mobility)
	{
		return Vector3.Distance (pos1, pos2);
	}

}

public class PathNode : FastPriorityQueueNode
{
	public Vector3 position;
	public List<PathArc> arcs;

	public float gScore;
	public bool isClosed;
	public PathNode cameFrom;

	public PathNode (Vector3 position)
	{
		this.position = position;
		arcs = new List<PathArc> (4);
	}
}

public struct PathArc
{
	public PathNode node1, node2;
	public float[] time;

	public PathArc (PathNode node1, PathNode node2)
	{
		time = new float[PathfinderData.NumMobilityTypes];
		this.node1 = node1;
		this.node2 = node2;
	}
}
