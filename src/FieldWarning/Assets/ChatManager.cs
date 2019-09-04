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
using UnityEngine.SceneManagement;
namespace Mirror
{

    public class ChatManager : NetworkBehaviour
    {
        public GameObject Chat;
        public TMPro.TMP_InputField InputField;
        public TMPro.TextMeshProUGUI MessagesText;
        [Tooltip("How long the Chat is visible when recieving a new Message")]
        public float MessagesVisibleFor = 7;
        private float _messagesVisible = 7;
        //Sync recieved messages to other clients
        [SyncVar]
        public string _messages = "";
        private string _oldMessages = "";
        // Start is called before the first frame update
        void Start()
        {
            Chat.active = false;

        }
        public void UpdateMessageText(string msg)
        {
            _messages = msg;
            MessagesText.text = msg;
            MessagesText.gameObject.active = true;
            _messagesVisible = MessagesVisibleFor;
        }

        // Update is called once per frame
        void Update()
        {
            if (_messagesVisible < 0) {
                MessagesText.gameObject.active = false;
                _messagesVisible = 7;
            } else {
                _messagesVisible -= Time.deltaTime;
            }
            //If Messages where updated via Sync, then this needs to be detected
            if (!_oldMessages.Equals(_messages)) {
                _oldMessages = _messages;
                MessagesText.text = _messages;
                MessagesText.gameObject.active = true;
                _messagesVisible = MessagesVisibleFor;
            }
            //Open/Close Chat
            if (Input.GetButtonDown("Chat")) {
                //if Chat is being closed handle the typed message
                if (Chat.active == true) {
                    //replace with the playername once we get accounts working
                    string user = "[" + Mirror.ClientScene.localPlayer.name + "]:";
                    _messages += user + InputField.text + "\n";
                    MessagesText.text = _messages;
                    MessagesText.gameObject.active = true;
                    _messagesVisible = MessagesVisibleFor;
                    PFW.Model.Game.GameManager.EnableInput = true;
                } else {
                    //activated chat
                    PFW.Model.Game.GameManager.EnableInput = false;
                    MessagesText.gameObject.active = true;
                    _messagesVisible = MessagesVisibleFor;
                }
                Chat.active = !Chat.active;
                InputField.Select();
                InputField.ActivateInputField();
                InputField.text = "[YOU]";


            }
        }

    }
}
