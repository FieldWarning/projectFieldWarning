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
using UnityEngine.VFX;

namespace PFW.Units.Component.Death
{
    /// <summary>
    /// When a unit is killed, it plays an explosion
    /// animation and stays around for a while
    /// as a charred inactive wreck. This is all done
    /// by activating this component.
    /// </summary>
    public class WreckComponent : MonoBehaviour
    {
        [SerializeField]
        private VisualEffect _deathEffect = null;
        [SerializeField]
        private GameObject _smokePrefab = null;
        [SerializeField]
        private AudioSource _explosionAudio = null;
        [SerializeField]
        private float _smokeDuration = 40f;
        [SerializeField]
        private float _explosionDuration = 1f;
        [SerializeField]
        private float _wreckLifetime = 60f;

        private GameObject _art;
        private float elapsedTime = 0f;

        /// <summary>
        /// Enables the wreck animations and deparents
        /// the unit art so that it stays around as a wreck
        /// even after the main unit GO is destroyed.
        /// </summary>
        public void Activate(GameObject art)
        {
            _art = art;
            _art.transform.parent = null;
            _art.BlackenRecursively();
            gameObject.SetActive(true);
            _deathEffect.Play();
            _explosionAudio.Play();
            Destroy(_art, _wreckLifetime);

            _smokePrefab.SetActive(true);
            _smokePrefab.transform.parent = null;
            Destroy(_smokePrefab, _smokeDuration);
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime > _explosionDuration)
            {
                _smokePrefab.SetActive(true);
                _smokePrefab.transform.parent = null;
                float maxDuration = _smokeDuration - _explosionDuration;
                Destroy(_smokePrefab, Random.Range(maxDuration / 2, maxDuration));
                Destroy(gameObject);
            }
        }
    }
}
