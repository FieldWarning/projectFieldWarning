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

using PFW.Model.Game;

namespace PFW.UI.Ingame
{
    /**
     * Manages chat within a match, synched server to clients, receives
     * client messages from the PlayerChat class.
     */
    public class ChatManager : Mirror.NetworkBehaviour
    {
        [SerializeField]
        private GameObject _chat = null;
        [SerializeField]
        private TMPro.TMP_InputField _inputField = null;
        [SerializeField]
        private TMPro.TextMeshProUGUI _messagesText = null;

        [Tooltip("How long the Chat is visible when receiving a new Message")]
        [SerializeField]
        private float MESSAGES_VISIBLE_MAX = 7;
        private float _messagesVisibleRemaining = 0;
        [SerializeField]
        private MatchSession _session = null;
        private string _oldMessages = "";

        // Contains all sent messages, synched across all clients.
        [Mirror.SyncVar]
        public string _sentMessages = "";

        // Start is called before the first frame update
        private void Start()
        {
            _chat.SetActive(false);
        }

        public void UpdateMessageText(string msg)
        {
            _sentMessages = msg;
            _messagesText.text = msg;
            _messagesText.gameObject.SetActive(true);
            _messagesVisibleRemaining = MESSAGES_VISIBLE_MAX;
        }

        // Update is called once per frame
        private void Update()
        {
            if (_messagesVisibleRemaining <= 0) {
                _messagesText.gameObject.SetActive(false);
                _messagesVisibleRemaining = MESSAGES_VISIBLE_MAX;
            } else {
                _messagesVisibleRemaining -= Time.deltaTime;
            }

            //If Messages where updated via Sync, then this needs to be detected
            if (!_oldMessages.Equals(_sentMessages)) {
                _oldMessages = _sentMessages;
                _messagesText.text = _sentMessages;
                _messagesText.gameObject.SetActive(true);
                _messagesVisibleRemaining = MESSAGES_VISIBLE_MAX;
            }

            //Open/Close Chat
            if (Input.GetButtonDown("Chat")) {
                //if Chat is being closed handle the typed message
                if (_chat.activeSelf == true) {
                    //replace with the playername once we get accounts working
                    string user = "[" + "Name here"+ "]:";
                    _sentMessages += user + _inputField.text + "\n";
                    _messagesText.text = _sentMessages;
                    _messagesText.gameObject.SetActive(true);
                    _messagesVisibleRemaining = MESSAGES_VISIBLE_MAX;
                    _session.isChatFocused = false;
                } else {
                    //activated chat
                    _session.isChatFocused = true;
                    _messagesText.gameObject.SetActive(true);
                    _messagesVisibleRemaining = MESSAGES_VISIBLE_MAX;
                }

                _chat.SetActive(!_chat.activeSelf);
                _inputField.Select();
                _inputField.ActivateInputField();
                _inputField.text = "[YOU]";
            }
        }
    }
}
