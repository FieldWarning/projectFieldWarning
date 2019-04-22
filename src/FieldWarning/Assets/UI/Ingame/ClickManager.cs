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

using static PFW.Constants;

/*
 * The purpose of this class is to distinguish between short clicks
 * and dragging movements with the mouse held down.
 *
 * The owner of a ClickManager initializes it with some callbacks and
 * repeatedly calls Update(). In each call, ClickManager checks the state
 * of the mouse and calls any relevant callbacks. ClickManager is
 * like a "trampoline" - when it is called its only job is to invoke
 * a handler, usually in the class that called it.
 */
public class ClickManager
{
    private readonly int _button;

    private Vector3 _lastMousePosition;
    private float _mouseDistanceTravelled = 0;

    private readonly Action _onMouseDown;
    private readonly Action _nonDragMouseRelease;
    private readonly Action _dragMouseRelease;
    private readonly Action _whileDraggingMouse;

    // Prevents the firing of any events
    // unless the inital mouse down event was detected.
    private bool _primed = false;

    public ClickManager(int button, Action onMouseDown, Action nonDragMouseRelease, Action dragMouseRelease, Action whileDraggingMouse)
    {
        if (onMouseDown == null || nonDragMouseRelease == null || dragMouseRelease == null || whileDraggingMouse == null)
            throw new Exception("Tried to create a ClickManager with a missing callback!");

        _button = button;
        _onMouseDown = onMouseDown;
        _nonDragMouseRelease = nonDragMouseRelease;
        _dragMouseRelease = dragMouseRelease;
        _whileDraggingMouse = whileDraggingMouse;
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(_button)) {
            _primed = true;
            _mouseDistanceTravelled = 0;
            _lastMousePosition = Input.mousePosition;
            _onMouseDown();
        }

        if (_primed && Input.GetMouseButton(_button) && !isDragClick()) {
            _mouseDistanceTravelled += Vector3.Distance(Input.mousePosition, _lastMousePosition);
            _lastMousePosition = Input.mousePosition;
        }

        if (_primed && Input.GetMouseButton(_button) && isDragClick())
            _whileDraggingMouse();

        if (_primed && Input.GetMouseButtonUp(_button)) {
            _primed = false;
            if (isDragClick())
                _dragMouseRelease();
            else
                _nonDragMouseRelease();
        }
    }

    private bool isDragClick()
    {
        return _mouseDistanceTravelled > MOUSE_DRAG_THRESHOLD;
    }
}
