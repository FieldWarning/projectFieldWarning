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

public class HealthBarBehaviour : SelectableBehavior
{
    UnitBehaviour unit;
    GameObject bar;

    void Start()
    {
        bar = transform.GetChild(0).gameObject;
        bar.AddComponent<SelectableBehavior>();
        SetHealth(unit.data.maxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        SetHealth(unit.GetHealth() / unit.data.maxHealth);
    }

    public void SetUnit(UnitBehaviour o)
    {
        unit = o;
    }

    void SetHealth(float h)
    {
        float health = Mathf.Clamp01(h);

        //bar.transform.localScale = new Vector3(health, 1, 1);
        //var offset = bar.GetComponent<Renderer>().bounds.extents.x ;
        //bar.transform.localPosition = new Vector3(offset-0.5f, 0, -.01f);
        bar.GetComponent<Renderer>().material.color = PickColor(health);

        //Debug.Log("-- hp : " + (1f - health));
        bar.GetComponent<Renderer>().material.SetFloat("_Cutoff", 1f - health);
    }

    private Color PickColor(float h)
    {
        if (h < 0.25f)
            return Color.red;

        if (h < 0.5f)
            return Color.yellow;

        return Color.green;
    }
}
