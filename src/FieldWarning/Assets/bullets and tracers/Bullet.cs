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

