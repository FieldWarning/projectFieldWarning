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

namespace AssemblyCSharp
{
	public class Bullet
	{
		private Vector3 _startPosition;
		private Vector3 _endPosition;
		int _vellocity;

        //will be used later so shells arent lasers
        int _arc;
		
		public Bullet (Vector3 start_position, Vector3 end_position, int vellocity, int arc, Boolean isHit)
		{
			_startPosition = start_position; 
			_endPosition = end_position;
			_vellocity = vellocity;
			_arc = arc;

			//If the shell is a miss end position will be recalculated into a
			//random yet close position to the enemy unit

			if (!isHit) {
				end_position.x = UnityEngine.Random.Range (-20.0f, 20.0f);
				end_position.z = UnityEngine.Random.Range (-20.0f, 20.0f);
			}
		}	
	}
}

