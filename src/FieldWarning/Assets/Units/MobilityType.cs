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

// The purpose of having MobilityType as a separate class from UnitData is
//     so that only a few pathfinding graphs are needed, instead of having a separate
//     one for each type of unit.
public class MobilityType
{
    // This list needs to be instantiated before the PathfinderData
    public static readonly List<MobilityType> MobilityTypes = new List<MobilityType>();

    public readonly int Index;

    // More all-terrain units like infantry should have reduced slope sensitivity
    public readonly float SlopeSensitivity;

    // A value of 0.5 means the unit will go the same speed on flat terrain as it does on a 30 degree downhill incline
    public readonly float DirectionalSlopeSensitivity;

    public MobilityType()
    {
        SlopeSensitivity = 1.5f;
        DirectionalSlopeSensitivity = 0.5f;

        Index = MobilityTypes.Count;
        MobilityTypes.Insert(Index, this);
    }
}
