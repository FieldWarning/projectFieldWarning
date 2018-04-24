using UnityEngine;
using System.Collections.Generic;
using Priority_Queue;

public class Pathfinder
{
	public const float Forever = float.MaxValue;
	public static Vector3 NoPosition = new Vector3 (float.NaN, float.NaN, float.NaN);
	private static Vector3 Up = new Vector3 (0f, 1f, 0f);

	private const float StepSize = 1.2f; // Any object that the pathfinder is able to navigate around must have at least this radius
	private const float CompletionDist = 2*StepSize; // Good enough if we can get within this distance of the target destination
	private const float AngSearchInc = 12f; // Angluar search increment for local path finding
	private const float MaxAngle = 85f; // Maximum turn a unit can make to either side to get around an obstacle

	private static FastPriorityQueue<PathNode> fScore;

	private UnitBehaviour unit;
	private PathfinderData data;
	private List<PathNode> path;  // path[0] is the final destination
	private PathNode previousNode;
	private MoveCommandType command;
	private Vector3 waypoint;
	private int timeToUpdate;

	public Pathfinder (UnitBehaviour unit, PathfinderData data)
	{
		this.unit = unit;
		this.data = data;
		path = new List<PathNode> ();
	}

	// Generate and store the sequence of nodes leading to the destination using the global graph
	// Returns the total normalized path time
	// If no path was found, return 'forever' and set the path directly to the destination
	public float SetPath (Vector3 destination, MoveCommandType command)
	{
		this.command = command;
		previousNode = null;

		//float pathTime = FindLocalPath (data, unit.transform.position, destination, unit.data.mobility, unit.data.radius);
		//path.Add (new PathNode (destination));
		float pathTime = data.FindPath (path, unit.transform.position, destination, unit.data.mobility, 0f, command);
		return pathTime;
	}

	// Gives the next step along the previously computed path
	// For speed, this will only update the waypoint on some frames
	// Returns 'NoPosition' if there is no destination or a step cannot be found
	public Vector3 GetWaypoint()
	{
		if (!HasDestination ()) { // Nowhere to go
			waypoint = NoPosition;
			return waypoint;
		}

		timeToUpdate--;
		if (timeToUpdate <= 0) {
			PathNode targetNode = path[path.Count - 1];

			float distance = Vector3.Distance (unit.transform.position, targetNode.position);
			if (distance < CompletionDist) { // Unit arrived at the next path node
				path.RemoveAt (path.Count - 1);
				if (!HasDestination ()) { // Unit arrived at the destination
					waypoint = NoPosition;
					return waypoint;
				} else {
					previousNode = null;
					targetNode = path[path.Count - 1];
				}
			}

			Vector3 newWaypoint = TakeStep (
				data, unit.transform.position, targetNode.position, unit.data.mobility, unit.data.radius);

			if (newWaypoint != null) {
				waypoint = newWaypoint;
			} else {
				
				// The unit has gotten stuck when following the previously computed path.
				// Now recompute a new path to the destination using the global graph, this time using finite radius
				float pathTime = data.FindPath (path, unit.transform.position, path[0].position, unit.data.mobility, unit.data.radius, command);
				//float pathTime = SetPath (path[0].position, command);

				if (pathTime == Forever) {  // The unit has somehow gotten itself trapped
					Debug.Log ("I am stuck!!!");
					waypoint = NoPosition;
				} else {
					// If this is an intermediate step of the path, then the pre-computed global graph might
					// be broken and the corresponding arc should be recomputed to avoid having units cycle forever
					if (previousNode != null && path.Count > 1) {
						data.RemoveArc (previousNode, targetNode);
						data.AddArc (previousNode, targetNode);
					}
				}
			}

			timeToUpdate = 4; // (int)(StepSize / unit.data.movementSpeed);
		}

		return waypoint;
	}

	public bool HasDestination()
	{
		return path.Count > 0;
	}

	// Build a path in an approximately straight line from start to destination by stringing together steps
	// This is NOT guaranteed to not get stuck in a local terrain feature
	// Returns the total normalized path time, or 'forever' if stuck
	public static float FindLocalPath (
		PathfinderData data,
		Vector3 start, Vector3 destination,
		MobilityType mobility,
		float radius)
	{
		float distance = (destination - start).magnitude;
		Vector3 waypoint = start;
		float time = 0f;

		while (distance > CompletionDist) {
			waypoint = TakeStep (data, waypoint, destination, mobility, radius);
			if (waypoint == NoPosition)
				return Forever;
			time += StepSize / data.GetUnitSpeed (mobility, waypoint, radius);
			distance = (destination - waypoint).magnitude;
		}

		return time;
	}

	// Finds an intermediate step along the way from start to destination
	// Returns 'NoPosition' if stuck
	private static Vector3 TakeStep (
		PathfinderData data,
		Vector3 start, Vector3 destination,
		MobilityType mobility,
		float radius)
	{
		Vector3 straight = (destination - start).normalized * StepSize;

		// Fan out in a two-point horizontal pattern to find a way forward
		for (float ang1 = 0f; ang1 <= MaxAngle; ang1 += AngSearchInc) {

			for (int direction = -1; direction <= 1; direction += 2) {

				Vector3 midpoint = start + (ang1 > 0f ? Quaternion.AngleAxis (ang1*direction, Up) * straight : straight);
				float midspeed = data.GetUnitSpeed (mobility, midpoint, radius);

				if (midspeed > 0f) {
					for (float ang2 = 0f; ang2 <= ang1; ang2 += AngSearchInc) {

						Vector3 endpoint = midpoint + (ang2 > 0f ? Quaternion.AngleAxis (ang2*direction, Up) * straight : straight);
						float endspeed = data.GetUnitSpeed (mobility, endpoint, radius);

						if (endspeed > 0f) 
							return ang1 > 0f ? midpoint : endpoint;
					}
				}

			}

		}

		// No step was found
		return NoPosition;
	}
	
}

