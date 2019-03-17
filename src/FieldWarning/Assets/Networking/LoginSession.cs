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

using Nakama;
using TMPro;
using UnityEngine;
using System;

namespace PFW.Networking
{
    public class LoginSession : MonoBehaviour
    {
        private const string PREF_KEY_NAME = "nakama.session";
        private const string SERVER_KEY = "defaultkey";

        private const int DEFAULT_GATEWAY_PORT = 7350;
        private const string FAKE_EMAIL_SUFFIX = "@pfw.com";

        [SerializeField]
        private TMP_InputField _serverInputField;
        [SerializeField]
        private TMP_InputField _portInputField;
        [SerializeField]
        private TMP_InputField _usernameInputField;
        [SerializeField]
        private TMP_InputField _passwordInputField;

        [SerializeField]
        private GameObject _loginModal;
        [SerializeField]
        private GameObject _failedNotification;

        private IClient _client;
        private ISession _session;

        public async void Login()
        {
            int port = DEFAULT_GATEWAY_PORT;

            if (!Int32.TryParse(_portInputField.text, out port)) {
                _failedNotification.SetActive(true);
                return;
            }

            _client = new Client(SERVER_KEY, _serverInputField.text, port, false);

            try {
                _session = await _client.AuthenticateEmailAsync(
                        _usernameInputField.text + FAKE_EMAIL_SUFFIX,
                        _passwordInputField.text);

            } catch {
                _failedNotification.SetActive(true);
                return;
            }

            _loginModal.SetActive(false);
            Debug.LogFormat("Authenticated session: {0}", _session);
        }
    }
}