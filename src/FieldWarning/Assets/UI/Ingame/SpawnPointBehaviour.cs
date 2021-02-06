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

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using PFW.Model.Match;
using PFW.Units;

namespace PFW.UI.Ingame
{
    public class SpawnPointBehaviour : MonoBehaviour
    {
        public Team Team;

        public byte Id = 0b11111111;

        private Queue<GhostPlatoonBehaviour> _spawnQueue = new Queue<GhostPlatoonBehaviour>();
        private float _spawnTime = Constants.SPAWNPOINT_QUEUE_DELAY;

        private void Start()
        {
            GetComponentInChildren<Renderer>().material.color =
                    Team.ColorScheme.BaseColor;
        }

        private void Update()
        {
            if (!_spawnQueue.Any())
                return;

            _spawnTime -= Time.deltaTime / _spawnQueue.Peek().UnitCount;
            if (_spawnTime > 0)
                return;


            GhostPlatoonBehaviour previewPlatoon = _spawnQueue.Dequeue();
            previewPlatoon.Spawn(transform.position);

            if (_spawnQueue.Count > 0)
                _spawnTime += Constants.SPAWNPOINT_MIN_SPAWN_INTERVAL;
            else
                _spawnTime = Constants.SPAWNPOINT_QUEUE_DELAY;
        }

        public void BuyPlatoon(GhostPlatoonBehaviour previewPlatoon)
        {
            _spawnQueue.Enqueue(previewPlatoon);
        }
    }
}
