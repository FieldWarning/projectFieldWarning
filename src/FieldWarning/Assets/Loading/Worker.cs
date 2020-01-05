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

using System.Collections;
using System.Threading;
using UnityEngine;

namespace Loading
{

    public delegate IEnumerator WorkerCoroutineDelegate();
    public delegate void WorkerThreadDelegate();
    public delegate void WorkerConcurrentDelegate();


    public abstract class Worker
    {
        public double PercentDone = 0;
        public bool Finished = false;
        public bool Started = false;
        public string description;

        public Worker(string desc)
        {

            description = desc;
        }

        protected void SetProgressToStarted()
        {
            Started = true;
            PercentDone = 0;
        }

        protected void SetProgressToFinished()
        {
            PercentDone = 100;
            Finished = true;
        }

        public abstract void Run();
    }

    public class CoroutineWorker : Worker
    {
        private WorkerCoroutineDelegate function;

        public CoroutineWorker(WorkerCoroutineDelegate func, string desc) : base(desc)
        {
            this.function = func;
        }

        public override void Run()
        {
            var runner = GameObject.FindObjectOfType<CoroutineRunner>();
            runner.StartCoroutine(coroutine());

        }

        public IEnumerator coroutine()
        {
            SetProgressToStarted();
            yield return function();
            SetProgressToFinished();
        }
    }

    public class MultithreadedWorker : Worker
    {
        private WorkerThreadDelegate function;

        public MultithreadedWorker(WorkerThreadDelegate func, string desc) : base(desc)
        {
            this.function = func;
        }

        public override void Run()
        {
            Thread _thread = new Thread(thread);
            _thread.Start();
        }

        private void thread(object obj)
        {
            SetProgressToStarted();
            function();
            SetProgressToFinished();
        }

    }
}







