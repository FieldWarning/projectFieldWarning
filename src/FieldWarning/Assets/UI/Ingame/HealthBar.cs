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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Warning: This script is incomplete and/or unused,
///          so it may make sense to remove it.
/// </summary>
public class HealthBar : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private GameObject _mainBar = null;

    [SerializeField]
    private GameObject _ghostBar = null;

    [Header("Configuration")]
    [SerializeField]
    private float _animationSpeed = 0.5f;

    private RectTransform _mainBarRect;
    private RectTransform _ghostBarRect;
    private bool _isAnimating = false;
    private float _lerpProgress;
    private float _lerpFrom;
    private float _lerpTo;
    private float _fullWidth;


    private void Start()
    {
        _mainBarRect = _mainBar.GetComponent<RectTransform>();
        _ghostBarRect = _ghostBar.GetComponent<RectTransform>();
        _fullWidth = GetComponent<RectTransform>().rect.width;

        _mainBarRect.sizeDelta = new Vector2(_fullWidth, 0);
        _ghostBarRect.sizeDelta = new Vector2(_fullWidth, 0);
    }

    private void Update()
    {
        if (_isAnimating)
            UpdateGhostBarWidth();
    }

    public void SetHealth(float percent)
    {
        SetMainBarWidth(percent);

        _isAnimating = true;
        _lerpFrom = _ghostBarRect.sizeDelta.x;
        _lerpTo = GetWidthForHealth(percent);
        _lerpProgress = 0f;
    }

    private void UpdateGhostBarWidth()
    {
        _lerpProgress += Mathf.Clamp(Time.deltaTime * _animationSpeed, 0f, 1f);
        float targetWidth = Mathf.Lerp(_lerpFrom, _lerpTo, _lerpProgress);

        _ghostBarRect.sizeDelta = new Vector2(targetWidth, 0);

        if (_lerpProgress >= 1f)
            _isAnimating = false;
    }

    private void SetMainBarWidth(float percent)
    {
        float targetWidth = GetWidthForHealth(percent);

        _mainBarRect.sizeDelta = new Vector2(targetWidth, 0);
    }

    private float GetWidthForHealth(float percent)
    {
        return _fullWidth * percent;
    }
}
