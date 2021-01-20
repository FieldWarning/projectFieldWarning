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

using System;

using UnityEngine;

using Mirror;
using TMPro;

using PFW.Model.Match;
using PFW.Networking;

namespace PFW.UI.Ingame
{
    /**
     * Manages chat within a match, synched server to clients, receives
     * client messages from the PlayerChat class.
     */
    public class ChatManager : NetworkBehaviour
    {
        private const int MAX_MESSAGES = 20;

        public bool IsChatOpen { get; private set; } = false;

        [SerializeField]
        private GameObject _chat = null;
        [SerializeField]
        private TMP_InputField _inputField = null;
        [SerializeField]
        private TextMeshProUGUI _messagesText = null;

        [Tooltip("How long the Chat is visible when receiving a new Message")]
        [SerializeField]
        private float MESSAGES_VISIBLE_MAX = 7;
        private float _messagesVisibleRemaining = 0;
        [SerializeField]
        private MatchSession _session = null;

        // Contains all sent messages, synched across all clients.
        private SyncList<string> _sentMessages = new SyncList<string>();

        private CommandConnection _connection { get { return CommandConnection.Connection; } }

        private void Start()
        {
            // TODO race if we enter the scene before connection complete (e.g. in editor), fix
            if (isClientOnly)
                _connection.ChatManager = this;
            _chat.SetActive(false);
            _sentMessages.Callback += OnChatUpdate;
        }

        // Called when we receive a new message from the server (or ourselves)
        private void OnChatUpdate(
                SyncList<string>.Operation op, int index, string oldItem, string newItem)
        {
            // Update the UI element
            _messagesText.text += newItem;
            if (_sentMessages.Count > MAX_MESSAGES) /* TODO perhaps also shrink the list */
                _messagesText.text = _messagesText.text.Substring(
                        _messagesText.text.IndexOf("\n") + "\n".Length);

            _messagesText.gameObject.SetActive(true);
            _messagesVisibleRemaining = MESSAGES_VISIBLE_MAX;
        }

        [Server]
        public void UpdateMessageText(
                PlayerData sender, string msg)
        {
            const string CHEAT_PREFIX = "/";
            const string DECK_CHEAT = "/deck";
            const string BETRAY_CHEAT = "/betray";

            if (msg.StartsWith(CHEAT_PREFIX))
            {
                // cheat codes
                if (msg.StartsWith(DECK_CHEAT))
                {
                    string deck = msg.Substring(DECK_CHEAT.Length);
                    deck = deck.Trim();
                    MatchSession.Current.RegisterDeckChange(sender, deck);
                }
                else if (msg.StartsWith(BETRAY_CHEAT))
                {
                    //MatchSession.Current.RegisterPlayerChange(_team.Players[0]);
                }
            }
            else
            {
                _sentMessages.Add($"[{sender.Name}]: {msg}\n");
            }
        }

        /// <summary>
        /// Called by the input manager when the user presses the
        /// open/close chat hotkey.
        /// </summary>
        public void OnToggleChat()
        {
            // If chat is being closed handle the typed message
            if (_chat.activeSelf == true)
            {
                if (!string.IsNullOrWhiteSpace(_inputField.text))
                {
                    _connection.CmdUpdateChat(
                            MatchSession.Current.LocalPlayer.Data.Id, _inputField.text);
                }
                IsChatOpen = false;
            }
            else
            {
                // activated chat
                IsChatOpen = true;
                _messagesText.gameObject.SetActive(true);
                _messagesVisibleRemaining = MESSAGES_VISIBLE_MAX;
            }

            _chat.SetActive(!_chat.activeSelf);
            _inputField.Select();
            _inputField.ActivateInputField();
            _inputField.text = string.Empty;
        }

        private void Update()
        {
            if (_messagesVisibleRemaining <= 0) 
            {
                _messagesText.gameObject.SetActive(false);
                _messagesVisibleRemaining = MESSAGES_VISIBLE_MAX;
            } 
            else if (!_session.IsChatFocused) 
            {
                _messagesVisibleRemaining -= Time.deltaTime;
            }
        }
    }
}
