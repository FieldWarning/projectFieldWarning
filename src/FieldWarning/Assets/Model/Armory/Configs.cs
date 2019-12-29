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

using System;
using System.Collections.Generic;

namespace PFW.Model.Armory
{
    [Serializable]
    public class DeckConfig
    {
        public List<string> LOG;
        public List<string> SUP;
        public List<string> INF;
        public List<string> TNK;
        public List<string> REC;
        public List<string> VHC;
        public List<string> HEL;

        public object this[string fieldName] {
            get {
                return typeof(DeckConfig)
                        .GetField(fieldName)
                        .GetValue(this);
            }
        }
    }

    [Serializable]
    public class UnitConfig
    {
        public string ID;
        public string CategoryKey;
        public string Name;
        public int Price;
        public string PrefabPath;
        public UnitDataConfig Data;
        public MobilityConfig Mobility;
        public List<UnitWeaponConfig> Weapons;
    }

    [Serializable]
    public class UnitDataConfig
    {
        public float MovementSpeed;
        public float ReverseSpeed;
        public float AccelRate;
        public float MaxRotationSpeed;
        public float MinTurnRadius;
        public float MaxLateralAccel;
        public float Suspension;
        public float MaxHealth;
        public float Length;
        public float Width;
        public int MobilityTypeIndex;
    }

    [Serializable]
    public class MobilityConfig
    {
        public float SlopeSensitivity;
        public float DirectionalSlopeSensitivity;
        public float PlainSpeed;
        public float ForestSpeed;
        public float WaterSpeed;
    }

    [Serializable]
    public class UnitWeaponConfig
    {
        public string ComponentRef;
        public string TypeRef;
        public int Priority;
        public string SoundRef;
        public string EffectRef;
        public int Volume;
    }
}