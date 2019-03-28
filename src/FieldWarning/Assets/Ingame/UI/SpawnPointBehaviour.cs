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

using System.Collections.Generic;
using System.Linq;
using PFW.Model.Game;
using UnityEngine;
public class SpawnPointBehaviour : MonoBehaviour {
    public const float MIN_SPAWN_INTERVAL = 2f;
    public const float QUEUE_DELAY = 1f;
    public Team @Team { get; private set; }
    
    private Queue<GhostPlatoonBehaviour> _spawnQueue { get; } = new Queue<GhostPlatoonBehaviour>();
    private float _spawnTime = MIN_SPAWN_INTERVAL;
    
    public void Awake() {
        this.Team = this.GetComponentInParent<Team>();
    }

    public void Start() {
        this.GetComponentInChildren<Renderer>().material.color = this.Team.Color;
        this.Team.Session.AddSpawnPoint(this);
    }

    public void Update() {
        // If there is no one in the spawn queue then don't continue.
        if (!this._spawnQueue.Any()) return;

        // Check if spawn time minus the current deltaTime is > 0 then exit method;
        if (this._spawnTime -= Time.deltaTime > 0) return;

        // NullReferenceException City - position is not set properly.
        this._spawnQueue.Dequeue().Spawn(this.transform.position);

        // Increase spawn queue time if total amount in queue is greater than zero
        //  otherwise set to QUEUE_DELAY (1f) PS: Turnary > if statements
        this._spawnTime = (this._spawnQueue.Count > 0) ?
            this._spawnTime += MIN_SPAWN_INTERVAL : QUEUE_DELAY;
    }

    public void BuyPlatoons(List<GhostPlatoonBehaviour> ghostPlatoons) {
        ghostPlatoons.ForEach (x => this._spawnQueue.Enqueue(x));
    }
}
