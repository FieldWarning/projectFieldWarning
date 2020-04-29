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

namespace PFW
{
    /// <summary>
    /// Intentionally put topmost, please store all constants here.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// When left/right clicking, how much does the mouse
        /// have to be moved before release for it to count as
        /// a drag click instead of a simple click?
        /// </summary>
        public const float MOUSE_DRAG_THRESHOLD = 10.0f;

        public const int MAX_PLATOON_SIZE = 4;
        public const int MIN_PLATOON_SIZE = 1;

        /// <summary>
        /// X meters * MAP_SCALE = Y unity units
        /// </summary>
        public const float MAP_SCALE = 1f / 50f;
    }
}
