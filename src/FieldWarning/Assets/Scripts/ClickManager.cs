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
using System;

public class ClickManager
{
    private int button;

    private float dragThreshold;
    private Vector3 lastMousePosition;
    private float mouseDistanceTravelled = 0;

    private Action onMouseDown;
    private Action nonDragMouseRelease;
    private Action dragMouseRelease;
    private Action whileDraggingMouse;

    public ClickManager(int button, float dragThreshold, Action onMouseDown, Action nonDragMouseRelease, Action dragMouseRelease, Action whileDraggingMouse)
    {
        if (onMouseDown == null || nonDragMouseRelease == null || dragMouseRelease == null || whileDraggingMouse == null)
            throw new Exception("Tried to create a ClickManager with a missing callback!");

        this.button = button;
        this.dragThreshold = dragThreshold;
        this.onMouseDown = onMouseDown;
        this.nonDragMouseRelease = nonDragMouseRelease;
        this.dragMouseRelease = dragMouseRelease;
        this.whileDraggingMouse = whileDraggingMouse;
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(button)) {
            mouseDistanceTravelled = 0;
            lastMousePosition = Input.mousePosition;
            onMouseDown();
        }

        if (Input.GetMouseButton(button) && !isDragClick()) {
            mouseDistanceTravelled += Vector3.Distance(Input.mousePosition, lastMousePosition);
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(button) && isDragClick())
            whileDraggingMouse();


        if (Input.GetMouseButtonUp(button))
            if (isDragClick())
                dragMouseRelease();
            else
                nonDragMouseRelease();
    }

    private bool isDragClick()
    {
        return mouseDistanceTravelled > dragThreshold;
    }
}
