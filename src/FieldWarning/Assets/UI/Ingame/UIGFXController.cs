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
using UnityEngine.UI;

namespace PFW.UI.Ingame
{
    [ExecuteAlways]
    public abstract class UIGFXController : MonoBehaviour
    {
        [Header("Auto-Size")]
        [SerializeField]
        private bool _autoSizeEnabled = false;
    #pragma warning disable 0649
        [SerializeField]
        private GameObject _autoSizeTarget;
    #pragma warning restore 0649

        [Header("Colors")]
        [SerializeField]
        protected UIColor _baseColorID;

        [SerializeField]
        protected UIColor _accentColorID;
        protected Color _baseColor;
        protected Color _accentColor;

        private RectTransform _targetRect;
        private RectTransform _rect;

        protected class Transition
        {
            public Image Component;
            public Color ColorFrom;
            public Color ColorTo;

            public Transition(Image component, Color colorFrom, Color colorTo)
            {
                Component = component;
                ColorFrom = colorFrom;
                ColorTo = colorTo;
            }

            public void Animate(float lerp)
            {
                Color newColor = Color.Lerp(ColorFrom, ColorTo, lerp);
                Component.color = newColor;
            }
        }

        protected List<Transition> _transitionList = new List<Transition>();

        protected virtual void Start()
        {
            if (_autoSizeEnabled && _autoSizeTarget != null) {
                _rect = GetComponent<RectTransform>();
                _targetRect = _autoSizeTarget.GetComponent<RectTransform>();
            }

            _baseColor = UIColors.GetColor(_baseColorID);
            _accentColor = UIColors.GetColor(_accentColorID);
        }

        protected virtual void Update()
        {
            if (_rect != null && _targetRect != null) {
                Vector2 targetSizeDelta = _targetRect.sizeDelta;

                if (targetSizeDelta != _rect.sizeDelta)
                    _rect.sizeDelta = targetSizeDelta;
            }
        }
    }
}
