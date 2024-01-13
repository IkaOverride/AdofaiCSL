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

        /// <summary>
        /// Add a custom level to the custom level screen.
        /// </summary>
        /// <param name="screen">The <see cref="scnCLS"/>.</param>
        /// <param name="path">The path to the level.</param>
        /// <param name="y">The vertical offset of the level.</param>
        public static void AddCustomLevel(this scnCLS screen, string path, float y) {
            
            // Add tile
            GameObject levelObject = Object.Instantiate(screen.tilePrefab, screen.floorContainer);
            levelObject.name = "CustomTile";
            levelObject.transform.LocalMoveY(y);

            scrFloor levelFloor = levelObject.GetComponent<scrFloor>();
            levelFloor.topGlow.gameObject.SetActive(true);
            levelFloor.isLandable = true;

            // Add level
            LevelDataCLS levelData = new LevelDataCLS();
            levelData.Setup();
            if (levelData.Decode(Json.DeserializePartially(RDFile.ReadAllText($"{path}{Path.DirectorySeparatorChar}main.adofai"), "actions") as Dictionary<string, object>))
                screen.loadedLevels.Add(path.Split(Path.DirectorySeparatorChar).Last(), levelData);

            // Configure tile
            CustomLevelTile levelTile = levelObject.GetComponent<CustomLevelTile>();
            levelTile.levelKey = path.Split(Path.DirectorySeparatorChar).Last();
            levelTile.title.text = Regex.Replace(levelData.title, @"<[^>]+>| ", "").Trim();
            levelTile.artist.text = Regex.Replace(levelData.artist, @"<[^>]+>| ", "").Trim();
            levelTile.image.enabled = levelData.previewIcon.Any();

            // Add song
            ADOBase.audioManager.FindOrLoadAudioClipExternal(Path.Combine(path, levelData.songFilename), false);

            screen.sortedLevelKeys.Add(levelTile.levelKey);
            screen.loadedLevelTiles.Add(levelTile.levelKey, levelTile);
            screen.loadedLevelDirs.Add(levelTile.levelKey, path);

            // This fixes refresh issue :)
            if (!screen.loadedLevelIsDeleted.ContainsKey(levelTile.levelKey))
                screen.loadedLevelIsDeleted.Add(levelTile.levelKey, false);
        }

        /// <summary>
        /// Add a custom pack to the custom level screen.
        /// </summary>
        /// <param name="screen">The <see cref="scnCLS"/>.</param>
        /// <param name="path">The path to the pack.</param>
        /// <param name="y">The vertical offset of the pack.</param>
        public static void AddCustomPack(this scnCLS screen, string path, float y) {

            // Add tile
            GameObject packObject = Object.Instantiate(screen.tilePrefab, screen.floorContainer);
            packObject.name = "CustomTile";
            packObject.transform.LocalMoveY(y);

            scrFloor packFloor = packObject.GetComponent<scrFloor>();
            packFloor.topGlow.gameObject.SetActive(true);
            packFloor.isLandable = true;

            // Configure tile
            Dictionary<string, string> packConfig = FileUtil.ReadCustomConfig(Directory.GetFiles(path, "*.pack").First());

            CustomLevelTile packTile = packObject.GetComponent<CustomLevelTile>();
            packTile.levelKey = $"CustomFolder:{path.Split(Path.DirectorySeparatorChar).Last()}";
            packTile.title.text = packConfig["title"];
            packTile.artist.text = packConfig["artist"];
            packTile.image.enabled = packConfig.ContainsKey("image");

            string image = packConfig.ContainsKey("image") ? packConfig["image"] : "";
            string icon = packConfig.ContainsKey("icon") ? packConfig["icon"] : "";

            FolderDataCLS packData = new FolderDataCLS(
                packConfig["title"],
                int.Parse(packConfig["difficulty"]),
                packConfig["artist"],
                packConfig["author"],
                packConfig["description"],
                image,
                icon,
                packConfig["color"].HexToColor()
            );

            foreach (string levelPath in Directory.GetDirectories(path))
                screen.AddCustomLevelToPack(packData, levelPath);
        }

        /// <summary>
        /// Add a custom level to a pack in the custom level screen.
        /// </summary>
        /// <param name="screen">The <see cref="scnCLS"/>.</param>
        /// <param name="packData">The <see cref="FolderDataCLS"/>.</param>
        /// <param name="path">The path to the level.</param>
        public static void AddCustomLevelToPack(this scnCLS screen, FolderDataCLS packData, string path) {

            // Add tile
            GameObject levelObject = Object.Instantiate(screen.tilePrefab, screen.floorContainer);
            levelObject.name = "CustomTile";
            levelObject.transform.LocalMoveY(int.MaxValue); // move far from screen

            scrFloor levelFloor = levelObject.GetComponent<scrFloor>();
            levelFloor.GetComponent<scrFloor>().topGlow.gameObject.SetActive(true);
            levelFloor.GetComponent<scrFloor>().isLandable = true;

            // Add level
            LevelDataCLS levelData = new LevelDataCLS();
            levelData.Setup();
            if (levelData.Decode(Json.DeserializePartially(RDFile.ReadAllText($"{path}{Path.DirectorySeparatorChar}main.adofai"), "actions") as Dictionary<string, object>)) {
                levelData.parentFolderName = (string) packData["title"];
                screen.loadedLevels.Add(path.Split(Path.DirectorySeparatorChar).Last(), levelData);
            }

            // Configure tile
            CustomLevelTile levelTile = levelObject.GetComponent<CustomLevelTile>();
            levelTile.levelKey = path.Split(Path.DirectorySeparatorChar).Last();
            levelTile.title.text = Regex.Replace(levelData.title, @"<[^>]+>| ", "").Trim();
            levelTile.artist.text = Regex.Replace(levelData.artist, @"<[^>]+>| ", "").Trim();
            levelTile.image.enabled = levelData.previewIcon.Any();

            packData.containingLevels.Add(levelTile.levelKey, levelData);

            screen.sortedLevelKeys.Add(levelTile.levelKey);
            screen.loadedLevelTiles.Add(levelTile.levelKey, levelTile);
            screen.loadedLevelDirs.Add(levelTile.levelKey, path);

            // This fixes refresh issue :)
            if (!screen.loadedLevelIsDeleted.ContainsKey(levelTile.levelKey))
                screen.loadedLevelIsDeleted.Add(levelTile.levelKey, false);
        }
    }
}
