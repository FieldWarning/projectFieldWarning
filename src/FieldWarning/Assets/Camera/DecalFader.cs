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


using PFW.Model.Match;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;

namespace PFW
{
    /// <summary>
    /// When seen from afar, the map terrain is painted by decals using screenshots
    /// from mapping websites. These are not very high-res, so as we zoom in,
    /// they have to be faded out.
    /// </summary>
    public class DecalFader : MonoBehaviour
    {
        [SerializeField]
        private int _minFadeAltitude = 100;
        [SerializeField]
        private int _maxFadeAltitude = 2000;
        [SerializeField]
        private float _minFade = 0f;
        [SerializeField]
        private float _maxFade = 0.65f;
        [SerializeField]
        private float _interpolationExponent = 1f;

        private GameObject _mainCamera;
        private DecalProjector _target;

        private void Start()
        {
            _mainCamera = Camera.main.gameObject;
            _target = GetComponent<DecalProjector>();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene aScene, LoadSceneMode aMode)
        {
            if (Camera.main != null)
            {
                _mainCamera = Camera.main.gameObject;
                this.enabled = true;
            }
            else 
            {
                this.enabled = false;
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Update()
        {
            // TODO this assumes that the map is at y = 0; robust enough?
            float altitude = _mainCamera.transform.position.y;
            altitude = Mathf.Clamp(
                    altitude / Constants.MAP_SCALE,
                    _minFadeAltitude,
                    _maxFadeAltitude);

            _target.fadeFactor = Util.InterpolateBetweenTwoRanges(
                    altitude,
                    _minFadeAltitude,
                    _maxFadeAltitude,
                    _minFade,
                    _maxFade,
                    _interpolationExponent);
        }
    }
}
