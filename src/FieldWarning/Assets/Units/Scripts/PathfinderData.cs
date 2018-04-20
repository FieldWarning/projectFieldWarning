using UnityEngine;

public class PathfinderData
{
	public TerrainData terrain;

	public PathfinderData (TerrainData terrain)
	{
		this.terrain = terrain;
		//InitializeObstructions (terrain);
		InitializeGraph (terrain);
	}

	private void InitializeGraph(TerrainData terrain)
	{

	}

	// Give the relative speed of a unit with the given MobilityType on the given terrain location
	public float GetUnitSpeed(MobilityType mobility, Vector3 location)
	{
		return 1f;
	}

}
