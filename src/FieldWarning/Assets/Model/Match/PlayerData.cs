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

using PFW.Model.Armory;

namespace PFW.Model.Match
{
    /**
     * Most players are passive data containers.
     *
     * For the logic behind managed players, take a look at the PlayerBehaviour class.
     */
    public class PlayerData
    {
        // Unique and must match the index the player is held in by MatchSession.Players
        public byte Id;
        public Team Team;
        public string Name;
        public Deck Deck;

        public float Money = 1000;
        public float IncomeTick = 7;

        public PlayerData(Deck deck, Team team, string name, byte id)
        {
            Deck = deck;
            Team = team;
            Name = name;
            Id = id;
        }
    }
}
