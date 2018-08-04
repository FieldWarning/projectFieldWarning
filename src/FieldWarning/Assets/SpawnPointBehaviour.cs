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

using Assets.Ingame.UI;

public class SpawnPointBehaviour : MonoBehaviour
{
    Vector3 oldPosition;
    private Queue<PlatoonBehaviour> spawnQueue = new Queue<PlatoonBehaviour>();
    public const float MIN_SPAWN_INTERVAL = 2f;
    public const float QUEUE_DELAY = 1f;
    float spawnTime = MIN_SPAWN_INTERVAL;
    public Team team;

    // Use this for initialization
    void Start()
    {
        UIManagerBehaviour.addSpawnPoint(this);
        if (team == Team.Blue) {
            GetComponentInChildren<Renderer>().material.color = Color.blue;
        } else {
            GetComponentInChildren<Renderer>().material.color = Color.red;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (spawnQueue.Count > 0) {
            spawnTime -= Time.deltaTime;
            if (spawnTime <= 0) {
                var go = spawnQueue.Dequeue();
                go.GetComponent<PlatoonBehaviour>().Spawn(transform.position);

                if (spawnQueue.Count > 0) {
                    spawnTime += MIN_SPAWN_INTERVAL;
                } else {
                    spawnTime = QUEUE_DELAY;
                }
            }
        }
    }

    public void buyUnits(List<GhostPlatoonBehaviour> ghostUnits)
    {
        var realPlatoons = ghostUnits.ConvertAll(x => x.GetComponent<GhostPlatoonBehaviour>().GetRealPlatoon());

        realPlatoons.ForEach(x => spawnQueue.Enqueue(x));
    }



}
