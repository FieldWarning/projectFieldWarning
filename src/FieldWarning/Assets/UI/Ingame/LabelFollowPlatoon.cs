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

/// <summary>
/// Ensures a platoon label implemented as screenlayout UI
/// will follow the platoon as it moves.
/// </summary>
public sealed class LabelFollowPlatoon : MonoBehaviour
{
    private Camera _mainCamera;
    private RectTransform _labelRect;
    [SerializeField]
    private Transform _transformToFollow = null;
    private float _vertOffset;
    private const float VERT_OFFSET_FACTOR = 0.1f;

    // Start is called before the first frame update
    private void Start()
    {
        _mainCamera = Camera.main;
        _labelRect = GetComponent<RectTransform>();
        _vertOffset = _mainCamera.scaledPixelHeight * VERT_OFFSET_FACTOR;
    }

    // Update is called once per frame
    private void Update()
    {
        _labelRect.position = _mainCamera.WorldToScreenPoint(
                _transformToFollow.position);
        _labelRect.position = new Vector3(
                _labelRect.position.x,
                _labelRect.position.y + _vertOffset,
                _labelRect.position.z);
    }
}
