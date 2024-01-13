using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdofaiCSL.API {

    public static class FileUtil {

        /// <summary>
        /// Sort a directory as a song directory.
        /// </summary>
        /// <param name="directoryPath">The path to the directory.</param>
        public static void SortAsSongDirectory(string directoryPath) {

            foreach (string topLevelPath in Directory.GetDirectories(directoryPath)) {

                // If there's a pack file
                if (Directory.GetFiles(topLevelPath, "*.pack").Length > 0) {

                    foreach (string levelPath in Directory.GetDirectories(topLevelPath)) {

                        List<string> chartFilesPaths = Directory.GetFiles(levelPath, "*.adofai").ToList();

                        // Do not count the backup file
                        chartFilesPaths.Remove(Path.Combine(levelPath, "backup.adofai"));

                        // If there is only one file, rename it to "main.adofai"
                        if (chartFilesPaths.Count == 1)
                            File.Move(chartFilesPaths[0], Path.Combine(levelPath, "main.adofai"));

                        // If main.adofai can't be found
                        else if (!File.Exists(Path.Combine(levelPath, "main.adofai")))
                            Main.ModEntry.Logger.Error($"\"main.adofai\" not found in \"{levelPath}\".");
                    }
                } 
                
                else {
                    List<string> chartFilesPaths = Directory.GetFiles(topLevelPath, "*.adofai").ToList();

                    // If there's a song file
                    if (chartFilesPaths.Count > 0) {

                        // Do not count the backup file
                        chartFilesPaths.Remove(Path.Combine(topLevelPath, "backup.adofai"));

                        // If there is only one file, rename it to "main.adofai"
                        if (chartFilesPaths.Count == 1)
                            File.Move(chartFilesPaths[0], Path.Combine(topLevelPath, "main.adofai"));

                        // If main.adofai can't be found
                        else if (!File.Exists(Path.Combine(topLevelPath, "main.adofai")))
                            Main.ModEntry.Logger.Critical($"Multiple .adofai files found in \"{topLevelPath}\". Please rename the correct file to \"main.adofai\"");
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

                IEnumerable<string> lineData = line.Split(new string[] { " = " }, StringSplitOptions.None);

                config.Add(lineData.ElementAt(0), lineData.ElementAt(1));
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