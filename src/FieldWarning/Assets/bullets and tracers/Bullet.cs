using System;
using UnityEngine;



//NOT CURRENTLY USED

namespace AssemblyCSharp
{
	public class Bullet
	{
		Vector3 start_position;
		//staring position of the shell
		Vector3 end_position;
		//end positin
		int Vellocity;
		int arc;
		//will be used later so shells arent lasers

	


		public Bullet (Vector3 start_position, Vector3 end_position, int vellocity, int arc, Boolean IsAHit)
		{
			this.start_position = start_position; 
			this.end_position = end_position;
			this.Vellocity = vellocity;
			this.arc = arc;

			//If the shell is a miss end position will be recalculated into a
			//random yet close position to the enemy unit

			if (IsAHit == false) {
				end_position.x = UnityEngine.Random.Range (-20.0f, 20.0f);
				end_position.z = UnityEngine.Random.Range (-20.0f, 20.0f);

			}

		}
		
	}
}

