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
    /// -> Ability to disable the different log levels at run and compile time.
    /// -> Redirecting logging to a file.
    /// </summary>
    public static class Logger
    {
        [System.Diagnostics.Conditional("PFW_LOG_ALL")]
        [System.Diagnostics.Conditional("PFW_LOG_NETWORKING")]
        public static void LogNetworking(
                string logMsg,
                LogLevel level = LogLevel.DEBUG)
        {
            Debug.Log($"[{level}] Networking: {logMsg}");
        }

        [System.Diagnostics.Conditional("PFW_LOG_ALL")]
        [System.Diagnostics.Conditional("PFW_LOG_NETWORKING")]
        public static void LogNetworking(
                string logMsg,
                Object context,
                LogLevel level = LogLevel.DEBUG)
        {
            Debug.Log($"[{level}] Networking: {logMsg}", context);
        }

        [System.Diagnostics.Conditional("PFW_LOG_ALL")]
        [System.Diagnostics.Conditional("PFW_LOG_TARGETING")]
        public static void LogTargeting(
                string logMsg,
                LogLevel level = LogLevel.DEBUG)
        {
            Debug.Log($"[{level}] Targeting: {logMsg}");
        }

        [System.Diagnostics.Conditional("PFW_LOG_ALL")]
        [System.Diagnostics.Conditional("PFW_LOG_TARGETING")]
        public static void LogTargeting(
                string logMsg,
                Object context,
                LogLevel level = LogLevel.DEBUG)
        {
            Debug.Log($"[{level}] Targeting: {logMsg}", context);
        }

        [System.Diagnostics.Conditional("PFW_LOG_ALL")]
        [System.Diagnostics.Conditional("PFW_LOG_DAMAGE")]
        public static void LogDamage(
                string logMsg,
                LogLevel level = LogLevel.DEBUG)
        {
            Debug.Log($"[{level}] Damage: {logMsg}");
        }

        [System.Diagnostics.Conditional("PFW_LOG_ALL")]
        [System.Diagnostics.Conditional("PFW_LOG_DAMAGE")]
        public static void LogDamage(
                string logMsg,
                Object context,
                LogLevel level = LogLevel.DEBUG)
        {
            Debug.Log($"[{level}] Damage: {logMsg}", context);
        }

        [System.Diagnostics.Conditional("PFW_LOG_ALL")]
        [System.Diagnostics.Conditional("PFW_LOG_PATHFINDING")]
        public static void LogPathfinding(
                string logMsg,
                LogLevel level = LogLevel.DEBUG)
        {
            Debug.Log($"[{level}] Pathfinding: {logMsg}");
        }

        [System.Diagnostics.Conditional("PFW_LOG_ALL")]
        [System.Diagnostics.Conditional("PFW_LOG_PATHFINDING")]
        public static void LogPathfinding(
                string logMsg,
                Object context,
                LogLevel level = LogLevel.DEBUG)
        {
            Debug.Log($"[{level}] Pathfinding: {logMsg}", context);
        }

        [System.Diagnostics.Conditional("PFW_LOG_ALL")]
        [System.Diagnostics.Conditional("PFW_LOG_CONFIG")]
        public static void LogConfig(
                string logMsg,
                LogLevel level = LogLevel.DEBUG)
        {
            Debug.Log($"[{level}] Config: {logMsg}");
        }

        [System.Diagnostics.Conditional("PFW_LOG_ALL")]
        [System.Diagnostics.Conditional("PFW_LOG_CONFIG")]
        public static void LogConfig(
                string logMsg,
                Object context,
                LogLevel level = LogLevel.DEBUG)
        {
            Debug.Log($"[{level}] Config: {logMsg}", context);
        }
    }

    /// <summary>
    /// Severity levels for the logger, ordered in
    /// from lest to most important.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Wordy debug output that describes minor
        /// details and is liable to flood the console.
        /// </summary>
        DUMP,
        /// <summary>
        /// Debug output that may help trace the logic
        /// but is not relevant for players.
        /// </summary>
        DEBUG,
        /// <summary>
        /// Output about normal events that may nonetheless
        /// be informational.
        /// </summary>
        INFO,
        /// <summary>
        /// Output about unusual events, such as failing
        /// to open a file. A warning may be the cause of
        /// an error down the line, but it may just indicate
        /// an allowed but highly unusual/suspicious state.
        /// </summary>
        WARNING,
        /// <summary>
        /// Output about errors detected during play.
        /// It may be possible to recover from an error,
        /// but it will usually lead to degradation such as
        /// not loading a unit.
        /// </summary>
        ERROR,
        /// <summary>
        /// Output about things that must never happen
        /// (hitting such output indicates a bug in the
        /// program logic).
        /// </summary>
        BUG
    }
}
