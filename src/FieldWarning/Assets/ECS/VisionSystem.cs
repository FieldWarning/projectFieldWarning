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
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace PFW.ECS
{
    public struct Vision : IComponentData
    {
        public float max_spot_range;
    }

    // TODO write as JobComponentSystem:
    public class VisionSystem : JobComponentSystem
    {
        [ReadOnly]
        public ComponentDataArray<Vision> Visibles;

        // The system decides here whether a job shall be executed or not. Note that if scheduled, the job's execute method will not be called once but rather as many times as there are components.
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //Debug.Log("yo");

            Job job = new Job();
            return job.Schedule(this, inputDeps);
        }

        private struct Job : IJobProcessComponentData<Vision>
        {
            public void Execute(ref Vision vid)
            {
                //Debug.Log("hi");
                return;
            }
        }
    }
}
