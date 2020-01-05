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

namespace PFW.Units.Component.Movement
{
    /// <summary>
    /// Movement strategies are passive classes that are used
    /// by the movement component to apply a certain type of movement.
    /// </summary>
    public interface IMovementStrategy
    {
        // TODO documentation
        void PlanMovement();

        /// <summary>
        /// Align the object with the ground, e.g. tilting up along slopes.
        /// </summary>
        /// <param name="forward"></param>
        /// <param name="right"></param>
        void UpdateMapOrientation(Vector3 forward, Vector3 right);

        // TODO documentation
        bool IsMoving();

        /// <summary>
        /// If this returns true, a new movement order may be enqueued 
        /// on the movementComponent.
        /// </summary>
        /// <returns></returns>
        bool AreOrdersComplete();
    }
}