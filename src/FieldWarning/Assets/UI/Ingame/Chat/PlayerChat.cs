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
namespace PFW.UI.Ingame
{

    public class PlayerChat : Mirror.NetworkBehaviour
    {
        public ChatManager ChatManager;
        private string _messages = "";

        // Start is called before the first frame update
        void Start()
        {
            ChatManager = GameObject.FindGameObjectWithTag("ChatManager").GetComponent<ChatManager>();

        }
        //(On Server) Update chat messages
        [Mirror.Command]
        void CmdupdateMsg(string msg)
        {
            _messages = msg;
            ChatManager.UpdateMessageText(msg);
            ChatManager.MessagesText.text = msg;
        }
        // Update is called once per frame
        void Update()
        {
            // When you wrote a message, send it to the server, which then distributes it to other clients
            if (isLocalPlayer) {
                if (!ChatManager._messages.Equals(_messages)) {
                   // Debug.Log("Sending" + ChatManager._messages);
                    CmdupdateMsg(ChatManager._messages);
                    _messages = ChatManager._messages;
                }

            }

        }

    }
}
