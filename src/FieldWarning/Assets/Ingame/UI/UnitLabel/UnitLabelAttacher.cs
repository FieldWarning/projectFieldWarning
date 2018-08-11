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
    private Vector3 _paddingVector;

    public void Start()
    {
        _canvas = GameObject.Find("UIWrapper").GetComponent<Canvas>();
        _collider = GetComponentInChildren<Collider>();

        Label = Instantiate(Resources.Load<GameObject>("UnitLabel"), _canvas.transform);

        _paddingVector = new Vector3(0, 5, 0);
    }

    public void LateUpdate()
    {
        Label.transform.position = GetScreenPosition(_canvas, Camera.main);
    }

    public void Hide()
    {
        Label.SetActive(false);
    }

    public void Show()
    {
        Label.SetActive(true);
    }

    public void SetVisibility(bool visible)
    {
        if (visible)
            Show();
        else
            Hide();
    }

    public Vector3 GetScreenPosition(Canvas canvas, Camera cam = null)
    {
        if (cam == null)
            cam = Camera.main;

        var labelPos = cam.WorldToScreenPoint(transform.position + _paddingVector);

        return labelPos;
    }
}
