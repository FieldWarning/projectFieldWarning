﻿/**
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

namespace PFW.Model.Game
{
    /**
     * Most players are passive data containers.
     *
     * For the logic behind managed players, take a look at the PlayerBehaviour class.
     */
    public class PlayerData
    {
        public Team Team;

        private MatchSession _session;
        public MatchSession Session {
            get {
                return _session;
            }

            set {
                if (_session == null)
                    _session = value;
            }
        }

        public float Money = 1000;
        public float IncomeTick = 7;
    }
}