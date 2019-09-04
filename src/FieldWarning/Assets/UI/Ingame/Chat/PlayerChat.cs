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

using Mirror;

namespace PFW.UI.Ingame
{
    /**
     * This class exists to allow clients to send their chat messages to the server.
     */
    public class PlayerChat : NetworkBehaviour
    {
        public ChatManager ChatManager;
        private string _messages = "";

        private void Start()
        {
            ChatManager =
                GameObject.FindGameObjectWithTag("ChatManager").GetComponent<ChatManager>();
        }

        // (On Server) Update chat messages
        [Command]
        public void CmdUpdateMsg(string msg)
        {
            _messages = msg;
            ChatManager.UpdateMessageText(msg);
        }

        private void Update()
        {
            // When you wrote a message, send it to the server,
            // which then distributes it to other clients
            if (isLocalPlayer) {
                if (!ChatManager._sentMessages.Equals(_messages)) {
                    CmdUpdateMsg(ChatManager._sentMessages);
                    _messages = ChatManager._sentMessages;
                }
            }
        }
    }
}
