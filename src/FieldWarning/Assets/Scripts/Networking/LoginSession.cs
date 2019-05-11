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
using Nakama.TinyJson;
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
        private TMP_InputField _serverInputField = null;
        [SerializeField]
        private TMP_InputField _portInputField = null;
        [SerializeField]
        private TMP_InputField _usernameInputField = null;
        [SerializeField]
        private TMP_InputField _passwordInputField = null;

        [SerializeField]
        private GameObject _loginModal = null;
        [SerializeField]
        private GameObject _failedNotification = null;

        private string _username;
        private IClient _client;
        private ISession _session;

        // Chat fields:
        const string CHAT_ROOM = "warchat";
        const bool CHAT_ROOM_PERSISTENT = true;
        const bool CHAT_ROOM_HIDDEN = false;

        [SerializeField]
        private TMP_Text _chatMessagesArea = null;
        [SerializeField]
        private TMP_InputField _chatInputField = null;
        private ISocket _chatSocket;
        private IChannel _chatChannel;

        public async void Login()
        {
            int port = DEFAULT_GATEWAY_PORT;

            if (!Int32.TryParse(_portInputField.text, out port)) {
                _failedNotification.SetActive(true);
                return;
            }

            _client = new Client(SERVER_KEY, _serverInputField.text, port, false);

            // TODO there is actually a server-managed server field that currently holds a random string, we should update that:
            _username = _usernameInputField.text;
            try {
                _session = await _client.AuthenticateEmailAsync(
                        _username + FAKE_EMAIL_SUFFIX,
                        _passwordInputField.text);

            } catch {
                _failedNotification.SetActive(true);
                return;
            }

            _loginModal.SetActive(false);

            // TODO move everything below this line to a chat-management module?:
            _chatSocket =  _client.CreateWebSocket();
            await _chatSocket.ConnectAsync(_session);
            _chatChannel = await _chatSocket.JoinChatAsync(
                    CHAT_ROOM, ChannelType.Room, CHAT_ROOM_PERSISTENT, CHAT_ROOM_HIDDEN);

            _chatMessagesArea.text += $"Welcome {_username}, you have connected with session {_session} to {_chatChannel.Id}.\n";

            // Handle any incoming messages:
            _chatSocket.OnChannelMessage += (_, message) => {

                if (_chatChannel.Id == message.ChannelId) {
                    // TODO This is naive, we can't strongly type network input so directly..
                    _chatMessagesArea.text += message.Content.FromJson<ChatMessage>().Flatten();
                }
            };
        }

        public async void SendChatMessage()
        {
            // TMP triggers OnEndEdit callbacks when the input field was deselected; ignore those:
            if (!Input.GetKey(KeyCode.Return) && !Input.GetKey(KeyCode.KeypadEnter))
                return;

            ChatMessage message = new ChatMessage(_username, _chatInputField.text);
            _chatInputField.text = "";
            string jsonPacket = message.ToJson();

            try {
                await _chatSocket.WriteChatMessageAsync(_chatChannel.Id, jsonPacket);
            } catch {
                _chatMessagesArea.text += "Failed to send message..\n";
            }
        }

        public struct ChatMessage
        {
            public string Author;
            public string Content;

            public ChatMessage(string author, string content)
            {
                Author = author;
                Content = content;
            }

            public string Flatten()
            {
                return Author + ": " + Content + "\n";
            }
        }
    }
}