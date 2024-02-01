using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdofaiCSL.API {

    public static class FileUtil {

        /// <summary>
        /// Sort a directory as a song directory.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        public static void SortAsSongDirectory(string path) {

            foreach (string levelPath in Directory.GetDirectories(path)) {

                // If there's a pack file
                if (Directory.GetFiles(levelPath, "*.pack").Length > 0) {

                    foreach (string sublevelPath in Directory.GetDirectories(levelPath)) {

                        List<string> charts = Directory.GetFiles(sublevelPath, "*.adofai").ToList();

                        // Ignore the backup file
                        charts.Remove(Path.Combine(sublevelPath, "backup.adofai"));

                        // If there is only one file, rename it to "main.adofai"
                        if (charts.Count == 1)
                            File.Move(charts[0], Path.Combine(sublevelPath, "main.adofai"));

                        // If main.adofai can't be found
                        else if (!File.Exists(Path.Combine(sublevelPath, "main.adofai")))
                            Main.ModEntry.Logger.Error($"\"main.adofai\" not found in \"{sublevelPath}\".");
                    }
                } 
                
                else {
                    List<string> charts = Directory.GetFiles(levelPath, "*.adofai").ToList();

                    // If there's a song file
                    if (charts.Count > 0) {

                        // Ignore the backup file
                        charts.Remove(Path.Combine(levelPath, "backup.adofai"));

                        // If there is only one file, rename it to "main.adofai"
                        if (charts.Count == 1)
                            File.Move(charts[0], Path.Combine(levelPath, "main.adofai"));

                        // If main.adofai can't be found
                        else if (!File.Exists(Path.Combine(levelPath, "main.adofai")))
                            Main.ModEntry.Logger.Critical($"Multiple .adofai files found in \"{levelPath}\". Please rename the correct file to \"main.adofai\"");
                    }
                }
            }
        }

        /// <summary>
        /// Read a custom config.
        /// </summary>
        /// <param name="path">The path to the custom config.</param>
        /// <returns>The custom config.</returns>
        public static Dictionary<string, string> ReadCustomConfig(string path) {
            Dictionary<string, string> config = new Dictionary<string, string>();

            StreamReader reader = new StreamReader(path);

            foreach (string line in reader.ReadToEnd().Split('\n')) {

                string[] lineData = line.Split(new string[] { " = " }, StringSplitOptions.None);

                config.Add(lineData[0], lineData[1]);
            }

            return config;
        }

        /// <summary>
        /// Write a custom config.
        /// </summary>
        /// <param name="path">The path to the custom config.</param>
        /// <param name="data">The custom config.</param>
        public static void WriteCustomConfig(string path, Dictionary<string, string> data) {
            StreamWriter writer = new StreamWriter(path);

            foreach (KeyValuePair<string, string> kvp in data)
                writer.WriteLine($"{kvp.Key} = {kvp.Value}");
        }
    }
}