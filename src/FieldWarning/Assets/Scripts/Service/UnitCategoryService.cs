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

using System.Collections.Generic;
using PFW.Model.Armory;
using UnityEngine;

namespace PFW.Service
{
    public class UnitCategoryService : MonoBehaviour, IUnitCategoryService
    {
        private  ICollection<UnitCategory> _categories;

        public void Awake()
        {
            _categories = new List<UnitCategory>();

            _categories.Add(new UnitCategory() { Name = "LOG" });
            _categories.Add(new UnitCategory() { Name = "INF" });
            _categories.Add(new UnitCategory() { Name = "SUP" });
            _categories.Add(new UnitCategory() { Name = "TNK" });
            _categories.Add(new UnitCategory() { Name = "REC" });
            _categories.Add(new UnitCategory() { Name = "VHC" });
            _categories.Add(new UnitCategory() { Name = "HEL" });
        }

        public ICollection<UnitCategory> All()
        {
            return _categories;
        }
    }
}

