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
using System.IO;
using UnityEngine;

using PFW.Model.Armory;
using PFW.Model.Armory.JsonContents;
using PFW.Model.Settings.JsonContents;
using Newtonsoft.Json;

namespace PFW
{
    public static class ConfigReader
    {
        public static Deck ParseDeck(string deckName, Armory armory)
        {
            TextAsset configFile = Resources.Load<TextAsset>($"Decks/{deckName}");
            DeckConfig config = JsonConvert.DeserializeObject<DeckConfig>(configFile.text);

            return new Deck(config, armory);
        }

        /// <summary>
        /// Given two maps of json files (which may be the same),
        /// replace all #include lines in the specified file from the first map
        /// with the content of files in the second map. If that content
        /// also contains #include lines, recursively replace those too.
        /// </summary>
        private static void ResolveIncludesRecursive(
                Dictionary<string, string> fileNameToFileContents,
                string filename,
                Dictionary<string, string> includeTargets,
                int depth = 0)
        {
            const int MAX_DEPTH = 10;
            const string INCLUDE_FIELD = "#include";

            Logger.LogConfig(LogLevel.DEBUG,
                    $"Resolving includes in {filename}.json.");

            if (depth >= MAX_DEPTH)
            {
                Logger.LogConfig(
                        LogLevel.ERROR,
                        $"Max recursion level ({MAX_DEPTH}) reached when " +
                        $"parsing the includes in {filename}. " +
                        $"There is very likely an include loop here.");
                return;
            }

            int pos = 0;
            while (pos != -1)
            {
                pos = fileNameToFileContents[filename].IndexOf(INCLUDE_FIELD, pos);
                if (pos != -1)
                {
                    int includeTargetNameStart = fileNameToFileContents[filename].IndexOf("\"", pos) + 1;
                    int includeTargetNameEnd = fileNameToFileContents[filename].IndexOf("\"", includeTargetNameStart);
                    if (includeTargetNameStart == 0 || includeTargetNameEnd == -1)
                    {
                        Logger.LogConfig(
                                LogLevel.ERROR,
                                $"Found what looks like an {INCLUDE_FIELD} directive, " +
                                "but it was not followed by a file name surrounded in " +
                                "double quotes (\").");
                        break;
                    }

                    string includeTargetName = fileNameToFileContents[filename].Substring(
                            includeTargetNameStart,
                            includeTargetNameEnd - includeTargetNameStart);

                    if (includeTargets.ContainsKey(includeTargetName))
                    {
                        ResolveIncludesRecursive(
                                includeTargets, 
                                includeTargetName,
                                includeTargets,
                                depth++);

                        // Discard braces
                        int includeStart = includeTargets[includeTargetName].IndexOf('{') + 1;
                        int includeEnd = includeTargets[includeTargetName].LastIndexOf('}');

                        // Some files have a trailing comma before the last brace, we have
                        // to discard that if present:
                        string includeContent = includeTargets[includeTargetName].Substring(
                                    includeStart, includeEnd - includeStart);
                        includeContent = includeContent.TrimEnd();
                        includeContent = includeContent.TrimEnd(new char[]{','});

                        fileNameToFileContents[filename] =
                            fileNameToFileContents[filename].Substring(
                                    0, pos)
                            + includeContent
                            + fileNameToFileContents[filename].Substring(
                                    includeTargetNameEnd + 1);

                        pos += includeTargets[includeTargetName].Length;
                    }
                }
            }
        }

        private static void ResolveIncludes(
                List<string> filenames,
                Dictionary<string, string> fileNameToFileContents,
                Dictionary<string, string> includeTargets)
        {
            foreach (string filename in filenames)
            {
                ResolveIncludesRecursive(
                        fileNameToFileContents, filename, includeTargets);
            }
        }

        private static Dictionary<string, string> ReadAllJsonFiles(
                string directory)
        {
            string[] configFiles = new string[0];
            try
            {
                configFiles = Directory.GetFiles(directory, "*.json", SearchOption.AllDirectories);
            }
            catch (System.Exception)
            {
                Logger.LogConfig(LogLevel.ERROR,
                        $"Could not access {directory} " +
                        "due to a system exception.");
            }

            var fileNameToFileContents = new Dictionary<string, string>();
            foreach (string configFile in configFiles)
            {
                // Turn 'C://UnitConfigTemplates/Tank.json' into 'Tank', which
                // is how this will be referred to in the 'Inherits' field
                // of the unit configs (or in the decks config files)
                string shortFileName = configFile.Substring(
                        directory.Length, configFile.Length - ".json".Length - directory.Length);
                shortFileName = shortFileName.Replace('\\', '/');  // Unix directory separators

                string configText = File.ReadAllText(configFile);
                fileNameToFileContents.Add(shortFileName, configText);
            }

            return fileNameToFileContents;
        }

        /// <summary>
        /// Parses all json files in the given directory and subdirectories.
        /// If the includeTargets argument is set, files that contain
        /// a line with ' #include "xyz" ' will have that line replaced
        /// with the contents of xyz.json, minus its enclosing brackets.
        /// 
        /// includeTargets is a map of json file names -> json file contents.
        /// </summary>
        private static Dictionary<string, T> ParseAllJsonFiles<T>(
                string directory, Dictionary<string, string> includeTargets = null)
        {
            Dictionary<string, string> fileNameToFileContents = ReadAllJsonFiles(directory);

            List<string> shortFilenames = new List<string>(fileNameToFileContents.Keys);

            if (includeTargets != null)
            {
                ResolveIncludes(shortFilenames, fileNameToFileContents, includeTargets);
            }

            var result = new Dictionary<string, T>();
            foreach (string shortFileName in shortFilenames)
            {
                Logger.LogConfig(LogLevel.DEBUG,
                        $"Parsing config file: {shortFileName}.");

                result.Add(
                        shortFileName,
                        JsonConvert.DeserializeObject<T>(
                                fileNameToFileContents[shortFileName]));
            }

            return result;
        }

        public static Armory ParseArmory()
        {
            // Load the includable unit configs, which don't turn into real units but
            // are #included by real units and each other.
            string includablesPath = Application.streamingAssetsPath +
                    "/UnitConfigsIncludable/";

            Logger.LogConfig(LogLevel.INFO, "Parsing unit template configs.");
            Dictionary<string, string> includableConfigs = ReadAllJsonFiles(
                    includablesPath);

            // We take all jsons in the UnitConfigs folder and subfolders
            string unitsPath = Application.streamingAssetsPath +
                    "/UnitConfigs/";

            Logger.LogConfig(LogLevel.INFO, "Parsing unit configs.");
            Dictionary<string, UnitConfig> configs = ParseAllJsonFiles<UnitConfig>(
                    unitsPath, includableConfigs);

            return new Armory(configs);
        }

        public static SettingsConfig ParseDefaultSettingsRaw()
        {
            string path = Application.streamingAssetsPath +
                    "/Settings/DefaultSettings.json";
            if (!File.Exists(path))
            {
                Logger.LogConfig(
                        LogLevel.ERROR,
                        $"Corrupted installation: Default settings file not found at {path}");
                return null;
            }
            else
            {
                string configText = File.ReadAllText(path);
                SettingsConfig config =
                    JsonConvert.DeserializeObject<SettingsConfig>(configText);

                return config;
            }
        }

        public static SettingsConfig ParseSettingsRaw()
        {
            SettingsConfig config = ParseDefaultSettingsRaw();

            // The local settings file overrides anything set in the default
            // settings file. This file may have partial content or not exist
            // at all.
            // We need to load it like this and not as an asset, because
            // its contents can be changed during runtime.
            string path = Application.streamingAssetsPath +
                    "/Settings/LocalSettings.json";
            if (!File.Exists(path))
            {
                return config;
            }
            else
            {
                string localSettingsText = File.ReadAllText(path);
                SettingsConfig config2 =
                        JsonConvert.DeserializeObject<SettingsConfig>(localSettingsText);

                return SettingsConfig.MergeSettings(config, config2);
            }
        }

        public static void WriteLocalConfig(SettingsConfig localConfig)
        {
            string path = Application.streamingAssetsPath + 
                    "/Settings/LocalSettings.json";

            string contents = JsonConvert.SerializeObject(localConfig, Formatting.Indented);

            // Overwrite the file if it exists
            using (FileStream fs = File.Create(path))
            {
                foreach (char c in contents)
                {
                    fs.WriteByte((byte)c);
                }
            }
        }

        public static Dictionary<string, DeckConfig> ParseDecksRaw() 
        {
            string templatesPath = Application.streamingAssetsPath +
                    "/Decks/";

            Logger.LogConfig(LogLevel.INFO, "Parsing local decks.");
            return ParseAllJsonFiles<DeckConfig>(templatesPath); ;
        }
    }
}
