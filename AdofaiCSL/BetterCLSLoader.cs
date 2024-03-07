using AdofaiCSL.API.Extensions;
using AdofaiCSL.API.Features;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static HarmonyLib.AccessTools;

namespace AdofaiCSL
{
    public static class BetterCLSLoader
    {
        public static Assembly BetterCLSUnity;

        public static bool IsBetterCLS => BetterCLSUnity is not null;

        public static void LoadCustomFiles(object __instance, object __result)
        {
            Main.SongsDirectory.SortAsSongsDirectory();

            ConcurrentBag<object> entries = OpenPack(Main.SongsDirectory, __instance);

            Type songDetailData = BetterCLSUnity.GetType("BetterCLSUnity.SongDetailData");
            IOrderedEnumerable<object> orderedEntries = entries.OrderBy(entry => (string) Field(songDetailData, "Title").GetValue(entry));

            MethodInfo add = Method(__result.GetType(), "Add");

            foreach (object entry in orderedEntries)
                add.Invoke(__result, [entry]);
        }

        public static ConcurrentBag<object> OpenPack(string path, object __instance)
        {
            ConcurrentBag<object> entries = [];

            Parallel.ForEach(Directory.GetDirectories(path), directory =>
            {
                // Sort pack
                if (Directory.GetFiles(directory, "*.pack").Length > 0)
                    foreach (object entry in OpenPack(directory, __instance))
                        entries.Add(entry);

                // Sort level
                else if (Directory.GetFiles(directory, "*.adofai").Length > 0)
                    entries.Add(AddLevel(directory, __instance));
            });

            return entries;
        }

        public static object AddLevel(string path, object __instance)
        {
            string artist = "";
            string title = "";
            string author = "";

            Type songDetailData = BetterCLSUnity.GetType("BetterCLSUnity.SongDetailData");
            ConstructorInfo constructor = songDetailData.GetConstructor(Type.EmptyTypes);
            object obj = constructor.Invoke([]);
            Field(songDetailData, "Path").SetValue(obj, path);

            using (StreamReader streamReader = File.OpenText(Path.Combine(path, "main.adofai")))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    // Artist
                    if (line.Contains("\"artist\": \""))
                    {
                        artist = line.Split("\"artist\": \"")[1].Split("\",")[0].Trim();
                        artist = Regex.Replace(artist, CustomLevelTileExtensions.NoTagsRegex, string.Empty);
                        Field(songDetailData, "Author").SetValue(obj, artist);
                    }

                    // Title
                    else if (line.Contains("\"song\": \""))
                    {
                        title = line.Split("\"song\": \"")[1].Split("\",")[0].Trim();
                        title = Regex.Replace(title, CustomLevelTileExtensions.NoTagsRegex, string.Empty).Replace("\\n", " ");
                        Field(songDetailData, "Title").SetValue(obj, title);
                    }

                    // Path
                    else if (line.Contains("\"songFilename\": \""))
                    {
                        string songPath = line.Split("\"songFilename\": \"")[1].Split("\"")[0].Trim();
                        if (!string.IsNullOrEmpty(songPath))
                            songPath = Path.Combine(path, songPath);

                        Field(songDetailData, "SongPath").SetValue(obj, songPath);
                    }

                    // Description
                    else if (line.Contains("\"levelDesc\": \""))
                    {
                        string description = line.Split("\"levelDesc\": \"")[1].Split("\",")[0].Trim().Replace("\\n", "\n");
                        Field(songDetailData, "Description").SetValue(obj, description);
                    }

                    // Icon color
                    else if (line.Contains("\"previewIconColor\": \""))
                    {
                        string color = line.Split("\"previewIconColor\": \"")[1].Split("\"")[0].Trim();
                        Field(songDetailData, "TargetColorString").SetValue(obj, color);
                    }

                    // Artist links
                    else if (line.Contains("\"artistLinks\": \""))
                    {
                        string link = line.Split("\"artistLinks\": \"")[1].Split("\"")[0].Trim();
                        if (link.Contains(","))
                            link = link.Split(",")[0];

                        Field(songDetailData, "ArtistLink").SetValue(obj, link);                        
                    }

                    // Difficulty
                    else if (line.Contains("\"difficulty\": "))
                    {
                        string difficultyValue = line.Split("\"difficulty\": ")[1].Split(",")[0].Trim();
                        if (float.TryParse(difficultyValue, out float difficulty))
                        {
                            difficulty *= 1.8f;
                            Field(songDetailData, "Difficulty").SetValue(obj, difficulty);
                        }
                    }

                    // Preview song start
                    else if (line.Contains("\"previewSongStart\": "))
                    {
                        string previewStartValue = line.Split("\"previewSongStart\": ")[1].Split(",")[0].Trim();
                        if (int.TryParse(previewStartValue, out int previewStart))
                            Field(songDetailData, "PreviewSongStart").SetValue(obj, previewStart);
                    }

                    // Preview song duration / end
                    else if (line.Contains("\"previewSongDuration\": "))
                    {
                        string previewDurationValue = line.Split("\"previewSongDuration\": ")[1].Split(",")[0].Trim();
                        if (int.TryParse(previewDurationValue, out var previewDuration))
                        {
                            int previewStart = (int) Field(songDetailData, "PreviewSongStart").GetValue(obj);
                            int previewEnd = previewStart + previewDuration;

                            if (previewEnd == 0)
                                previewEnd = 10;

                            Field(songDetailData, "PreviewSongEnd").SetValue(obj, previewEnd);
                        }
                    }
                    
                    // Preview image
                    else if (line.Contains("\"previewImage\": \""))
                    {
                        string imagePath = line.Split("\"previewImage\": \"")[1].Split("\"")[0].Trim();
                        if (!string.IsNullOrEmpty(imagePath))
                            imagePath = Path.Combine(path, imagePath);

                        Field(songDetailData, "PreviewImagePath").SetValue(obj, imagePath);
                    }

                    // Author
                    else if (line.Contains("\"author\": \""))
                    {
                        author = line.Split("\"author\": \"")[1].Split("\"")[0].Trim();
                        Field(songDetailData, "Maker").SetValue(obj, author);
                    }

                    // Tags
                    else if (line.Contains("\"levelTags\": \""))
                    {
                        Field(songDetailData, "RequireNeocosmos").SetValue(obj, line.Contains("Neo Cosmos"));
                    }

                    // Angle data / Tile count
                    else if (line.Contains("\"angleData\": ["))
                    {
                        string angleData = line.Split("\"angleData\": [")[1].Split("], ")[0].Trim();
                        Field(songDetailData, "TileCount").SetValue(obj, angleData.Split(", ").Length);
                    }

                    // Path data / Tile count
                    else if (line.Contains("\"pathData\": \""))
                    {
                        string pathData = line.Split("\"pathData\": \"")[1].Split("\"")[0].Trim();
                        Field(songDetailData, "TileCount").SetValue(obj, pathData.Length);
                    }

                    // BPM
                    else if (line.Contains("\"bpm\": "))
                    {
                        Field(songDetailData, "BPM").SetValue(obj, line.Split("\"bpm\": ")[1].Split(",")[0].Trim());
                        break;
                    }
                }
            }

            Type utils = BetterCLSUnity.GetType("BetterCLSUnity.Utils");
            MethodInfo getMd5Hash = utils.GetMethod("GetMd5Hash");
            string songId = (string) getMd5Hash.Invoke(null, [author + artist + title]);
            Field(songDetailData, "SongID").SetValue(obj, songId);

            Type controller = BetterCLSUnity.GetType("BetterCLSUnity.SongListController");

            bool useXAccuracy = (bool) Field(controller, "UseXAccuracy").GetValue(null);
            
            Type adofaiAPI = BetterCLSUnity.GetType("BetterCLSUnity.AdofaiAPI");
            
            MethodInfo getXAccuracy = adofaiAPI.GetMethod("GetCustomWorldXAccuracy");
            MethodInfo getAccuracy = adofaiAPI.GetMethod("GetCustomWorldAccuracy");
            float maxAccuracy = (useXAccuracy ? (float) getXAccuracy.Invoke(null, [songId]) : (float) getAccuracy.Invoke(null, [songId])) * 100;
            Field(songDetailData, "MaxAccuracy").SetValue(obj, maxAccuracy);

            MethodInfo getCompletion = adofaiAPI.GetMethod("GetCustomWorldCompletion");
            float maxProgress = (float) getCompletion.Invoke(null, [songId]) * 100;
            Field(songDetailData, "MaxProgress").SetValue(obj, maxProgress);

            MethodInfo isMaxAcc = adofaiAPI.GetMethod("GetCustomWorldIsHighestPossibleAcc");
            bool isPerfect = (bool) isMaxAcc.Invoke(null, [songId]);
            Field(songDetailData, "IsPerfect").SetValue(obj, isPerfect);

            MethodInfo getAttemps = adofaiAPI.GetMethod("GetCustomWorldAttempts");
            int attempt = (int) getAttemps.Invoke(null, [songId]);
            Field(songDetailData, "Attempt").SetValue(obj, attempt);

            Dictionary<string, Dictionary<string, bool>> _customSongs = (Dictionary<string, Dictionary<string, bool>>) Field(controller, "_customSongs").GetValue(__instance);

            if (_customSongs["favorite"].ContainsKey(songId))
                Field(songDetailData, "IsFavorite").SetValue(obj, true);

            return obj;
        }
    }
}
