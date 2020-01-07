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

namespace PFW.Loading
{
    public delegate IEnumerator WorkerCoroutineDelegate();
    public delegate void WorkerThreadDelegate();
    public delegate void WorkerConcurrentDelegate();

    public abstract class Worker
    {
        public double PercentDone = 0;
        public readonly string Description;

        public Worker(string desc)
        {
            Description = desc;
        }

        protected void SetProgressToStarted()
        {
            PercentDone = 0;
        }

        protected void SetProgressToFinished()
        {
            PercentDone = 100;
        }

        public bool IsFinished()
        {
            return PercentDone == 100;
        }

        public abstract void Start();
    }

    /// <summary>
    /// Coroutines give us 'fake' multithreading(they must run on the main thread).
    /// </summary>
    public class CoroutineWorker : Worker
    {
        private readonly WorkerCoroutineDelegate _function;

        public CoroutineWorker(WorkerCoroutineDelegate func, string desc) : base(desc)
        {
            _function = func;
        }

        public override void Start()
        {
            var runner = GameObject.FindObjectOfType<CoroutineRunner>();
            runner.StartCoroutine(Run());

        }

        public IEnumerator Run()
        {
            SetProgressToStarted();
            yield return _function();
            SetProgressToFinished();
        }
    }

    public class MultithreadedWorker : Worker
    {
        private readonly WorkerThreadDelegate _function;

        public MultithreadedWorker(WorkerThreadDelegate func, string desc) : base(desc)
        {
            _function = func;
        }

        public override void Start()
        {
            Thread _thread = new Thread(Run);
            _thread.Start();
        }

        private void Run(object obj)
        {
            SetProgressToStarted();
            _function();
            SetProgressToFinished();
        }
    }
}