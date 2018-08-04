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

public class MeshBuilder : MonoBehaviour
{

    // Use this for initialization
    float roadWidth = .5f;
    float roadStretch = .5f;
    float pointDensity = 1;
    void Start()
    {
        //buildRoadStretch(new Vector2(),new Vector3(30,0,30),new Vector3(30,0,0),new Vector3(30,0,30));
    }
    public void buildRoadStretch(Vector3 p1, Vector3 p1p, Vector3 p2, Vector3 p2p)
    {
        var stretch = getRoadStretch(p1, p1p, p2, p2p);
        //stretch.ForEach(x => Debug.Log(x));
        var mesh = new Mesh();
        buildRoadMesh(ref mesh, stretch);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        ;
        //var go = GameObject.Instantiate(gameObject);
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<Renderer>().material.mainTexture.wrapMode = TextureWrapMode.Repeat;
    }
    private List<Vector3> getRoadStretch(Vector3 p1, Vector3 p1p, Vector3 p2, Vector3 p2p)
    {


        //f=a*t^3+b*t^2+c*t+d
        var a = 2 * (p1 - p2) + p1p + p2p;
        var b = 3 * (p2 - p1) - 2 * p1p - p2p;
        var c = p1p;
        var d = p1;
        float dt = 1 / ((p1 - p2).magnitude * pointDensity);
        List<Vector3> output = new List<Vector3>();
        for (float t = 0; t <= 1; t += dt) {
            Vector3 v = a * t * t * t + b * t * t + c * t + d;
            output.Add(v);
        }
        return output;


    }
    public void buildRoads(List<List<Vector3>> list)
    {
        foreach (var l in list) {
            var go = Instantiate(gameObject);
            var mesh = new Mesh();
            buildRoadMesh(ref mesh, l);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            go.GetComponent<MeshFilter>().mesh = mesh;
            go.GetComponent<Renderer>().material.mainTexture.wrapMode = TextureWrapMode.Repeat;
            //go.transform.position += 3 * (i++) * Vector3.up;
        }
    }
    void buildRoadMesh(ref Mesh mesh, List<Vector3> points)
    {

        if (points.Count < 2) return;
        int vc = mesh.vertices.Length;
        List<Vector3> verteces = new List<Vector3>(mesh.vertices);
        var r = right(points[0], points[1]);
        verteces.Add(points[0] + r);
        verteces.Add(points[0] - r);
        for (int i = 1; i < points.Count - 1; i++) {
            r = right(points[i - 1], points[i], points[i + 1]);
            verteces.Add(points[i] + r);
            verteces.Add(points[i] - r);
            verteces.Add(points[i] + r);
            verteces.Add(points[i] - r);
        }
        r = right(points[points.Count - 2], points[points.Count - 1]);
        verteces.Add(points[points.Count - 1] + r);
        verteces.Add(points[points.Count - 1] - r);

        List<int> triangles = new List<int>(mesh.triangles);
        for (int i = 0; i < points.Count - 1; i++) {
            triangles.Add(vc + 4 * i);
            triangles.Add(vc + 4 * i + 2);
            triangles.Add(vc + 4 * i + 1);
            triangles.Add(vc + 4 * i + 1);
            triangles.Add(vc + 4 * i + 2);
            triangles.Add(vc + 4 * i + 3);
        }
        List<Vector2> uv = new List<Vector2>(mesh.uv);
        float distanceRight = 0;
        float distanceLeft = 0;
        //Debug.Log(verteces.Count);
        uv.Add(new Vector2(1, 0));
        uv.Add(new Vector2(0, 0));
        for (int i = 0; i < points.Count - 1; i++) {


            distanceRight += roadStretch * (verteces[4 * i] - verteces[4 * i + 2]).magnitude / roadWidth;
            distanceLeft += roadStretch * (verteces[4 * i + 1] - verteces[4 * i + 3]).magnitude / roadWidth;
            uv.Add(new Vector2(1, distanceRight));
            uv.Add(new Vector2(0, distanceLeft));
            if (distanceRight > distanceLeft) {
                distanceRight = 2 * distanceLeft - distanceRight;
            } else {
                distanceLeft = 2 * distanceRight - distanceLeft;
            }
            if (i < points.Count - 2) {
                uv.Add(new Vector2(1, distanceRight));
                uv.Add(new Vector2(0, distanceLeft));
            }

        }
        //Debug.Log(verteces.Count);
        //Debug.Log(uv.Count);
        //var mesh = new Mesh();
        mesh.SetVertices(verteces);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uv);
    }
    Vector3 right(Vector3 v1, Vector3 v2)
    {
        Vector3 first = (v2 - v1).normalized;
        return roadWidth * Vector3.Cross(Vector3.down, first);
    }
    Vector3 right(Vector3 v1, Vector3 v2, Vector3 v3)
    {

        Vector3 first = (v2 - v1).normalized;
        Vector3 second = (v3 - v2).normalized;
        Vector3 right = Vector3.Cross(Vector3.down, first);
        var candidate = (second - first).normalized;
        var scale = Vector3.Dot(right, candidate);
        if (scale == 0) {
            return roadWidth * right;
        } else {
            return roadWidth * candidate / scale;
        }
    }



}
