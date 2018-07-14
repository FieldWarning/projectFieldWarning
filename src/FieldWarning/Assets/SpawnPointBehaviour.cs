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

public class SpawnPointBehaviour : MonoBehaviour {
    Camera camera;
    Vector3 oldPosition;
    [SerializeField] private float height = 0.5f;
    float size = 0.1f;
    private Queue<PlatoonBehaviour> spawnQueue = new Queue<PlatoonBehaviour>();
    public static float spawnDelay = 2f;
    public static float queueDelay = 1f;
    float spawnTime = spawnDelay;
    public Team team;
	// Use this for initialization
	void Start () {
        camera = Camera.main;
        UIManagerBehaviour.addSpawnPoint(this);
        if (team==Team.Blue) {
            GetComponent<Renderer>().material.color = Color.blue;
        } else {
            GetComponent<Renderer>().material.color = Color.red;
        }
	}
	
	// Update is called once per frame
	void Update () {
        transform.rotation = Quaternion.LookRotation(camera.transform.forward);
        var distance = (camera.transform.position - transform.position).magnitude;
        transform.localScale = size * distance * Vector3.one;

        if (spawnQueue.Count > 0) {
            spawnTime -= Time.deltaTime;
            if (spawnTime < 0) {
                var go=spawnQueue.Dequeue();
                go.GetComponent<PlatoonBehaviour>().spawn(transform.position);

                if (spawnQueue.Count > 0) {
                    spawnTime = spawnDelay;
                } else {
                    spawnTime = queueDelay;
                }
            }
        }
	}

    public void updateQueue(List<PlatoonBehaviour> list)
    {
        list.ForEach(x => spawnQueue.Enqueue(x));
    }
    
}
