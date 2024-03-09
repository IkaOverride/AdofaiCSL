using ADOFAI;
using AdofaiCSL.API.Features;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static HarmonyLib.Code;
using Object = UnityEngine.Object;

namespace AdofaiCSL.API.Extensions
{
    public static class scnCLSExtensions
    {
        /// <summary>
        /// Check if a key is a valid level key.
        /// </summary>
        /// <param name="screen">The <see cref="scnCLS"/>.</param>
        /// <param name="key">The level's key.</param>
        /// <returns>If the key is a valid level key.</returns>
        public static bool IsKeyValid(this scnCLS screen, string key) => key != null && screen.loadedLevels.ContainsKey(key);

        public static bool IsKeyWorkshop(this scnCLS screen, string key) => screen.isWorkshopLevel.TryGetValue(key, out bool isWorkshop) && isWorkshop;

        public static bool IsKeySearched(this scnCLS screen, string key)
        {
            GenericDataCLS genericDataCLS = screen.loadedLevels[key];
            string[] array = [genericDataCLS.artist, genericDataCLS.author, genericDataCLS.title];
            bool flag = false;
            if (genericDataCLS.parentFolderName == screen.currentFolderName)
            {
                if (!screen.searchParameter.IsNullOrEmpty())
                {
                    string[] array2 = array;
                    for (int i = 0; i < array2.Length; i++)
                    {
                        if (array2[i].RemoveRichTags().ToLower().Contains(screen.searchParameter.ToLower()))
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                else
                {
                    flag = true;
                }
            }

            return flag;
        }

        /// <summary>
        /// Create a custom tile.
        /// </summary>
        /// <param name="screen">The <see cref="scnCLS"/>.</param>
        /// <param name="y">The vertical offset of the tile.</param>
        /// <returns>The tile's <see cref="GameObject"/>.</returns>
        public static GameObject CreateTile(this scnCLS screen, float y)
        {
            // Instantiate the tile object
            GameObject gameObject = Object.Instantiate(screen.tilePrefab, screen.floorContainer);
            gameObject.transform.LocalMoveY(y);

            // Setup the floor
            scrFloor floor = gameObject.GetComponent<scrFloor>();
            floor.topGlow.gameObject.SetActive(true);
            floor.isLandable = true;

            return gameObject;
        }

        /// <summary>
        /// Load a tile.
        /// </summary>
        /// <param name="screen">The <see cref="scnCLS"/>.</param>
        /// <param name="path">The path to the level.</param>
        /// <param name="tile">The <see cref="CustomLevelTile"/>.</param>
        /// <param name="data">The tile's <see cref="GenericDataCLS"/>.</param>
        public static void LoadTile(this scnCLS screen, string path, CustomLevelTile tile, GenericDataCLS data)
        {
            screen.sortedLevelKeys.Add(tile.levelKey);
            screen.loadedLevels[tile.levelKey] = data;
            screen.loadedLevelTiles[tile.levelKey] = tile;
            screen.loadedLevelDirs[tile.levelKey] = path;
            screen.loadedLevelIsDeleted[tile.levelKey] = false;
            screen.levelCount += 1;
        }

        /// <summary>
        /// Add all levels in a directory.
        /// </summary>
        /// <param name="screen">The <see cref="scnCLS"/>.</param>
        /// <param name="path">The path to the songs directory.</param>
        public static void AddLevels(this scnCLS screen, string path)
        {
            foreach (string directory in Directory.GetDirectories(path))
            {
                try
                {
                    // Add pack
                    if (Directory.GetFiles(directory, "main.pack").Length > 0)
                        screen.AddPack(directory);

                    // Add level
                    else if (Directory.GetFiles(directory, "main.adofai").Length > 0)
                        screen.AddLevel(directory);
                }

                catch (Exception e)
                {
                    Main.ModEntry.Logger.Error($"Could not load the level or pack at '{directory}'. Error: '{e.GetType().Name} - {e.Message}\n{e.StackTrace}'");
                }
            }
        }

        /// <summary>
        /// Add a custom level to a <see cref="scnCLS"/>.
        /// </summary>
        /// <param name="screen">The <see cref="scnCLS"/>.</param>
        /// <param name="path">The path to the level.</param>
        public static void AddLevel(this scnCLS screen, string path)
        {
            // Setup object
            GameObject gameObject = screen.CreateTile(screen.gemTopY);

            // Setup data or return if it can't
            LevelDataCLS data = new();
            if (!data.TrySetup(path))
                return;

            // Setup tile
            CustomLevelTile tile = gameObject.GetComponent<CustomLevelTile>();
            tile.Setup("Custom:" + path.Split(Path.DirectorySeparatorChar).Last(), data);

            gameObject.name = tile.levelKey;

            // Load tile
            screen.LoadTile(path, tile, data);

            // Move top gem
            screen.gemTop.MoveY(++screen.gemTopY);
        }

        /// <summary>
        /// Add a custom pack to a <see cref="scnCLS"/>.
        /// </summary>
        /// <param name="screen">The <see cref="scnCLS"/>.</param>
        /// <param name="path">The path to the pack.</param>
        public static void AddPack(this scnCLS screen, string path)
        {
            // Setup object
            GameObject gameObject = screen.CreateTile(screen.gemTopY);

            // Read pack config or return if it can't
            Dictionary<string, string> packConfig = CustomConfig.Read(Directory.GetFiles(path, "main.pack").First());
            if (packConfig is null)
                return;

            // Setup data
            FolderDataCLS data = packConfig.AsPackData();

            // Setup tile
            CustomLevelTile tile = gameObject.GetComponent<CustomLevelTile>();
            tile.Setup("Custom:" + path.Split(Path.DirectorySeparatorChar).Last(), data);

            // Add levels and packs
            foreach (string directory in Directory.GetDirectories(path))
            {
                // Add child pack
                if (Directory.GetFiles(directory, "main.pack").Length > 0)
                    screen.AddChildPack(directory, tile.levelKey);

                // Add child directory
                else if (Directory.GetFiles(directory, "main.adofai").Length > 0)
                    screen.AddChildLevel(directory, tile.levelKey, data);
            }

            gameObject.name = tile.levelKey;

            // Load tile
            screen.LoadTile(path, tile, data);

            // Move top gem
            screen.gemTop.MoveY(++screen.gemTopY);
        }

        /// <summary>
        /// Add a custom level to a pack in a <see cref="scnCLS"/>.
        /// </summary>
        /// <param name="screen">The <see cref="scnCLS"/>.</param>
        /// <param name="path">The path to the level.</param>
        /// <param name="packKey">The parent pack's key.</param>
        /// <param name="packData">The <see cref="FolderDataCLS"/>.</param>
        /// <param name="depth">The depth of the child level. E.g. if the level is in a pack which is in another pack (two packs deep), the depth would be 2.</param>
        public static void AddChildLevel(this scnCLS screen, string path, string packKey, FolderDataCLS packData, int depth = 1)
        {
            if (depth < 1)
                depth = 1;

            // Setup object
            GameObject gameObject = screen.CreateTile(int.MaxValue);

            // Setup data or return if it can't
            LevelDataCLS data = new();
            if (!data.TrySetup(path))
                return;

            data.parentFolderName = packKey;

            // Setup tile
            CustomLevelTile tile = gameObject.GetComponent<CustomLevelTile>();
            string[] dirs = path.Split(Path.DirectorySeparatorChar);
            tile.Setup("Custom:" + string.Join(Path.DirectorySeparatorChar.ToString(), dirs.Skip(dirs.Length - (depth + 1)).Take(depth + 1)), data);

            gameObject.name = tile.levelKey;

            // Load tile
            packData.containingLevels[tile.levelKey] = data;
            screen.LoadTile(path, tile, data);
        }

        /// <summary>
        /// Add a custom pack to a pack in a <see cref="scnCLS"/>.
        /// </summary>
        /// <param name="screen">The <see cref="scnCLS"/>.</param>
        /// <param name="path">The path to the level.</param>
        /// <param name="packKey">The parent pack's key.</param>
        /// <param name="depth">The depth of the child level. E.g. if the level is in a pack which is in another pack (two packs deep), the depth would be 2.</param>
        public static void AddChildPack(this scnCLS screen, string path, string packKey, int depth = 1)
        {
            if (depth < 1)
                depth = 1;

            // Setup object
            GameObject gameObject = screen.CreateTile(int.MaxValue);

            // Read pack config or return if it can't
            Dictionary<string, string> packConfig = CustomConfig.Read(Directory.GetFiles(path, "main.pack").First());
            if (packConfig is null)
                return;

            // Setup data
            FolderDataCLS data = packConfig.AsPackData();
            data.parentFolderName = packKey;

            // Setup tile
            CustomLevelTile tile = gameObject.GetComponent<CustomLevelTile>();
            string[] dirs = path.Split(Path.DirectorySeparatorChar);
            tile.Setup("Custom:" + string.Join(Path.DirectorySeparatorChar.ToString(), dirs.Skip(dirs.Length - (depth + 1)).Take(depth + 1)), data);

            // Add levels and packs
            foreach (string directory in Directory.GetDirectories(path))
            {
                // Add child pack
                if (Directory.GetFiles(directory, "main.pack").Length > 0)
                    screen.AddChildPack(directory, tile.levelKey, depth + 1);

                // Add child level
                else if (Directory.GetFiles(directory, "main.adofai").Length > 0)
                    screen.AddChildLevel(directory, tile.levelKey, data, depth + 1);
            }

            gameObject.name = tile.levelKey;

            // Load tile
            screen.LoadTile(path, tile, data);
        }
    }
}
