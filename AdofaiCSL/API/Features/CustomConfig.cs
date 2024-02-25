﻿using ADOFAI;
using System;
using System.Collections.Generic;
using System.IO;

namespace AdofaiCSL.API.Features {

    public static class CustomConfig {

        /// <summary>
        /// Read a custom config.
        /// </summary>
        /// <param name="path">The path to the custom config.</param>
        /// <returns>The custom config or null if it can't read it.</returns>
        public static Dictionary<string, string> Read(string path) {

            StreamReader reader = null;

            try {
                Dictionary<string, string> config = new Dictionary<string, string>();

                reader = new StreamReader(path);

                foreach (string line in reader.ReadToEnd().Split('\n')) {

                    string[] lineData = line.Split(new string[] { " = " }, StringSplitOptions.None);

                    config.Add(lineData[0].Trim(), lineData.Length == 1 ? null : lineData[1].Trim());
                }

                reader.Close();

                return config;
            } 
            
            catch (Exception e) {
                Main.ModEntry.Logger.Error($"An exception occured while reading config at {path}: {e.GetType().Name} - {e.Message}\n{e.StackTrace}");
                reader?.Close();
                return null;
            }
        }

        /// <summary>
        /// Write a custom config.
        /// </summary>
        /// <param name="path">The path to the custom config.</param>
        /// <param name="data">The custom config.</param>
        public static void Write(string path, Dictionary<string, string> data) {

            StreamWriter writer = null;

            try {
                writer = new StreamWriter(path);

                foreach (KeyValuePair<string, string> kvp in data)
                    writer.WriteLine($"{kvp.Key} = {kvp.Value}");

                writer.Close();
            }
            
            catch (Exception e) {
                Main.ModEntry.Logger.Error($"An exception occured while writing config to {path}: {e.GetType().Name} - {e.Message}\n{e.StackTrace}");
                writer?.Close();
            }
        }

        /// <summary>
        /// Creates a new <see cref="FolderDataCLS"/> with a pack config.
        /// </summary>
        /// <param name="packConfig">The pack config.</param>
        /// <param name="path">The path to the pack.</param>
        /// <returns>The <see cref="FolderDataCLS"/>.</returns>
        public static FolderDataCLS AsPackData(this Dictionary<string, string> packConfig, string path) {
            return packConfig is null ? 
                null :
                new FolderDataCLS(
                    packConfig.TryGetValue("title", out string title) ? title : "",
                    (packConfig.TryGetValue("difficulty", out string difficultyValue) && int.TryParse(difficultyValue, out int difficulty)) ? difficulty : 1,
                    packConfig.TryGetValue("artist", out string artist) ? artist : "",
                    packConfig.TryGetValue("author", out string author) ? author : "",
                    packConfig.TryGetValue("description", out string description) ? description : "",
                    packConfig.ContainsKey("image") ? packConfig["image"].Trim() : "",
                    packConfig.ContainsKey("icon") ? packConfig["icon"].Trim() : "",
                    packConfig.ContainsKey("color") ? packConfig["color"].HexToColor() : default
                );
        }
    }
}
