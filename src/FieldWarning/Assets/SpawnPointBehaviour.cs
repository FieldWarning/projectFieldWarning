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
        if (oldPosition != transform.position) {
            var y = GameObject.Find("Terrain").GetComponent<Terrain>().GetComponent<TerrainCollider>().terrainData.GetInterpolatedHeight(transform.position.x, transform.position.z);
            transform.position += (y + height) * Vector3.up;
            oldPosition = transform.position;
        }

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
