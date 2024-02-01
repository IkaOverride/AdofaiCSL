using ADOFAI;
using GDMiniJSON;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AdofaiCSL.API {

    public static class CLSExtensions {

        public static GameObject SetupTileObject(this scnCLS screen, float y) {

            // Instantiate the tile object
            GameObject gameObject = Object.Instantiate(screen.tilePrefab, screen.floorContainer);
            gameObject.name = "CustomTile";
            gameObject.transform.LocalMoveY(y);

            // Setup the floor
            scrFloor floor = gameObject.GetComponent<scrFloor>();
            floor.topGlow.gameObject.SetActive(true);
            floor.isLandable = true;

            return gameObject;
        }

        /// <summary>
        /// Add a custom level to the custom level screen.
        /// </summary>
        /// <param name="screen">The <see cref="scnCLS"/>.</param>
        /// <param name="path">The path to the level.</param>
        /// <param name="y">The vertical offset of the level.</param>
        public static void AddCustomLevel(this scnCLS screen, string path) {

            // Setup object
            GameObject gameObject = screen.SetupTileObject(screen.gemTopY);

            // Setup data
            LevelDataCLS data = new LevelDataCLS();
            data.Setup();
            if (data.Decode(Json.DeserializePartially(RDFile.ReadAllText($"{path}{Path.DirectorySeparatorChar}main.adofai"), "actions") as Dictionary<string, object>))
                screen.loadedLevels.Add(path.Split(Path.DirectorySeparatorChar).Last(), data);

            // Setup tile
            CustomLevelTile tile = gameObject.GetComponent<CustomLevelTile>();
            tile.levelKey = path.Split(Path.DirectorySeparatorChar).Last();
            tile.title.text = Regex.Replace(data.title, @"<[^>]+>| ", "").Trim();
            tile.artist.text = Regex.Replace(data.artist, @"<[^>]+>| ", "").Trim();
            tile.image.enabled = data.previewIcon.Any();

            // Load audio
            ADOBase.audioManager.FindOrLoadAudioClipExternal(Path.Combine(path, data.songFilename), false);

            // Load tile
            screen.sortedLevelKeys.Add(tile.levelKey);
            screen.loadedLevelTiles[tile.levelKey] = tile;
            screen.loadedLevelDirs[tile.levelKey] = path;
            screen.loadedLevelIsDeleted[tile.levelKey] = false;

            // Move top gem
            screen.gemTop.MoveY(++screen.gemTopY);
        }

        /// <summary>
        /// Add a custom pack to the custom level screen.
        /// </summary>
        /// <param name="screen">The <see cref="scnCLS"/>.</param>
        /// <param name="path">The path to the pack.</param>
        /// <param name="y">The vertical offset of the pack.</param>
        public static void AddCustomPack(this scnCLS screen, string path) {

            // Setup object
            GameObject gameObject = screen.SetupTileObject(screen.gemTopY);

            // Read pack config
            Dictionary<string, string> packConfig = FileUtil.ReadCustomConfig(Directory.GetFiles(path, "*.pack").First());

            string title = packConfig.ContainsKey("title") ? packConfig["title"] : "Unknown";
            string artist = packConfig.ContainsKey("artist") ? packConfig["artist"] : "Unknown";
            string author = packConfig.ContainsKey("author") ? packConfig["author"] : "Unknown";
            string description = packConfig.ContainsKey("description") ? packConfig["description"] : "Unknown";
            string image = (packConfig.ContainsKey("image") && File.Exists($"{path}{Path.DirectorySeparatorChar}{packConfig["image"]}")) ? packConfig["image"].Trim() : "";
            string icon = (packConfig.ContainsKey("icon") && File.Exists($"{path}{Path.DirectorySeparatorChar}{packConfig["icon"]}")) ? packConfig["icon"].Trim() : "";
            Color color = packConfig.ContainsKey("color") ? packConfig["color"].HexToColor() : Color.black;

            if (!packConfig.ContainsKey("difficulty") || !int.TryParse(packConfig["difficulty"], out int difficulty))
                difficulty = 1;

            // Setup data
            FolderDataCLS packData = new FolderDataCLS(
                title,
                difficulty,
                artist,
                author,
                description,
                image,
                icon,
                color
            );

            // Setup tile
            CustomLevelTile tile = gameObject.GetComponent<CustomLevelTile>();
            tile.levelKey = $"CustomFolder:{path.Split(Path.DirectorySeparatorChar).Last()}";
            tile.title.text = packConfig.ContainsKey("title") ? packConfig["title"] : "Unknown";
            tile.artist.text = packConfig.ContainsKey("artist") ? packConfig["artist"] : "Unknown";
            tile.image.enabled = packConfig.ContainsKey("image");

            // Add levels in pack
            foreach (string levelPath in Directory.GetDirectories(path).Where(levelPath => Directory.GetFiles(levelPath, "main.adofai").Length > 0))
                screen.AddCustomLevelToPack(levelPath, tile.levelKey, packData);

            // Load tile
            screen.sortedLevelKeys.Add(tile.levelKey);
            screen.loadedLevels[tile.levelKey] = packData;
            screen.loadedLevelTiles[tile.levelKey] = tile;
            screen.loadedLevelDirs[tile.levelKey] = path;
            screen.loadedLevelIsDeleted[tile.levelKey] = false;

            // Move top gem
            screen.gemTop.MoveY(++screen.gemTopY);
        }

        /// <summary>
        /// Add a custom level to a pack in the custom level screen.
        /// </summary>
        /// <param name="screen">The <see cref="scnCLS"/>.</param>
        /// <param name="packData">The <see cref="FolderDataCLS"/>.</param>
        /// <param name="path">The path to the level.</param>
        public static void AddCustomLevelToPack(this scnCLS screen, string path, string packKey, FolderDataCLS packData) {

            // Setup object
            GameObject gameObject = screen.SetupTileObject(int.MaxValue);

            // Setup data
            LevelDataCLS data = new LevelDataCLS();
            data.Setup();
            if (data.Decode(Json.DeserializePartially(RDFile.ReadAllText($"{path}{Path.DirectorySeparatorChar}main.adofai"), "actions") as Dictionary<string, object>)) {
                data.parentFolderName = packKey;
                screen.loadedLevels.Add(path.Split(Path.DirectorySeparatorChar).Last(), data);
            }

            // Setup tile
            CustomLevelTile tile = gameObject.GetComponent<CustomLevelTile>();
            tile.levelKey = path.Split(Path.DirectorySeparatorChar).Last();
            tile.title.text = Regex.Replace(data.title, @"<[^>]+>| ", "").Trim();
            tile.artist.text = Regex.Replace(data.artist, @"<[^>]+>| ", "").Trim();
            tile.image.enabled = data.previewIcon.Any();

            // Load tile
            packData.containingLevels[tile.levelKey] = data;
            screen.sortedLevelKeys.Add(tile.levelKey);
            screen.loadedLevelTiles[tile.levelKey] = tile;
            screen.loadedLevelDirs[tile.levelKey] = path;
            screen.loadedLevelIsDeleted[tile.levelKey] = false;
        }
    }
}
