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

using PFW.Model.Settings.JsonContents;

namespace PFW.Model.Settings
{
    /// <summary>
    /// The settings menu has a number of fields that
    /// reflect entries in our DefaultSettings.json.
    /// If any value is changed and the change is applied,
    /// the new values are stored in LocalSettings.json
    /// and used as an override.
    /// </summary>
    public class SettingsMenu : MonoBehaviour
    {
        [SerializeField]
        private TMPro.TMP_InputField _zoomSpeed = null;
        [SerializeField]
        private TMPro.TMP_InputField _turnSpeed = null;
        [SerializeField]
        private TMPro.TMP_InputField _panSpeed = null;
        [SerializeField]
        private TMPro.TMP_InputField _attackMove = null;
        [SerializeField]
        private TMPro.TMP_InputField _fastMove = null;
        [SerializeField]
        private TMPro.TMP_InputField _reverseMove = null;
        [SerializeField]
        private TMPro.TMP_InputField _firePosition = null;
        [SerializeField]
        private TMPro.TMP_InputField _split = null;
        [SerializeField]
        private TMPro.TMP_InputField _visionTool = null;
        [SerializeField]
        private TMPro.TMP_InputField _load = null;
        [SerializeField]
        private TMPro.TMP_InputField _unload = null;
        [SerializeField]
        private TMPro.TMP_InputField _menuToggle = null;
        [SerializeField]
        private TMPro.TMP_InputField _stop = null;
        [SerializeField]
        private TMPro.TMP_InputField _weaponsOff = null;
        [SerializeField]
        private TMPro.TMP_InputField _smoke = null;
        [SerializeField]
        private TMPro.TMP_InputField _unitInfo = null;
        [SerializeField]
        private TMPro.TMP_InputField _flareAttack = null;
        [SerializeField]
        private TMPro.TMP_InputField _flareStop = null;
        [SerializeField]
        private TMPro.TMP_InputField _flareCustom = null;

        private void Start()
        {
            _zoomSpeed.scrollSensitivity = 0;
            _turnSpeed.scrollSensitivity = 0;
            _panSpeed.scrollSensitivity = 0;
            _attackMove.scrollSensitivity = 0;
            _fastMove.scrollSensitivity = 0;
            _reverseMove.scrollSensitivity = 0;
            _firePosition.scrollSensitivity = 0;
            _split.scrollSensitivity = 0;
            _visionTool.scrollSensitivity = 0;
            _load.scrollSensitivity = 0;
            _unload.scrollSensitivity = 0;
            _menuToggle.scrollSensitivity = 0;
            _stop.scrollSensitivity = 0;
            _weaponsOff.scrollSensitivity = 0;
            _smoke.scrollSensitivity = 0;
            _unitInfo.scrollSensitivity = 0;
            _flareAttack.scrollSensitivity = 0;
            _flareStop.scrollSensitivity = 0;
            _flareCustom.scrollSensitivity = 0;
        }

        public void OnApply()
        {
            ConfigReader.WriteLocalConfig(GetFields());
            GameSession.Singleton.ReloadSettings();
        }

        /// <summary>
        /// Resets the menu fields to the default values.
        /// After pressing reset, the user also needs to press
        /// apply to save the changes.
        /// </summary>
        public void OnReset()
        {
            SetFields(ConfigReader.ParseDefaultSettingsRaw());
        }

        public void OnExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnEnable()
        {
            SetFields(GameSession.Singleton.SettingsRaw);
        }

        private void SetFields(SettingsConfig config)
        {
            _zoomSpeed.text = config.Camera.ZoomSpeed.ToString();
            _turnSpeed.text = config.Camera.RotationSpeed.ToString();
            _panSpeed.text = config.Camera.PanSpeed.ToString();
            _attackMove.text = config.Hotkeys.AttackMove;
            _fastMove.text = config.Hotkeys.FastMove;
            _reverseMove.text = config.Hotkeys.ReverseMove;
            _firePosition.text = config.Hotkeys.FirePosition;
            _split.text = config.Hotkeys.Split;
            _visionTool.text = config.Hotkeys.VisionTool;
            _load.text = config.Hotkeys.Load;
            _unload.text = config.Hotkeys.Unload;
            _menuToggle.text = config.Hotkeys.MenuToggle;
            _stop.text = config.Hotkeys.Stop;
            _weaponsOff.text = config.Hotkeys.WeaponsOff;
            _smoke.text = config.Hotkeys.Smoke;
            _unitInfo.text = config.Hotkeys.UnitInfo;
            _flareAttack.text = config.Hotkeys.FlareAttack;
            _flareStop.text = config.Hotkeys.FlareStop;
            _flareCustom.text = config.Hotkeys.FlareCustom;
        }

        private SettingsConfig GetFields()
        {
            return new SettingsConfig
            {
                Camera = new CameraConfig
                {
                    ZoomSpeed = int.Parse(_zoomSpeed.text),
                    RotationSpeed = int.Parse(_turnSpeed.text),
                    PanSpeed = int.Parse(_panSpeed.text)
                },
                Hotkeys = new HotkeyConfig
                {
                    AttackMove = _attackMove.text.ToUpper(),
                    FastMove = _fastMove.text.ToUpper(),
                    ReverseMove = _reverseMove.text.ToUpper(),
                    FirePosition = _firePosition.text.ToUpper(),
                    Split = _split.text.ToUpper(),
                    VisionTool = _visionTool.text.ToUpper(),
                    Load = _load.text.ToUpper(),
                    Unload = _unload.text.ToUpper(),
                    MenuToggle = _menuToggle.text,
                    Stop = _stop.text.ToUpper(),
                    WeaponsOff = _weaponsOff.text.ToUpper(),
                    Smoke = _smoke.text.ToUpper(),
                    UnitInfo = _unitInfo.text.ToUpper(),
                    FlareAttack = _flareAttack.text.ToUpper(),
                    FlareStop = _flareStop.text.ToUpper(),
                    FlareCustom = _flareCustom.text.ToUpper()
                }
            };
        }
    }
}
