using UnityEngine;
using System;
using System.Collections.Generic;

public class PathfinderData
{
	public static int NumMobilityTypes;
	private const float GraphRadius = 3f; // This should be replaced with the largest unit radius of the given MobilityType
	private const float SparseGridSpacing = 200f;

	public TerrainData terrain;
	public List<PathNode> graph;

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


		// Fill in any big open spaces with a sparse grid
		Vector3 size = TerrainData.size;
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

		// Compute arcs for all pairs of nodes
		for (int i = 0; i < graph.Count; i++) {
			for (int j = i + 1; j < graph.Count; j++) {
				AddArc (graph[i], graph[j]);
			}
		}

		// Remove unnecessary arcs

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
		for (int i = 0; i < node1.arcs.Count; i++) {
			PathArc arc = node1.arcs[i];
			if (arc.node1.Equals(node2) || arc.node2.Equals(node2)) {
				node1.arcs.Remove (arc);
				node2.arcs.Remove (arc);
			}
		}
	}

	// Gives the relative speed of a unit with the given MobilityType on the given terrain location
	public float GetUnitSpeed (MobilityType mobility, Vector3 location)
	{
		return 1f;
	}

}

public struct PathNode
{
	public Vector3 position;
	public List<PathArc> arcs;

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
