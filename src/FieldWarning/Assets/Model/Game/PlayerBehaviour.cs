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

namespace PFW.Model.Game
{
    /**
     * Represents "managed" players.
     *
     * This full-fledged player object is for players for who we
     * manage lines of sight, income, and other data as opposed
     * to just getting them over the network.
     */
    public class PlayerBehaviour : MonoBehaviour
    {
        public PlayerData Data;

        /**
         * Returns the money rounded to a multiple of the income tick.
         */
        public float Money {
            get {
                return Mathf.Floor(Data.Money / Data.IncomeTick) * Data.IncomeTick;
            }
        }

        public void Update()
        {
            Data.Money += Data.IncomeTick * Time.deltaTime;
        }
    }
}
