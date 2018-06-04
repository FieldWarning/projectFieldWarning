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

 // TODO: What is this? Is it even used?

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RayCast : MonoBehaviour
{

    // Use this for initialization
    Camera camera;
    List<GameObject> selected;
    float clickTime;
    float clickExtent = 0.2f;
    void Start()
    {
        camera = GetComponent<Camera>();
        selected = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetMouseButton(0))
        {
            //check shift etc
            selected.Clear();
            RaycastHit hit;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                var go = hit.transform.gameObject;
                if (go.GetComponent<SelectableBehavior>())
                {
                    selected.Add(go);
                }
            }
        }
        
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                foreach (var go in selected)
                {
                    var behaviour = go.GetComponent<SelectableBehavior>();
                    behaviour.setDestination(hit.point);
                }
            }
            clickTime = Time.time;
        }

        if (Input.GetMouseButtonUp(1) && clickTime + clickExtent < Time.time)
        {
            RaycastHit hit;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                foreach (var go in selected)
                {
                    if (go == null) continue;
                    var behaviour = go.GetComponent<SelectableBehavior>();
                    behaviour.setFinalHeading(hit.point);
                    
                }
            }
        }*/
    }
}
