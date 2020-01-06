

using System;
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PFW.Units.Component.Movement
{
    /// <summary>
    /// Intended to manage a single movement component.
    /// (as opposed to multiple, e.g. picking between an amphib and a ground mover).
    /// </summary>
    public class SimpleNavigationComponent : INavigationComponent
    {
        // TODO
        private IMoveComponent _movementComponent;

        public void EnqueueRouteDirect(Vector3 destination)
        {
            throw new NotImplementedException();
        }

        public void EnqueueRouteFast(Vector3 destination)
        {
            throw new NotImplementedException();
        }

        public void EnqueueRouteReverse(Vector3 destination)
        {
            throw new NotImplementedException();
        }
    }
}
