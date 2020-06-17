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

namespace PFW.Units.Component.Weapon
{
    /// <summary>
    /// Represents a unit's target, which may either be another unit
    /// or a stationary set of ground coordinates.
    /// </summary>
    public class TargetTuple
    {
        private Vector3 _position { get; set; }
        public UnitDispatcher Enemy { get; private set; }

        public readonly TargetType Type;

        /// <summary>
        /// The position (location) of the target,
        /// regardless of whether its a unit or not.
        /// </summary>
        public Vector3 Position {
            get {
                if (IsGround)
                    return _position;
                else
                    return Enemy.Transform.position;
            }
        }

        public TargetTuple(Vector3 position)
        {
            _position = position;
            Enemy = null;
            Type = TargetType.GROUND;
        }

        /// <summary>
        /// Do not call this constructor outside of the UnitDispatcher class!
        /// </summary>
        /// <param name="enemy"></param>
        public TargetTuple(UnitDispatcher enemy, TargetType type)
        {
            _position = Vector3.zero;
            Enemy = enemy;
            Type = type;
        }

        /// <summary>
        /// Is the target just a position on the ground,
        /// as opposed to an enemy unit?
        /// </summary>
        public bool IsGround {
            get {
                return Type == TargetType.GROUND;
            }
        }

        /// <summary>
        /// Is the target an enemy unit,
        /// as opposed to a position on the ground?
        /// </summary>
        public bool IsUnit {
            get {
                return !IsGround;
            }
        }

        /// <summary>
        /// Is this a non-zero target?
        ///
        /// Warning: Do not intentionally create zero targets,
        /// set and check for null instead.
        /// </summary>
        public bool Exists {
            get {
                return Enemy != null || _position != Vector3.zero;
            }
        }

        /// <summary>
        /// Allows UnitDispatcher to remove references to itself when destroyed.
        /// </summary>
        public void Reset()
        {
            Enemy = null;
            _position = Vector3.zero;
        }
    }

    public enum TargetType
    {
        GROUND = 0,
        INFANTRY,
        VEHICLE,
        HELO,
        _SIZE
    }
}
