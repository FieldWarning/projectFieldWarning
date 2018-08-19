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


using System.Collections.Generic;
using UnityEngine;

namespace PFW.Model.Game
{
    public class Team : MonoBehaviour
    {
        public string Name;
        public Color Color;

        public List<Player> Players { get; private set; }

        public void Awake()
        {
            Players = new List<Player>();
        }

        public bool IsEnemy(Team t)
        {
            return Color != t.Color;
        }
    }
}
