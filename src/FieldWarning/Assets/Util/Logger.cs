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
    /// 
    /// This logger is still a work in progress. The goals are:
    /// -> Ability to remove the messages of specific subsystems at compile time.
    /// TODO: 
    /// -> A ranking of log levels (dump, debug, info, warning, error).
    /// -> Ability to disable the different log levels at run and compile time.
    /// -> Redirecting logging to a file.
    /// </summary>
    public static class Logger
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

        [System.Diagnostics.Conditional("PFW_LOG_DAMAGE")]
        public static void LogDamage(string logMsg)
        {
            Debug.Log($"Damage: {logMsg}");
        }

        [System.Diagnostics.Conditional("PFW_LOG_DAMAGE")]
        public static void LogDamage(string logMsg, Object context)
        {
            Debug.Log($"Damage: {logMsg}", context);
        }
    }
}
