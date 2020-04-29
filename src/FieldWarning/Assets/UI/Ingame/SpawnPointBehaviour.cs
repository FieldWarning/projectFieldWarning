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

using PFW.Model.Game;
using PFW.Units;

public class SpawnPointBehaviour : MonoBehaviour
{
    public Team Team;

    public const float MIN_SPAWN_INTERVAL = 2f;
    public const float QUEUE_DELAY = 1f;

    private Queue<PlatoonBehaviour> _spawnQueue = new Queue<PlatoonBehaviour>();
    private float _spawnTime = MIN_SPAWN_INTERVAL;

    private void Start()
    {
        GetComponentInChildren<Renderer>().material.color = 
                Team.ColorScheme.BaseColor;

        MatchSession.Current.RegisterSpawnPoint(this);
    }

    private void Update()
    {
        if (!_spawnQueue.Any())
            return;

        _spawnTime -= Time.deltaTime;
        if (_spawnTime > 0)
            return;


        PlatoonBehaviour previewPlatoon = _spawnQueue.Dequeue();
        previewPlatoon.Spawn(transform.position);

        if (_spawnQueue.Count > 0)
            _spawnTime += MIN_SPAWN_INTERVAL;
        else
            _spawnTime = QUEUE_DELAY;
    }

    public void BuyPlatoons(List<PlatoonBehaviour> previewPlatoons)
    {
        // TODO show the preview to allies over the network
        previewPlatoons.ForEach(x => _spawnQueue.Enqueue(x));
    }
}
