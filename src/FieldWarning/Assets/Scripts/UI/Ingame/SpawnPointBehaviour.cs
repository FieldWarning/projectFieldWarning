﻿/**
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

public class SpawnPointBehaviour : MonoBehaviour
{
    public PFW.Model.Game.Team Team;

    public const float MIN_SPAWN_INTERVAL = 2f;
    public const float QUEUE_DELAY = 1f;

    private Vector3 oldPosition;
    private Queue<GhostPlatoonBehaviour> spawnQueue = new Queue<GhostPlatoonBehaviour>();
    private float spawnTime = MIN_SPAWN_INTERVAL;

    public void Awake()
    {
        Team = GetComponentInParent<PFW.Model.Game.Team>();
    }

    public void Start()
    {
        GetComponentInChildren<Renderer>().material.color = Team.Color;

        Team.Session.AddSpawnPoint(this);
    }

    public void Update()
    {
        if (!spawnQueue.Any())
            return;
        
        spawnTime -= Time.deltaTime;
        if (spawnTime > 0)
            return;

            
        GhostPlatoonBehaviour ghostPlatoon = spawnQueue.Dequeue();
        ghostPlatoon.Spawn(transform.position);

        if (spawnQueue.Count > 0)
            spawnTime += MIN_SPAWN_INTERVAL;
        else
            spawnTime = QUEUE_DELAY;
        
    }

    public void BuyPlatoons(List<GhostPlatoonBehaviour> ghostPlatoons)
    {
        ghostPlatoons.ForEach(x => spawnQueue.Enqueue(x));
    }
}
