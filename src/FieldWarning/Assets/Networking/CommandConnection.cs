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

using Mirror;
using UnityEngine;

using PFW.UI.Ingame;

namespace PFW.Networking
{
    /**
     * This class exists to allow clients to send commands to the server.
     *
     * Weird name to make it easier to distinguish from the mirror classes.
     */
    public class CommandConnection : NetworkBehaviour
    {
        public ChatManager ChatManager;
        public static CommandConnection Connection;

        private void Start()
        {
            ChatManager = FindObjectOfType<ChatManager>();
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            Connection = this;
        }

        // (On Server) Update chat messages
        [Command]
        public void CmdUpdateChat(string msg)
        {
            ChatManager.UpdateMessageText(msg);
        }

        // Spawn an object for all network participants.
        // NetworkServer.Spawn() can only be called by objects with authority, e.g. this one
        [Command]
        public void CmdSpawnObject(GameObject go)
        {
            NetworkServer.Spawn(go);
        }
    }
}
