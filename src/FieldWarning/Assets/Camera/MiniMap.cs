using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
    public Terrain Terrain;
    private Camera _camera;

    public void Start()
    {
        _camera = GetComponent<Camera>();
    }

    public void LateUpdate ()
    {
        var size = Terrain.terrainData.bounds.size;

        _camera.orthographicSize = size.x / 2f;   
	}
}
