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
using PFW.Model.Match;
using PFW.Networking;
using UnityEngine;

namespace PFW.UI.Ingame
{
    /// <summary>
    /// Players can press F1 to place a flare on the map,
    /// which is a graphic indicator visible to their own team only.
    /// </summary>
    public sealed class Flare : NetworkBehaviour
    {
        [SerializeField]
        private TMPro.TextMeshProUGUI _text = null;
        [SerializeField]
        private GameObject _green = null;
        [SerializeField]
        private GameObject _red = null;

        public string Text { get { return _text.text; } }

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            bool canSend = false;
            if (initialState)
            {
                writer.WriteBoolean(_red.activeSelf);
                writer.WriteString(Text);
                canSend = true;
            }
            else
            {
                // Not supported
            }

            return canSend;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (initialState)
            {
                bool isRed = reader.ReadBoolean();
                string text = reader.ReadString();

                Initialize(text, isRed);
            }
            else
            {
                // Not supported
            }
        }

        public static Flare Create(string text, Vector3 position, Team team)
        {
            GameObject go = Instantiate(Resources.Load<GameObject>("Flare"));
            Flare flare = go.GetComponent<Flare>();
            go.transform.position = position;

            flare.Initialize(text, team.Name == Team.TeamName.USSR);

            return flare;
        }

        private void Initialize(string text, bool isRed)
        {
            _text.text = text;

            if (isRed)
            {
                _red.SetActive(true);
            }
            else
            {
                _green.SetActive(true);
            }

            GetComponent<NetworkTeamVisibility>().Initialize(
                    isRed ? Team.TeamName.USSR : Team.TeamName.NATO);
        }

        /// <summary>
        /// Called when the flare label is right clicked.
        /// Tells the server to destroy the flare.
        /// </summary>
        public void Destroy()
        {
            CommandConnection.Connection.CmdDestroyFlare(netId);
        }
    }
}
