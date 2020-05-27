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

using PFW;
using UnityEngine;
using static PFW.SlidingCameraBehaviour;

/// <summary>
/// A camera moved around a pivot. 
/// 
/// Currently not used, but it may find use in the armory in the future.
/// </summary>
public class OrbitCameraBehaviour : MonoBehaviour
{
    private Camera cam;
    private Vector3 camOffset;
    private GameObject target = null;

    [SerializeField]
    private float _borderPanningOffset = 2; // Pixels
    [SerializeField]
    private float _borderPanningCornerSize = 200; // Pixels

    float zoomFactor = 3.6f;
    float horizontalROtationSpeed = 5f;
    float verticalROtationSpeed = .1f;
    float upperAngleLimit = 20f;
    float lowerAngleLimit = 80f;
    public float maxZoom = 350f;
    public float minZoom = 2;

    [SerializeField]
    private float _rotLerpSpeed = 10f;

    [SerializeField]
    private float _zoomSpeed = 1000000f * Constants.MAP_SCALE;


    public void SetTarget(GameObject t)
    {
        target = t;
        camOffset = transform.position - target.transform.position;
    }


    private void Start()
    {
        cam = GetComponent<Camera>();
        camOffset = transform.position;
        if (target)
        {
            camOffset = transform.position- target.transform.position;
        }
    }

    private void Update()
    {
        float xin = Input.GetAxis("Horizontal");
        float zin = Input.GetAxis("Vertical");


        ScreenCorner corner = SlidingCameraBehaviour.GetScreenCornerForMousePosition(
                _borderPanningOffset, _borderPanningCornerSize);

        // if we translate the camera manually then take us off orbit mode.. This will need
        // to be refactored to make this entire component more flexible.
        if (corner != ScreenCorner.None || xin != 0 || zin != 0)
        {
            gameObject.GetComponentInParent<SlidingCameraBehaviour>().enabled = true;
            enabled = false;
        }

        if (!target)
        {
            return;

        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            // do not allow us to scroll past a certain point
            if ((camOffset.magnitude < maxZoom || scroll > 0) && (camOffset.magnitude > minZoom || scroll < 0))
            {
                Vector3 calcOffset = transform.position;

                // lerp the zoom factor to zoom fast when far away from target and much slower when close
                float zoomFNew = Mathf.Lerp(zoomFactor / 10, zoomFactor * 4, camOffset.magnitude / maxZoom);

                calcOffset += transform.forward * scroll * _zoomSpeed * zoomFNew;
                camOffset = (calcOffset - target.transform.position);
            }
        }

        if (Input.GetMouseButton(2))
        {
            float dy = -Input.GetAxis("Mouse Y");
            float dx = Input.GetAxis("Mouse X");

            if ((Vector3.Angle(camOffset, Vector3.up) > upperAngleLimit || dy < 0)
                && (Vector3.Angle(camOffset, Vector3.up) < lowerAngleLimit || dy > 0))
            {
                camOffset = Vector3.RotateTowards(camOffset, Vector3.up, dy * verticalROtationSpeed, 0f);
            }

            camOffset = Quaternion.AngleAxis(dx * horizontalROtationSpeed, Vector3.up) * camOffset;

        }

        transform.position = Vector3.Lerp(transform.position, target.transform.position + camOffset, Time.deltaTime * _rotLerpSpeed);

        // if we are not rotating.. then we want a smooth look at.. means we are switching units
        // if we dont have this logic here our rotation becomes too smooth and looks unnatural
        if (!Input.GetMouseButton(2))
        {
            //// smooth the lookat/rotation
            Quaternion lookOnLook = Quaternion.LookRotation(
                    target.transform.position - transform.position);
            transform.rotation = Quaternion.Slerp(
                    transform.rotation, lookOnLook, Time.deltaTime * _rotLerpSpeed);
        }
        else
        {
            transform.LookAt(target.transform.position);
        }
    }

    // this is needed to reset our zoom level after we are enabled again. We dont want to suddenly jump
    // back to the zoom before we were disabled.
    void OnEnable()
    {
        camOffset = transform.position;
        if (target)
        {
            camOffset = transform.position - target.transform.position;
        }
    }
}
