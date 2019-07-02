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

namespace PFW
{
    /// <summary>
    /// To enable logging for a subsystem, go to file->build settings->player settings,
    /// then add the respective define name (e.g. PFW_LOG_NETWORKING)
    /// to 'Scripting Define Symbols'
    ///
    /// Alternatively, you can write '#define PFW_LOG_NETWORKING' at the top of all
    /// files that call the relevant logging function (LogNetworking).
    /// </summary>
    public static class Logging
    {
        [System.Diagnostics.Conditional("PFW_LOG_NETWORKING")]
        public static void LogNetworking(string logMsg)
        {
            Debug.Log($"Networking: {logMsg}");
        }

        [System.Diagnostics.Conditional("PFW_LOG_NETWORKING")]
        public static void LogNetworking(string logMsg, Object context)
        {
            Debug.Log($"Networking: {logMsg}", context);
        }

        [System.Diagnostics.Conditional("PFW_LOG_TARGETING")]
        public static void LogTargeting(string logMsg)
        {
            Debug.Log($"Targeting: {logMsg}");
        }

        [System.Diagnostics.Conditional("PFW_LOG_TARGETING")]
        public static void LogTargeting(string logMsg, Object context)
        {
            Debug.Log($"Targeting: {logMsg}", context);
        }

        [System.Diagnostics.Conditional("PFW_LOG_VISION")]
        public static void LogVision(string logMsg)
        {
            Debug.Log($"Vision: {logMsg}");
        }

        [System.Diagnostics.Conditional("PFW_LOG_VISION")]
        public static void LogVision(string logMsg, Object context)
        {
            Debug.Log($"Vision: {logMsg}", context);
        }

        [System.Diagnostics.Conditional("PFW_LOG_SPAWN")]
        public static void LogSpawn(string logMsg)
        {
            Debug.Log($"Spawn: {logMsg}");
        }

        [System.Diagnostics.Conditional("PFW_LOG_SPAWN")]
        public static void LogSpawn(string logMsg, Object context)
        {
            Debug.Log($"Spawn: {logMsg}", context);
        }
    }
}
