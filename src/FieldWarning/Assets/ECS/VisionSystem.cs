using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace ECS
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
            Debug.Log("yo");

            Job job = new Job();
            return job.Schedule(this, inputDeps);
        }

        private struct Job : IJobProcessComponentData<Vision>
        {
            public void Execute(ref Vision vid)
            {
                Debug.Log("hi");
                return;
            }
        }
    }
}
