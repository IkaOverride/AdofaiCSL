using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AdofaiCSL.API.Features {

    public static class SongsDirectory {

        /// <summary>
        /// Sort a directory as a songs directory.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        public static void SortAsSongsDirectory(this string path) {

            foreach (string directory in Directory.GetDirectories(path)) {

                // Sort pack
                if (Directory.GetFiles(directory, "*.pack").Length > 0)
                    directory.SortAsPackDirectory();

                // Sort level
                else if (Directory.GetFiles(directory, "*.adofai").Length > 0)
                    directory.SortAsLevelDirectory();
            }
        }

        /// <summary>
        /// Sort a directory as a level directory.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        public static void SortAsLevelDirectory(this string path) {

            List<string> charts = Directory.GetFiles(path, "*.adofai").ToList();

            // Ignore the backup file
            charts.Remove(Path.Combine(path, "backup.adofai"));

            // If there is only one file, rename it to "main.adofai"
            if (charts.Count == 1)
                File.Move(charts[0], Path.Combine(path, "main.adofai"));

            // If multiple .adofai files are detected and not one of them is main.adofai
            else if (charts.Count > 1 && !File.Exists(Path.Combine(path, "main.adofai")))
                Main.ModEntry.Logger.Warning($"Multiple .adofai files found in \"{path}\". Please rename the correct file to \"main.adofai\"");
        }

        /// <summary>
        /// Sort a directory as a pack directory.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        public static void SortAsPackDirectory(this string path) {

            foreach (string directory in Directory.GetDirectories(path)) {

                List<string> configs = Directory.GetFiles(path, "*.pack").ToList();

                // If there is only one file, rename it to "main.adofai"
                if (configs.Count == 1)
                    File.Move(configs[0], Path.Combine(path, "main.pack"));

                // If multiple .adofai files are detected and not one of them is main.adofai
                else if (configs.Count > 1 && !File.Exists(Path.Combine(path, "main.pack")))
                    Main.ModEntry.Logger.Warning($"Multiple .pack files found in \"{path}\". Please rename the correct file to \"main.pack\"");

                // Sort pack
                if (Directory.GetFiles(directory, "*.pack").Length > 0)
                    directory.SortAsPackDirectory();

                // Sort level
                else if (Directory.GetFiles(directory, "*.adofai").Length > 0)
                    directory.SortAsLevelDirectory();
            }
        }

        public static string GetUniqueDirectoryPath(this string path) {
            
            if (!Directory.Exists(path)) 
                return path;

            path = Regex.Replace(path, @" \(\d+\)$", string.Empty);

            int i = 1;
            while (Directory.Exists($"{path} ({i})"))
                i++;

            return $"{path} ({i})";
        }

        public static void CreatePack(this string path) {
            string configPath = Path.Combine(path, "main.pack");
            Dictionary<string, string> newPackConfig = new Dictionary<string, string>() {
                    { "title", "Pack" },
                    { "author", "AdofaiCSL" },
                    { "artist", "AdofaiCSL" },
                    { "difficulty", "1" },
                    { "description", string.Empty },
                    { "image", string.Empty },
                    { "icon", string.Empty },
                    { "color", default(Color).ToHex() }
                };

            Directory.CreateDirectory(path);
            File.Create(configPath).Close();
            CustomConfig.Write(configPath, newPackConfig);
        }
    }
}
