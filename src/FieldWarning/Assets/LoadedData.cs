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

using PFW.Units.Component.Movement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadedData : MonoBehaviour
{
    public TerrainMap terrainData;
    public PathfinderData pathFinderData;

    //private Loading _loader;

    // Start is called before the first frame update
    void Start()
    {
        // TODO: Pass terrain from future location of starting matches (no Find)
        DontDestroyOnLoad(this.gameObject);

        Terrain[] terrains = GameObject.FindObjectsOfType<Terrain>();
        terrainData = new TerrainMap(terrains);

        pathFinderData = new PathfinderData(terrainData);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
