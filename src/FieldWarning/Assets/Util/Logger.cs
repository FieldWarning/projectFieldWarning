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
    /// Features:
    /// -> Ability to enable the messages of specific subsystems at compile time.
    /// [enable PFW_LOG_ALL or name a specific subsystem, e.g. PFW_LOG_NETWORKING]
    /// -> Ability to disable the different log levels at compile time.
    /// [enable PFW_DEBUG to disable DUMP, enable PFW_ERROR to only see ERROR and BUG etc]
    /// TODO: 
    /// -> Ability to remove the messages of specific subsystems at compile time.
    /// -> Redirecting logging to a file.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Logging without subsytem should be avoided,
        /// as it can't be disabled except by disabling
        /// all messages of that severity level.
        /// </summary>
        [System.Diagnostics.Conditional("PFW_LOG_ALL")]
        public static void LogWithoutSubsystem(
                LogLevel level,
                string logMsg)
        {
            Log(logMsg, "", level);
        }

        /// <summary>
        /// Logging without subsytem should be avoided,
        /// as it can't be disabled except by disabling
        /// all messages of that severity level.
        /// </summary>
        [System.Diagnostics.Conditional("PFW_LOG_ALL")]
        public static void LogWithoutSubsystem(
                LogLevel level,
                Object context,
                string logMsg)
        {
            Log(logMsg, context, "", level);
        }

        [System.Diagnostics.Conditional("PFW_LOG_ALL")]
        [System.Diagnostics.Conditional("PFW_LOG_LOADING")]
        public static void LogLoading(
                LogLevel level,
                string logMsg)
        {
            Log(logMsg, "Loading", level);
        }

        [System.Diagnostics.Conditional("PFW_LOG_ALL")]
        [System.Diagnostics.Conditional("PFW_LOG_LOADING")]
        public static void LogLoading(
                LogLevel level,
                Object context,
                string logMsg)
        {
            Log(logMsg, context, "Loading", level);
        }

        [System.Diagnostics.Conditional("PFW_LOG_ALL")]
        [System.Diagnostics.Conditional("PFW_LOG_NETWORKING")]
        public static void LogNetworking(
                LogLevel level,
                string logMsg)
        {
            Log(logMsg, "Networking", level);
        }

        [System.Diagnostics.Conditional("PFW_LOG_ALL")]
        [System.Diagnostics.Conditional("PFW_LOG_NETWORKING")]
        public static void LogNetworking(
                LogLevel level,
                Object context,
                string logMsg)
        {
            Log(logMsg, context, "Networking", level);
        }

        [System.Diagnostics.Conditional("PFW_LOG_ALL")]
        [System.Diagnostics.Conditional("PFW_LOG_TARGETING")]
        public static void LogTargeting(
                LogLevel level,
                string logMsg)
        {
            Log(logMsg, "Targeting", level);
        }

        [System.Diagnostics.Conditional("PFW_LOG_ALL")]
        [System.Diagnostics.Conditional("PFW_LOG_TARGETING")]
        public static void LogTargeting(
                LogLevel level,
                Object context,
                string logMsg)
        {
            Log(logMsg, context, "Targeting", level);
        }

        [System.Diagnostics.Conditional("PFW_LOG_ALL")]
        [System.Diagnostics.Conditional("PFW_LOG_DAMAGE")]
        public static void LogDamage(
                LogLevel level,
                string logMsg)
        {
            Log(logMsg, "Damage", level);
        }

        [System.Diagnostics.Conditional("PFW_LOG_ALL")]
        [System.Diagnostics.Conditional("PFW_LOG_DAMAGE")]
        public static void LogDamage(
                LogLevel level,
                Object context,
                string logMsg)
        {
            Log(logMsg, context, "Damage", level);
        }

        [System.Diagnostics.Conditional("PFW_LOG_ALL")]
        [System.Diagnostics.Conditional("PFW_LOG_PATHFINDING")]
        public static void LogPathfinding(
                LogLevel level,
                string logMsg)
        {
            Log(logMsg, "Pathfinding", level);
        }

        [System.Diagnostics.Conditional("PFW_LOG_ALL")]
        [System.Diagnostics.Conditional("PFW_LOG_PATHFINDING")]
        public static void LogPathfinding(
                LogLevel level,
                Object context,
                string logMsg)
        {
            Log(logMsg, context, "Pathfinding", level);
        }

        [System.Diagnostics.Conditional("PFW_LOG_ALL")]
        [System.Diagnostics.Conditional("PFW_LOG_MENU")]
        public static void LogMenu(
                LogLevel level,
                string logMsg)
        {
            Log(logMsg, "Menu", level);
        }

        [System.Diagnostics.Conditional("PFW_LOG_ALL")]
        [System.Diagnostics.Conditional("PFW_LOG_MENU")]
        public static void LogMenu(
                LogLevel level,
                Object context,
                string logMsg)
        {
            Log(logMsg, context, "Menu", level);
        }

        [System.Diagnostics.Conditional("PFW_LOG_ALL")]
        [System.Diagnostics.Conditional("PFW_LOG_CONFIG")]
        public static void LogConfig(
                LogLevel level,
                string logMsg)
        {
            Log(logMsg, "Config", level);
        }

        [System.Diagnostics.Conditional("PFW_LOG_ALL")]
        [System.Diagnostics.Conditional("PFW_LOG_CONFIG")]
        public static void LogConfig(
                LogLevel level,
                Object context,
                string logMsg)
        {
            Log(logMsg, context, "Config", level);
        }

        private static void Log(
                string logMsg,
                string subsystem,
                LogLevel level = LogLevel.DEBUG)
        {
            switch (level)
            {
                case LogLevel.DUMP:
#if PFW_DEBUG
                    break;
#endif
                case LogLevel.DEBUG:
#if PFW_INFO
                    break;
#endif
                case LogLevel.INFO:
#if PFW_WARNING
                    break;
#endif
                case LogLevel.WARNING:
#if PFW_ERROR
                    break;
#endif
                case LogLevel.ERROR:
#if PFW_BUG
                    break;
#endif
                case LogLevel.BUG:
#if PFW_QUIET
                    break;
#endif
                default:
                    Debug.Log($"[{level}] {subsystem}: {logMsg}");
                    break;
            }
        }

        private static void Log(
                string logMsg,
                Object context, 
                string subsystem,
                LogLevel level = LogLevel.DEBUG)
        {
            switch (level)
            {
                case LogLevel.DUMP:
#if PFW_DEBUG
                    break;
#endif
                case LogLevel.DEBUG:
#if PFW_INFO
                    break;
#endif
                case LogLevel.INFO:
#if PFW_WARNING
                    break;
#endif
                case LogLevel.WARNING:
#if PFW_ERROR
                    break;
#endif
                case LogLevel.ERROR:
#if PFW_BUG
                    break;
#endif
                case LogLevel.BUG:
#if PFW_QUIET
                    break;
#endif
                default:
                    Debug.Log($"[{level}] {subsystem}: {logMsg}", context);
                    break;
            }
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
