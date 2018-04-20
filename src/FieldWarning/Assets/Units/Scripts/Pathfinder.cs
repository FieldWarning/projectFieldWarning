using UnityEngine;
using System.Collections.Generic;

public class Pathfinder
{
	public const float forever = float.MaxValue;
	public static Vector3 invalid = new Vector3 (float.NaN, float.NaN, float.NaN);
	private static Vector3 up = new Vector3 (0f, 1f, 0f);
	private const float stepSize = 5f; // Any object that the pathfinder is able to navigate around must have at least this radius
	private const float completionDist = 3*stepSize; // Good enough if we can get within this distance of the target destination

	private UnitBehaviour unit;
	private PathfinderData data;
	private List<PathNode> path;
	private MoveCommandType command;
	private Vector3 waypoint;
	private int timeToUpdate;

	public Pathfinder (UnitBehaviour unit, PathfinderData data)
	{
		this.unit = unit;
		this.data = data;
		path = new List<PathNode> ();
	}

	// Generate and store the sequence of points leading to the destination using the global graph
	// Returns the total normalized path time
	// If no path was found, return 'forever' and set the path directly to the destination
	public float FindPath (Vector3 destination, MoveCommandType command)
	{
		this.command = command;
		path.Clear ();

		float pathTime = FindLocalPath (data, unit.transform.position, destination, unit.data.mobility, unit.data.radius);
		if (pathTime < forever)
			path.Add (new PathNode (destination));
		return pathTime;
	}

	// Gives the next step along the previously computed path
	// For speed, this will only update the waypoint on some frames
	// Returns 'invalid' if there is no destination or a step cannot be found
	public Vector3 GetWaypoint()
	{
		if (!HasDestination ()) { // Nowhere to go
			waypoint = invalid;
			return waypoint;
		}

		timeToUpdate--;
		if (timeToUpdate <= 0) {

			float distance = Vector3.Distance (unit.transform.position, path[path.Count - 1].position);
			if (distance < completionDist) { // We have arrived at the next path node
				path.RemoveAt (path.Count - 1);
				if (!HasDestination ()) { // We have arrived at the destination
					waypoint = invalid;
					return waypoint;
				}
			}

			Vector3 newWaypoint = TakeStep (
				data, unit.transform.position, path[path.Count - 1].position, unit.data.mobility, unit.data.radius);

			if (newWaypoint != null) {
				waypoint = newWaypoint;
			} else {
				
				// The unit has gotten stuck when following the previously computed path.
				// Now recompute a new path using the global graph
				float pathTime = FindPath (path[0].position, command);
				if (pathTime == forever) {  // The unit has somehow gotten itself trapped
					Debug.Log ("I am stuck!!!");
					waypoint = invalid;
				} else {

					// TODO: If this is an intermediate step of the path, then the pre-computed global graph might
					//       be broken and the corresponding arc should be recomputed to avoid having units cycle forever
				}
			}

			timeToUpdate = 2 * (int)(stepSize / unit.data.movementSpeed);
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

		while (distance > completionDist) {
			waypoint = TakeStep (data, waypoint, destination, mobility, radius);
			if (waypoint == invalid)
				return forever;
			time += stepSize / data.GetUnitSpeed (mobility, waypoint);
			distance = (destination - waypoint).magnitude;
		}

		return time;
	}

	// Finds an intermediate step along the way from start to destination
	// Returns 'invalid' if stuck
	private static Vector3 TakeStep (
		PathfinderData data,
		Vector3 start, Vector3 destination,
		MobilityType mobility,
		float radius)
	{
		const float angSearchInc = 20f; // Angluar search increment for local path finding
		const float maxAngle = 85f; // Maximum turn a unit can make to either side to get around an obstacle

		Vector3 straight = (destination - start).normalized * stepSize;

		// Fan out in a two-point horizontal pattern to find a way forward
		for (float ang1 = 0f; ang1 <= maxAngle; ang1 += angSearchInc) {

			for (int direction = -1; direction <= 1; direction += 2) {

				Vector3 midpoint = start + Quaternion.AngleAxis (ang1*direction, up) * straight;
				float midspeed = data.GetUnitSpeed (mobility, midpoint);

				if (midspeed > 0f) {
					for (float ang2 = 0f; ang2 <= ang1; ang2 += angSearchInc) {

						Vector3 endpoint = midpoint + Quaternion.AngleAxis (ang2*direction, up) * straight;
						float endspeed = data.GetUnitSpeed (mobility, endpoint);

						if (endspeed > 0f) 
							return midpoint;
					}
				}

			}

		}

		// No step was found
		return invalid;
	}
	
	private struct PathNode {
		public Vector3 position;

		public PathNode (Vector3 position)
		{
			this.position = position;
		}
	}
	
	private struct PathArc {
		
	}
	
}

