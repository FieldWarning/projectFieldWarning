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

public class UnitLabelAttacher : MonoBehaviour
{
    public GameObject Label;
    private Canvas _canvas;
    private Collider _collider;

    // Use this for initialization
    void Start()
    {
        _canvas = GameObject.Find("UIWrapper").GetComponent<Canvas>();
        _collider = GetComponentInChildren<Collider>();

        Label = Instantiate(Resources.Load<GameObject>("UnitLabel"), _canvas.transform);
    }

    // Update is called once per frame
    void Update()
    {
        Label.transform.position = GetScreenPosition(_canvas);
    }

    public Vector3 GetScreenPosition(Canvas canvas, Camera cam = null)
    {
        if (cam == null)
            cam = Camera.main;

        var labelPos = cam.WorldToScreenPoint(transform.position);

        labelPos.y += 40f;

        return labelPos;
    }
}
