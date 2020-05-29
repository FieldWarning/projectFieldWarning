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


using System.Collections.Generic;
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
        private const float SMOKE_DURATION = 40f;
        private const float SMOKE_DELAY = 2f;
        private const float WRECK_DURATION = 60f;

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
            Destroy(_art, WRECK_DURATION);

            _smokePrefab.SetActive(true);
            _smokePrefab.transform.parent = null;
            Destroy(_smokePrefab, SMOKE_DURATION);
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime > SMOKE_DELAY)
            {
                _smokePrefab.SetActive(true);
                _smokePrefab.transform.parent = null;
                Destroy(_smokePrefab, SMOKE_DURATION - SMOKE_DELAY);
                Destroy(gameObject);
            }
        }
    }
}
