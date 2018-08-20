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

public class TransporterBehaviour : MonoBehaviour
{
    // Use this for initialization
    public InfantryBehaviour transported;
    public InfantryBehaviour target;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (target != null) {
            if (target.interactsWithTransport(true)) {
                GetComponent<UnitBehaviour>().SetUnitDestination(transform.position);
            }
            //target.setRally(getRallyPoint(), transform.position);//???????
            else if (GetComponent<UnitBehaviour>().Pathfinder.HasDestination()) {
                GetComponent<UnitBehaviour>().SetUnitDestination(target.transform.position);
            }
        }
    }

    public void unload()
    {
        var pos = transform.position;
        var rallyPoint = getRallyPoint();
        transported.unload(pos, rallyPoint);
        //Debug.Log("unload");
    }

    public Vector3 getRallyPoint()
    {
        var rallyDistance = 2;
        var rallyPoint = transform.position - rallyDistance * transform.forward;
        return rallyPoint;
    }

    public void load(InfantryBehaviour t)
    {
        target = t;
        //GetComponent<UnitBehaviour>().gotDestination = true;

    }

    public bool loadingComplete()
    {
        if (target.interactsWithTransport(false)) {
            transported = target;
            return true;
        } else {
            return false;
        }
    }

    public bool unloadingComplete()
    {
        if (transported == null || !transported.interactsWithTransport(false)) {
            transported = null;
            return true;
        } else {
            return false;
        }

    }

    public bool interrupt()
    {
        if (transported == null) {
            return true;
        } else if (!target.interactsWithTransport(true)) {
            return true;
        } else {
            return false;
        }
    }
}
